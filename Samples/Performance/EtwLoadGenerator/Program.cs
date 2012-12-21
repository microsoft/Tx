using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace EtwLoadGenerator
{
    class Program
    {
        static InputGenerator _generator;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(
@"Usage: EtwLoadGenerator <size>

    size is Small, Medium or Large");
                Environment.Exit(1);
            }

            EventSize size;
            if (!Enum.TryParse(args[0], out size))
            {
                Console.WriteLine("Unknown event size " + args[0]);
                Environment.Exit(1);
            }

            Console.WriteLine("Generating {0} events", size);
            Console.WriteLine();
            _generator = new EventWriteInputGenerator(size);

            while (true)
            {
                Console.Write("Events in Burst:");
                int eventsInBurst = int.Parse(Console.ReadLine());

                Console.Write("Sleep in milliseconds between bursts:");
                int sleepMilliseconds = int.Parse(Console.ReadLine());

                double estimatedFrequency = 1000.0 * eventsInBurst / sleepMilliseconds;
                Console.WriteLine("Target sustained rate: {0:n}", estimatedFrequency);

                Console.WriteLine();

                GenerateLoad(eventsInBurst, sleepMilliseconds);
            }
        }

        static void GenerateLoad(int eventsInBurst, int sleepMilliseconds)
        {
            long counter = 0;
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (!Console.KeyAvailable)
            {
                _generator.Generate(eventsInBurst);

                if (sleepMilliseconds > 0)
                {
                    Thread.Sleep(sleepMilliseconds);
                }
            }
            Console.ReadKey();
        }
    }
}
