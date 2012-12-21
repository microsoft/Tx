namespace EtwListener
{
    using System;
    using System.Diagnostics;
    using System.Reactive.Linq;
    using System.Reactive.Tx;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Microsoft.Etw;
    using Microsoft.Etw.Prototype_Eventing_Provider;

    class Program
    {
        static Guid _providerId = new Guid("3838EF9A-CB6F-4A1C-9033-84C0E8EBF5A7");

        static void Main(string[] args)
        {
            Process logman = Process.Start("logman.exe", "stop TxRealTime -ets");
            logman.WaitForExit();

            logman = Process.Start("logman.exe", "create trace TxRealTime -rt -nb 2 2 -bs 1024 -p {" + _providerId + "} -ets");
            logman.WaitForExit();

            if (args.Length == 0)
            {
                Console.WriteLine(
@"Usage: 
    EtwListener Tx Small - listen with Tx/Rx, Small events. Try also Medium and Large
    EtwListener Direct   - just count the events in the ETW callback (no Tx/Rx)");
                Environment.Exit(0);
            }

            if (args[0] == "Tx")
            {
                switch(args[1].ToLower())
                {
                    case "small":
                        ListenWithTx<SmallEvent>();
                        break;

                    case "medium":
                        ListenWithTx<MediumEvent>();
                        break;

                    case "large":
                        ListenWithTx<LargeEvent>();
                        break;

                        throw new Exception("Unknown event size " + args[1]);
                }
            }
            else
            {
                ListenDirect();
            }
            Console.ReadLine();
        }

        static void ListenWithTx<T>()
        {
            Console.WriteLine("listening for {0}", typeof(T).Name);
            Console.WriteLine();

            var pb = new Playback();
            pb.AddRealTimeSession("TxRealTime");

            var all = pb.GetObservable<T>();

            var windows = from w in all.Window(TimeSpan.FromSeconds(1))
                          from c in w.Count()
                          select c;

            windows.Subscribe(c => Console.WriteLine("Using Tx and Rx for count : {0:n}", c));

            pb.Start();
        }

        private const uint TraceModeRealTime = 0x00000100;
        private const uint TraceModeEventRecord = 0x10000000;
        private static readonly ulong InvalidHandle = (Environment.OSVersion.Version.Major >= 6 ? 0x00000000FFFFFFFF : 0xFFFFFFFFFFFFFFFF);
        static Timer _timer;

        static void ListenDirect()
        {
            _timer = new Timer(OnTimer, null, 1000, 1000);

            EVENT_TRACE_LOGFILE logFile = new EVENT_TRACE_LOGFILE();
            logFile.ProcessTraceMode = TraceModeEventRecord | TraceModeRealTime;
            logFile.LoggerName = "TxRealTime";
            logFile.EventRecordCallback = EventRecordCallback;
            ulong traceHandle = EtwNativeMethods.OpenTrace(ref logFile);

            if (traceHandle == InvalidHandle)
            {
                Console.WriteLine("Error in OpenTrace {0}", Marshal.GetLastWin32Error());
            }

            ulong[] array = { traceHandle };
            int error = EtwNativeMethods.ProcessTrace(array, 1, IntPtr.Zero, IntPtr.Zero);
            if (error != 0)
            {
                Console.WriteLine("Error in PropcessTrace {0}", error);
            }
        }

        static long counter = 0;

        static void EventRecordCallback([In] ref EVENT_RECORD eventRecord)
        {
            counter++;
        }

        static void OnTimer(object state)
        {
            Console.WriteLine("Using plain code: {0:n}", counter);
            counter = 0;
        }
    }
}

