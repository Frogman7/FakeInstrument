using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace FakeInstrument
{
    internal class Display
    {
        private const string CurrentStatusString = "Current Status: ";

        private const string TemperatureString = "Current Temperature: ";

        private const string ExperimentString = "Experiment: ";

        private const string ExperimentProgress = "Experiment Progress: ";

        private InstrumentState instrumentState;

        private Point statusCursorPosition;

        private Point temperatureCursorPosition;

        private Point experimentCursorPosition;

        private Point experimentProgressCursorPosition;

        private object refreshDisplayLock;

        public Display(InstrumentState instrumentState)
        {
            this.instrumentState = instrumentState;

            this.refreshDisplayLock = new object();

            this.Initialize();
        }

        public void Refresh()
        {
            lock (this.refreshDisplayLock)
            {
                SetConsoleCursor(statusCursorPosition);

                var statusString = instrumentState.Status.ToString();

                var padLength = Console.WindowWidth - Console.CursorLeft - statusString.Length;

                ConsoleWriteColor(statusString.PadRight(padLength), ConsoleColor.DarkYellow);

                SetConsoleCursor(temperatureCursorPosition);

                var temperatureString = instrumentState.Temperature.ToString("#.#") + "°C";

                padLength = Console.WindowWidth - Console.CursorLeft - temperatureString.Length;

                ConsoleWriteColor(temperatureString.PadRight(padLength), ConsoleColor.Red);

                SetConsoleCursor(experimentCursorPosition);

                var experimentString = this.instrumentState.Experiment.Name;

                padLength = Console.WindowWidth - Console.CursorLeft - experimentString.Length;

                ConsoleWriteColor(experimentString.PadRight(padLength), ConsoleColor.Blue);

                SetConsoleCursor(experimentProgressCursorPosition);

                var experimentPercentageString = ((int)this.instrumentState.ExperimentPercentage).ToString() + '%';

                padLength = Console.WindowWidth - Console.CursorLeft - experimentPercentageString.Length;

                ConsoleWriteColor(experimentPercentageString.PadRight(padLength), ConsoleColor.DarkGreen);
            }
        }

        private void Initialize()
        {
            ConsoleWriteColor(CurrentStatusString, ConsoleColor.White);

            statusCursorPosition = GetConsoleCursorPosition();

            ConsoleWriteColor(this.instrumentState.Status.ToString(), ConsoleColor.DarkYellow);

            Console.WriteLine();

            ConsoleWriteColor(TemperatureString, ConsoleColor.Green);

            temperatureCursorPosition = GetConsoleCursorPosition();

            ConsoleWriteColor(this.instrumentState.Temperature.ToString("#.#") + "°C", ConsoleColor.Red);

            Console.WriteLine();

            ConsoleWriteColor(ExperimentString, ConsoleColor.Cyan);

            experimentCursorPosition = GetConsoleCursorPosition();

            ConsoleWriteColor(this.instrumentState.Experiment.Name, ConsoleColor.Blue);

            Console.WriteLine();

            ConsoleWriteColor(ExperimentString, ConsoleColor.Magenta);

            experimentProgressCursorPosition = GetConsoleCursorPosition();

            ConsoleWriteColor("0%", ConsoleColor.DarkGreen);
        }

        private static void ConsoleWriteColor(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            var savedForeground = Console.ForegroundColor;
            var savedBackground = Console.BackgroundColor;

            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;

            Console.Write(text);

            Console.ForegroundColor = savedForeground;
            Console.BackgroundColor = savedBackground;
        }

        private static void SetConsoleCursor(Point point)
        {
            Console.CursorLeft = point.X;
            Console.CursorTop = point.Y;
        }

        private static Point GetConsoleCursorPosition()
        {
            return new Point(Console.CursorLeft, Console.CursorTop);
        }
    }
}
