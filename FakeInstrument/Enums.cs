using System;
using System.Collections.Generic;
using System.Text;

namespace FakeInstrument
{
    public enum Command
    {
        Ping,
        Status,
        StartSpin,
        Spinning,
        SetTemperature,
        GetTemperature,
        GetExperiments,
        StartExperiment,
        ExperimentStatus
    };
}
