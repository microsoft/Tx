using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Reactive.Tx;
using Microsoft.Etw;
using Microsoft.Etw.WcfInterception;

namespace OutputUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static readonly Guid ProviderId = new Guid("83093276-1f35-45a2-8b19-6964cc85c70f");
        Playback _pb;
        DateTimeOffset _lastSample = DateTimeOffset.MinValue;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs ev)
        {
            canvas1.LayoutTransform = new MatrixTransform(1, 0, 0, -1, 0, canvas1.Height);

            Process logman = Process.Start("logman.exe", "create trace wcf -rt -p {" + ProviderId + "} -ets");
            logman.WaitForExit();

            _pb = new Playback();
            _pb.AddRealTimeSession("wcf");

            var start = _pb.GetObservable<StartRequest>();
            var end = _pb.GetObservable<EndRequest>();

            var requests = from s in start 
                           from e in end.Where(e=>e.requestMessageId == s.requestMessageId).Take(1)
                           select new 
                {
                    StartTime = s.Header.Timestamp,
                    Operation = s.operationName,
                    Duration = e.Header.Timestamp - s.Header.Timestamp
                };

            var bars = from r in requests
                group r by new
                {
                    Duration = Math.Ceiling(r.Duration.TotalMilliseconds),
                    Operation = r.Operation
                }
                into groups
                from window in groups.Window(TimeSpan.FromSeconds(2), _pb.Scheduler)
                from c in window.Count()
                select new HistogramBar
                {
                    Operation = groups.Key.Operation,
                    Duration = groups.Key.Duration,
                    Count = c,
                };

            var histogram = bars.Buffer(TimeSpan.FromSeconds(2));

            histogram.ObserveOn(Dispatcher).Subscribe(DrawHistogram);

            _pb.Start();
        }

        void DrawHistogram(IList<HistogramBar> histogram)
        {
            canvas1.Children.Clear();
            SortedDictionary<string, MethodStatistics> stats = new SortedDictionary<string, MethodStatistics>();

            foreach (HistogramBar h in histogram)
            {
                if (!stats.ContainsKey(h.Operation))
                {
                    stats.Add(h.Operation, new MethodStatistics
                    {
                         MethodName = h.Operation,
                          TotalCount = 1,
                          TotalDuration = h.Duration
                    });
                }
                else
                {
                    MethodStatistics ms = stats[h.Operation];
                    ms.TotalCount += 1;
                    ms.TotalDuration += h.Duration;
                }

                if (h.Operation != (string)HistogramFilter.Content)
                    continue; 
                
                Rectangle r = new Rectangle();
                r.Fill = new SolidColorBrush(Colors.Red);
                r.Height = h.Count * canvas1.Height / 10;
                r.Width = canvas1.Width / 50;
                Canvas.SetLeft(r, canvas1.Width / 50 * Math.Floor(h.Duration));
                Canvas.SetTop(r, 0);
                canvas1.Children.Insert(0, r);
            }

            MethodStatisticsGrid.ItemsSource = stats.Values;
        }

        private void MethodStatisticsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MethodStatistics selected = (MethodStatistics)MethodStatisticsGrid.SelectedItem;

            if (selected != null)
            {
                HistogramFilter.Content = selected.MethodName;
            }
        }

        public class MethodStatistics
        {
            public string MethodName { get; set; }
            public long   TotalCount { get; set; }
            public double TotalDuration { get; set; }
            public double AverageDuration { get { return TotalDuration / TotalCount; } }
        }

        public class HistogramBar
        {
            public string Operation;
            public double Duration;
            public long Count;
        }
    }
}
