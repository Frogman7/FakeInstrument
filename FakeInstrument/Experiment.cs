using System;
using System.Xml.Serialization;

namespace FakeInstrument
{
    public struct Experiment
    {
        private const string defaultName = "None";

        private string name;

        private int timeToComplete;

        [XmlAttribute]
        public int TimeToComplete
        {
            get => this.timeToComplete;
            set
            {
                if (value < 0)
                {
                    throw new InvalidOperationException("Experiment time to complete must be greater than 0");
                }

                this.timeToComplete = value;
            }
        }

        [XmlAttribute]
        public string Name
        {
            get => string.IsNullOrEmpty(this.name) ? defaultName : this.name;
            set
            {
                this.name = value;
            }
        }
    }
}
