using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq.Charting;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tx.Windows;
using Tx.Windows.Microsoft_Windows_Kernel_Network;

namespace TcpSyntheticCounters
{
    public partial class TcpSyntheticCounters : Form
    {
        const string SessionName = "tcp";
        Guid ProviderId = new Guid("{7dd42a49-5329-4832-8dfd-43d979153a88}");

        IDisposable _subscription;
        Playback _playback;
        Column _received;
        Column _send;
        Chart _chart;
        
        public TcpSyntheticCounters()
        {
            InitializeComponent();

            _received = new Column { Points = { }, LegendText = "Received" };
            _send = new Column { Points = { }, LegendText = "Send" };
            _chart = new Chart
            {
                ChartAreas = { new ChartArea { Series = { _received, _send } } }
            ,
                Dock = DockStyle.Fill, 
            };
            this.Controls.Add(_chart);
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

        private void TcpSyntheticCounters_Load(object sender, EventArgs e)
        {
            StartSession(SessionName, ProviderId);

            _playback = new Playback();
            _playback.AddRealTimeSession("tcp");

            var received = from r in _playback.GetObservable<KNetEvt_RecvIPV4>()
                           select new PacketEvent { addr = r.daddr, received = r.size };

            var send = from r in _playback.GetObservable<KNetEvt_SendIPV4>()
                           select new PacketEvent { addr = r.daddr, send = r.size };

            var all = received.Merge(send);

            var x = from window in all.Window(TimeSpan.FromSeconds(1), _playback.Scheduler)
                    from stats in
                        (   from packet in window
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
                    select stats.OrderBy(s=>s.address);

            _subscription = x.ObserveOn(_chart).Subscribe(v =>
            {
                _chart.BeginInit();
                _received.BasePoints.Clear();
                _send.BasePoints.Clear();

                foreach (var point in v)
                {
                    _received.Add(point.address, point.received);
                    _send.Add(point.address, point.send);
                }

               _chart.EndInit();
            });

            _playback.Start();
        }
    }

    class PacketEvent
    {
        public uint addr;
        public uint send;
        public uint received;
    }

}
