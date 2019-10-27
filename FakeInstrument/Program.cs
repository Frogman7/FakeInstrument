using System;
using System.Linq;

namespace FakeInstrument
{
    class Program
    {
        const ushort DefaultTcpPort = 1987;

        static void Main(string[] args)
        {
            ushort tcpPort = DefaultTcpPort;

            if (args.Length > 0)
            {
                var portArg = args.FirstOrDefault(arg => arg.StartsWith("port="));

                if (portArg != null)
                {
                    string portAsString = portArg.Substring(portArg.IndexOf('=') + 1);

                    if (!ushort.TryParse(portAsString, out tcpPort))
                    {
                        Console.WriteLine($"Could not parse port argument '{portAsString}' into a valid port value");
                    }
                }
            }

            FakeInstrument fakeInstrument = new FakeInstrument(tcpPort);

            fakeInstrument.Start();

            Console.CursorTop = Console.WindowHeight - 1;
            Console.CursorLeft = 0;

            Console.Write("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
