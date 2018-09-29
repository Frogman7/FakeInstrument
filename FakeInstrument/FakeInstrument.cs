using FakeInstrument.Networking;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Serialization;

namespace FakeInstrument
{
    class FakeInstrument
    {
        private const int RefreshRate = 500;

        private const int MaxSpinSpeed = 5000;

        private TcpServer tcpServer;

        private InstrumentState instrumentState;

        private ICollection<Experiment> experiments;

        private Display display;

        private Timer updateTimer;

        private float targetTemperature;

        public FakeInstrument(ushort port)
        {
            this.tcpServer = new TcpServer(port);

            this.tcpServer.DataReceived += this.RequestReceived;

            this.instrumentState = new InstrumentState();

            this.experiments = this.DeserializeExperimentsFromFile();

            this.display = new Display(this.instrumentState);

            this.targetTemperature = instrumentState.Temperature;

            this.updateTimer = new Timer(RefreshRate);

            this.updateTimer.Elapsed += this.UpdateInstrument;
        }

        public void Start()
        {
            this.tcpServer.Start();

            this.updateTimer.Start();
        }

        public void Stop()
        {
            this.tcpServer.Stop();

            this.updateTimer.Stop();
        }

        protected void RequestReceived(byte[] dataReceived, TcpClient tcpClient)
        {
            try
            {
                var request = Request.ParseRequest(dataReceived);

                switch (request.Command)
                {
                    case Command.Ping:
                        {
                            this.SendResponse("ping", request.Parameters, tcpClient.Client);
                            break;
                        }
                    case Command.Status:
                        {
                            this.SendResponse("status", this.instrumentState.Status.ToString(), tcpClient.Client);
                            break;
                        }
                    case Command.StartSpin:
                        {
                            int spinSpeed = int.Parse(request.Parameters);

                            if (this.instrumentState.Status > InstrumentState.InstrumentStatus.Spinning)
                            {
                                this.SendError($"Spin is only allowed in the '{InstrumentState.InstrumentStatus.Idle}' or '{InstrumentState.InstrumentStatus.Spinning}' states", tcpClient.Client);
                            }
                            else if (spinSpeed >= 0)
                            {
                                if (spinSpeed <= MaxSpinSpeed)
                                {
                                    this.instrumentState.SpinSpeed = spinSpeed;

                                    this.instrumentState.Status = (spinSpeed > 0) ? InstrumentState.InstrumentStatus.Spinning : InstrumentState.InstrumentStatus.Idle;

                                    this.SendResponse("spinning", spinSpeed.ToString(), tcpClient.Client);
                                }
                                else
                                {
                                    this.instrumentState.Status = InstrumentState.InstrumentStatus.Error;
                                }
                            }
                            else
                            {
                                this.SendError("Spin speed cannot be negative", tcpClient.Client);
                            }

                            break;
                        }
                    case Command.Spinning:
                        {
                            this.SendResponse("spinning", this.instrumentState.SpinSpeed.ToString(), tcpClient.Client);

                            break;
                        }
                    case Command.SetTemperature:
                        {
                            if (float.TryParse(request.Parameters, out var setTemperature))
                            {
                                this.targetTemperature = setTemperature;

                                this.SendResponse("temperature", this.instrumentState.Temperature.ToString("#.#"), tcpClient.Client);
                            }
                            else
                            {
                                this.SendError($"Could not parse {request.Parameters} as a floating point integer", tcpClient.Client);
                            }

                            break;
                        }
                    case Command.GetTemperature:
                        {
                            this.SendResponse("temperature", this.instrumentState.Temperature.ToString("#.#"), tcpClient.Client);
                            break;
                        }
                    case Command.GetExperiments:
                        {
                            this.SendResponse("experiments", string.Join(',', this.experiments.Select(exp => exp.Name)), tcpClient.Client);
                            break;
                        }
                    case Command.StartExperiment:
                        {
                            if (this.instrumentState.Status != InstrumentState.InstrumentStatus.Idle)
                            {
                                this.SendError("Experiments can only be started in the instrument idle state", tcpClient.Client);
                            }

                            this.instrumentState.Experiment = this.experiments.First(exp => exp.Name.Equals(request.Parameters, StringComparison.InvariantCultureIgnoreCase));

                            this.instrumentState.ExperimentPercentage = 0;
                            this.instrumentState.ExperimentRunning = true;
                            this.instrumentState.Status = InstrumentState.InstrumentStatus.Running;

                            this.SendResponse("experimentstarted", this.instrumentState.Experiment.Name, tcpClient.Client);

                            break;
                        }
                    case Command.ExperimentStatus:
                        {
                            this.SendResponse("experimentstatus", ((int)this.instrumentState.ExperimentPercentage).ToString(), tcpClient.Client);

                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                this.SendError(exception.Message, tcpClient.Client);
            }
        }

        private void UpdateInstrument(object sender, ElapsedEventArgs eventArgs)
        {
            if (this.targetTemperature != this.instrumentState.Temperature)
            {
                this.UpdateTemperature();
            }

            if (this.instrumentState.ExperimentRunning)
            {
                // Math doesn't perfectly check out but close enough for now...
                var percentageChange = (RefreshRate / (float)(this.instrumentState.Experiment.TimeToComplete * 1000)) * 100;

                if ((this.instrumentState.ExperimentPercentage + percentageChange) > 100)
                {
                    this.instrumentState.ExperimentPercentage = 100;
                }
                else
                {
                    this.instrumentState.ExperimentPercentage += percentageChange;
                }

                if ((int)this.instrumentState.ExperimentPercentage == 100)
                {
                    this.instrumentState.Status = InstrumentState.InstrumentStatus.Idle;
                    this.instrumentState.ExperimentRunning = false;
                }
            }

            this.display.Refresh();
        }

        private void UpdateTemperature()
        {
            var randomNumberGenerator = new Random();

            if (this.targetTemperature > this.instrumentState.Temperature)
            {
                this.instrumentState.Temperature += ((float)randomNumberGenerator.NextDouble()) + 1;

                if (this.instrumentState.Temperature > this.targetTemperature)
                {
                    this.instrumentState.Temperature = this.targetTemperature;
                }
            }
            else
            {
                this.instrumentState.Temperature -= ((float)randomNumberGenerator.NextDouble()) + 1;

                if (this.instrumentState.Temperature < this.targetTemperature)
                {
                    this.instrumentState.Temperature = this.targetTemperature;
                }
            }
        }

        private void SendResponse(string command, string parameters, Socket socket)
        {
            var bytes = Encoding.ASCII.GetBytes($"!{command} {parameters}");

            socket.Send(bytes);
        }

        private void SendError(string message, Socket socket)
        {
            var bytes = Encoding.ASCII.GetBytes("!error " + message);

            socket.Send(bytes);
        }

        private ICollection<Experiment> DeserializeExperimentsFromFile()
        {
            ICollection<Experiment> experiments = null;

            using (var fileReader = new FileStream("Experiments.xml", FileMode.Open, FileAccess.Read))
            {
                var xmlSerializer = new XmlSerializer(typeof(Collection<Experiment>), new XmlRootAttribute("Experiments"));

                experiments = (ICollection<Experiment>)xmlSerializer.Deserialize(fileReader);
            }

            return experiments;
        }
    }
}
