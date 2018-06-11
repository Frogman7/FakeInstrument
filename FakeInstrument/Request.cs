using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FakeInstrument
{
    class Request
    {
        public const string RequestRegexPattern = @"#(\w+)\s?(.+)?";

        public Command Command { get; }

        public string Parameters { get; }

        private static Regex requestRegex;

        static Request()
        {
            requestRegex = new Regex(RequestRegexPattern, RegexOptions.Compiled);
        }

        public static Request ParseRequest(byte[] requestBytes)
        {
            var requestString = Encoding.ASCII.GetString(requestBytes);

            var match = requestRegex.Match(requestString);

            if (!match.Success)
            {
                throw new InvalidOperationException($"Could not validate request \"{requestString}\"!");
            }

            if (!Enum.TryParse(typeof(Command), match.Groups[1].Value, true, out var command))
            {
                throw new ArgumentException("Command could not be resolved");
            }

            var parameter = string.Empty;

            if (match.Groups[2].Success)
            {
                parameter = match.Groups[2].Value;
            }

            return new Request((Command)command, parameter);
        }

        protected Request(Command command, string parameters)
        {
            this.Command = command;
            this.Parameters = parameters;
        }
    }
}
