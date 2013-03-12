// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Charting;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.Principal;
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

        Dictionary<string, FastLine> _trends = new Dictionary<string, FastLine>();
        DateTime _start = DateTime.Now;
       
        public TcpSyntheticCounters()
        {
            InitializeComponent();

            _received = new Column { Points = { }, LegendText = "Received" };
            _send = new Column { Points = { }, LegendText = "Send" };
            _chart = new Chart
            {
                ChartAreas = { new ChartArea() },
                Dock = DockStyle.Fill, 
            };
            this.Controls.Add(_chart);
        }

        private void TcpSyntheticCounters_Load(object sender, EventArgs e)
        {
            StartSession(SessionName, ProviderId);

            _playback = new Playback();
            _playback.AddRealTimeSession("tcp");

            var received = _playback.GetObservable<KNetEvt_RecvIPV4>();

            var x = from window in received.Window(TimeSpan.FromSeconds(1), _playback.Scheduler)
                    from stats in
                        (   from packet in window
                            group packet by packet.daddr into g
                            from total in g.Sum(p=>p.size)
                            select new
                            {
                                address = new IPAddress(g.Key).ToString(),
                                received = total
                            })
                            .ToList()
                    select stats.OrderBy(s=>s.address);

            _subscription = x.ObserveOn(_chart).Subscribe(v =>
            {
                _chart.BeginInit();

                foreach (var point in v)
                    AddPoint(point.address, point.received);

               _chart.Invalidate();
               _chart.EndInit();
            });

            _playback.Start();
        }

        void AddPoint(string address, double received)
        {
            FastLine trend;
            if (!_trends.TryGetValue(address, out trend))
            {
                trend = new FastLine{ LegendText=address };
                _trends.Add(address, trend);
                _chart.BaseSeries.Add(trend);
            }
            var x = DateTime.Now.Subtract(_start).TotalMinutes;
            trend.Add(x, new FastLine.DataPoint(received));
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
}
