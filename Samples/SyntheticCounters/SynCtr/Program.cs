using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.Principal;
using Tx.Windows;
using Tx.Windows.Microsoft_Windows_Kernel_Network;

namespace SynCtr
{
    class Program
    {
        const string SessionName = "tcp";
        static Guid ProviderId = new Guid("{7dd42a49-5329-4832-8dfd-43d979153a88}");
        static IDisposable _subscription;
        static Playback _playback;

        static void Main()
        {
            StartSession(SessionName, ProviderId);

            _playback = new Playback();
            _playback.AddRealTimeSession(SessionName);

            var received = from r in _playback.GetObservable<KNetEvt_RecvIPV4>()
                           select new PacketEvent { addr = r.daddr, received = r.size };

            var send = from r in _playback.GetObservable<KNetEvt_SendIPV4>()
                       select new PacketEvent { addr = r.daddr, send = r.size };

            var all = received.Merge(send);

            var x = from window in all.Window(TimeSpan.FromSeconds(1), _playback.Scheduler)
                    from stats in
                        (from packet in window
                         group packet by packet.addr into g
                         from aggregate in g.Aggregate(
                             new { send = 0.0, received = 0.0 },
                             (ac, p) => new { send = ac.send + p.send, received = ac.received + p.received })
                         select new
                         {
                             address = new IPAddress(g.Key).ToString(),
                             aggregate.received,
                             aggregate.send
                         })
                            .ToList()
                    select stats.OrderBy(s => s.address);

            _subscription = x.Subscribe(v =>
                {
                    Console.WriteLine("--- {0} ---", DateTime.Now);
                    foreach (var s in v)
                        Console.WriteLine("{0, -15} {1,-10:n0} ", s.address, s.received + s.send);
                    Console.WriteLine();
                });

            _playback.Start();
        }

        static void StartSession(string sessionName, Guid providerId)
        {
            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                throw new Exception("To use ETW real-time session you must be administrator");

            Process logman = Process.Start("logman.exe", "stop " + sessionName + " -ets");
            logman.WaitForExit();

            logman = Process.Start("logman.exe", "create trace " + sessionName + " -rt -nb 2 2 -bs 1024 -p {" + providerId + "} 0xffffffffffffffff -ets");
            logman.WaitForExit();
        }
    }

    class PacketEvent
    {
        public uint addr;
        public uint send;
        public uint received;
    }
}
