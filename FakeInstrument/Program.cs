using System;

namespace FakeInstrument
{
    class Program
    {
        static void Main(string[] args)
        {
            FakeInstrument fakeInstrument = new FakeInstrument(1987);

            fakeInstrument.Start();

            Console.CursorTop = Console.WindowHeight - 1;
            Console.CursorLeft = 0;

            Console.Write("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
