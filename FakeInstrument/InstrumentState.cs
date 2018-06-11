using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FakeInstrument
{
    class InstrumentState
    {
        public enum InstrumentStatus { Idle, Spinning, Running, Error };

        private const string NoExperiment = "None";

        private const int StartingTemperature = 23;

        private volatile InstrumentStatus status;

        private volatile int spinSpeed;

        private volatile float experimentPercentage;

        private volatile float temperature;

        private volatile bool experimentRunning;

        private Experiment experiment;

        public float Temperature { get => this.temperature; set => this.temperature = value; }

        public InstrumentStatus Status { get => this.status; set => this.status = value; }

        public int SpinSpeed { get => this.spinSpeed; set => this.spinSpeed = value; }

        public bool ExperimentRunning { get => this.experimentRunning; set => this.experimentRunning = value; }

        public Experiment Experiment
        {
            get => this.experiment;

            set
            {
                if (this.experimentRunning)
                {
                    throw new InvalidOperationException("Cannot set Experiment while an experiment is already running");
                }

                this.experiment = value;
            }
        }

        public float ExperimentPercentage
        {
            get => this.experimentPercentage;
            set
            {
                if (value < 0)
                {
                    throw new InvalidOperationException("Percentage cannot be less than 0");
                }
                else if (value > 100)
                {
                    throw new InvalidOperationException("Percentage cannot be greater than 100");
                }

                this.experimentPercentage = value;
            }
        }

        public InstrumentState()
        {
            this.ExperimentPercentage = 0;
            this.Temperature = StartingTemperature;
            this.Status = InstrumentStatus.Idle;

            this.experimentRunning = false;
        }
    }
}