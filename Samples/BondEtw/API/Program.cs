using System;
using System.Diagnostics;
using System.Reactive;
using System.Security.Principal;
using Tx.Bond;

namespace BondEtwSample
{
    class Program
    {
        static void Main(string[] args)
        {
            Write();

            Read();
        }

        static void Write()
        {
            StartEtw();

            var observer = new BinaryEtwObserver("Sample", new[] { typeof(Evt) }, TimeSpan.FromMinutes(1));

            for (int i = 0; i < 10; i++)
                observer.OnNext(new Evt { Time = DateTime.UtcNow.ToShortDateString(), Message = "iteration " + i });

            observer.OnCompleted();

            StopEtw();
        }

        static void Read()
        {
            Playback playback = new Playback();
            playback.AddBondEtlFiles(sessionName + ".etl");

            playback.GetObservable<Evt>()
                .Subscribe(e=> Console.WriteLine("{0}: {1}", e.Time, e.Message));

            playback.Run();
        }

        const string providerId = "{4f8f06bf-8261-4099-ae5f-07c54bbcfab3}";
        const string sessionName = "BondEtwSample";

        static void StartEtw()
        {
            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                throw new Exception("To enable ETW tracing you must be administrator");
            }

            StopEtw();

            var cmd = "create trace " + sessionName + " -nb 2 2 -bs 1024 -p " + providerId + "  0xffffffffffffffff -o " + sessionName + ".etl -ets";
            var logman = Process.Start("logman.exe", cmd);
            logman.WaitForExit();
        }

        static void StopEtw()
        {
            var logman = Process.Start("logman.exe", "stop " + sessionName + " -ets");
            logman.WaitForExit();
        }
    }
}
