// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace TcpSyntheticCounters
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq.Charting;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Windows.Forms;
    using Tx.Windows;

    public partial class PerfomanceCounterUI : Form
    {
        IDisposable _subscription;
        Playback _playback;
        Chart _chart;
        Dictionary<string, FastLine> _trends = new Dictionary<string, FastLine>();
        private static readonly string[] CounterPaths = new[] {@"\Processor(_Total)\% Processor Time"};
        private static readonly string PerfCounterSessionName = "prcpers";

        public PerfomanceCounterUI()
        {
            InitializeComponent();
            InitChart();

            this.Controls.Add(_chart);
        }

        private void InitChart()
        {
            var area = new ChartArea();
            area.AxisX.Title = "Time";
            area.AxisX.TitleFont = new Font(area.AxisX.TitleFont, FontStyle.Bold);
            area.AxisX.ArrowStyle = System.Windows.Forms.DataVisualization.Charting.AxisArrowStyle.Triangle;

            area.AxisY.Title = "Value";
            area.AxisY.TitleFont = new Font(area.AxisY.TitleFont, FontStyle.Bold);
            area.AxisY.ArrowStyle = System.Windows.Forms.DataVisualization.Charting.AxisArrowStyle.Triangle;

            _chart = new Chart
            {
                ChartAreas = { area },
                Dock = DockStyle.Fill,
            };
        }

        private void TcpSyntheticCounters_Load(object sender, EventArgs e)
        {
            _playback = new Playback();
            _playback.AddRealTimeSession(PerfCounterSessionName);

            IObservable<PerformanceSample> perfCounters = PerfCounterObservable.FromRealTime(TimeSpan.FromSeconds(1), CounterPaths);
            _subscription = perfCounters.ObserveOn(_chart).Subscribe(CounterAdded);

            _playback.Start();
        }

        private void CounterAdded(PerformanceSample counter)
        {
            _chart.BeginInit();

            AddPoint(counter.Value, counter.CounterName);

            _chart.Invalidate();
            _chart.EndInit();
        }

        private void AddPoint(double value, string counterName)
        {
            FastLine trend;
            if (!_trends.TryGetValue(counterName, out trend))
            {
                trend = new FastLine { LegendText = counterName };
                _trends.Add(counterName, trend);
                _chart.BaseSeries.Add(trend);
            }

            trend.Add(DateTime.Now.ToLongTimeString(), new FastLine.DataPoint(value));
        }

    }
}
