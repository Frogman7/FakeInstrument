using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace FakeInstrument.Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var ipEndpoint = new IPEndPoint(IPAddress.Loopback, 1987);

                var tcpClient = new TcpClient();

                int attempts = 10;

                while (attempts > 0)
                {
                    try
                    {
                        tcpClient.Connect(ipEndpoint);
                        attempts = 0;
                    }
                    catch (SocketException exception)
                    {
                        Console.WriteLine("Connection failed with message: " + exception.Message);
                        attempts--;
                    }
                }

                var response = SendAndGetReply("#ping test", tcpClient.Client);

                Console.WriteLine("Response from ping received: " + response);

                float targetTemperature = 50;

                response = SendAndGetReply("#settemperature " + targetTemperature, tcpClient.Client);

                Console.WriteLine("Response from set temperature received: " + response);

                float currentTemperature = ParseTemperatureFromResponse(response);

                Console.WriteLine("Pulling for target temperature: " + targetTemperature);

                while (currentTemperature != targetTemperature)
                {
                    Thread.Sleep(500);

                    response = SendAndGetReply("#gettemperature", tcpClient.Client);

                    currentTemperature = ParseTemperatureFromResponse(response);

                    Console.WriteLine("Current temperature is now: " + currentTemperature);
                }

                Console.WriteLine("Target temperature achieved, attempting spin");

                response = SendAndGetReply("#startspin 250", tcpClient.Client);

                Console.WriteLine("Response from start spin: " + response);

                Console.WriteLine("Now checking the instrument status");

                response = SendAndGetReply("#status", tcpClient.Client);

                Console.WriteLine("Status response was: " + response);

                Console.WriteLine("Now checking the spin speed");

                response = SendAndGetReply("#spinning", tcpClient.Client);

                Console.WriteLine("Spinning response was: " + response);

                Console.WriteLine("Stopping spin and checking status");

                response = SendAndGetReply("#startspin 0", tcpClient.Client);

                Console.WriteLine("Response of stop spin: " + response);

                response = SendAndGetReply("#status", tcpClient.Client);

                Console.WriteLine("Response of get status: " + response);

                Console.WriteLine("Now getting experiments");

                response = SendAndGetReply("#getexperiments", tcpClient.Client);

                Console.WriteLine("Response from get experiments: " + response);

                string experimentToStart = "FastExperiment";

                Console.WriteLine("Starting experiment: " + experimentToStart);

                response = SendAndGetReply("#startexperiment " + experimentToStart, tcpClient.Client);

                Console.WriteLine("Response from start experiment: " + response);

                int percentComplete = 0;

                while (percentComplete != 100)
                {
                    Thread.Sleep(500);

                    response = SendAndGetReply("#experimentstatus", tcpClient.Client);

                    percentComplete = ParsePercentageFromResponse(response);

                    Console.WriteLine("Current experiment completion is now: " + percentComplete);
                }

                Console.WriteLine("Experiment completed");
            }
            catch (Exception exception)
            {
                Console.WriteLine("Exception occurred: " + exception.Message);
            }

            Console.WriteLine();
            Console.WriteLine("All finished, press any key to exit...");
            Console.ReadKey();
        }

        private static string SendAndGetReply(string command, Socket socket)
        {
            var bytes = Encoding.ASCII.GetBytes(command);

            int bytesSent = socket.Send(bytes);

            var buffer = new byte[4096];

            int bytesReceivedCount = socket.Receive(buffer);

            return Encoding.ASCII.GetString(buffer, 0, bytesReceivedCount);
        }

        private static float ParseTemperatureFromResponse(string response)
        {
            float temperature = float.NaN;

            var regex = new Regex(@"!temperature\s(\d+(\.\d+)?)");

            var match = regex.Match(response);

            if (match.Success)
            {
                temperature = float.Parse(match.Groups[1].Value);
            }
            else
            {
                throw new InvalidOperationException("Could not validate response");
            }

            return temperature;
        }

        private static int ParsePercentageFromResponse(string response)
        {
            int percentage = -1;

            var regex = new Regex(@"!experimentstatus\s(\d+)");

            var match = regex.Match(response);

            if (match.Success)
            {
                percentage = int.Parse(match.Groups[1].Value);
            }
            else
            {
                throw new InvalidOperationException("Could not validate response");
            }

            return percentage;
        }
    }
}
