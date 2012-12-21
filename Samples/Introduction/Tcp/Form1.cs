using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.Principal;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Tx.Windows;
using Tx.Windows.Microsoft_Windows_Kernel_Network;

namespace Tcp
{
    public partial class Form1 : Form
    {
        const string SessionName = "tcp";
        Guid ProviderId = new Guid("{7dd42a49-5329-4832-8dfd-43d979153a88}");
        Dictionary<string, Series> _adresses = new Dictionary<string, Series>();
        DateTime _start = DateTime.Now;

        IDisposable subscription;

        public Form1()
        {
            InitializeComponent();
            chart1.Series.Clear();



        }

        private void Form1_Load(object sender, EventArgs e)
        {
            StartSession(SessionName, ProviderId);

            Playback playback = new Playback();
            playback.AddRealTimeSession("tcp");

            var c = from req in playback.GetObservable<KNetEvt_RecvIPV4>()
                        select new
                        {
                            Time = req.OccurenceTime,
                            Address = new IPAddress(req.daddr)
                        };

            subscription = c.ObserveOn(this).Subscribe(d =>
                {
                    double x = (d.Time - _start).TotalMinutes;
                    double y = d.Address.Address;
                    AddPoint(d.Address.ToString(), x, y);
                });

            playback.Start();
        }

        void AddPoint(string seriesName, double x, double y)
        {
            Series series;
            if (!_adresses.TryGetValue(seriesName, out series))
            {
                series = new Series { ChartType = SeriesChartType.FastPoint };
                series.Name = seriesName;
                series.Points.Clear();
                series.MarkerSize = 10;
                _adresses.Add(seriesName, series);
                chart1.Series.Add(series);

                series.Points.AddXY(x, y);
                series.Tag = y;
            }
            else
            {
                series.Points.AddXY(x, series.Tag);
            }
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
