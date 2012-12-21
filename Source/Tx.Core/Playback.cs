using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Linq.Expressions;
using System.Diagnostics;

namespace System.Reactive
{
    /// <summary>
    /// Playback serves two purposes:
    /// (1) Replay the history from one or more trace/log files, by turning the event occurence records into C# instances
    /// (2) Deliver events from fixed set of real time sessions as C# instances
    /// 
    /// The invariants that Playback preserves are:
    /// (a) The order within one input file/session (inputs events must be in order of occurence, and have increasing timestamps)
    /// (b) The illusion of global order - merging different files/streams on timestamp
    /// </summary>
    public class Playback : ITimeSource, IPlaybackConfiguration, IDisposable
    {
        readonly List<InputStream> _inputs;
        PullMergeSort<Timestamped<object>> _mergesort;
        OutputPump<Timestamped<object>> _pump;
        ManualResetEvent _pumpStart;
        Subject<Timestamped<object>> _subject = new Subject<Timestamped<object>>();
        TimeSource<Timestamped<object>> _timeSource;
        Demultiplexor _demux;
        IDisposable _toDemux;
        Stopwatch _stopwatch;

        List<IDisposable> _outputBuffers; 

        /// <summary>
        /// Constructor
        /// </summary>
        public Playback()
        {
            _inputs = new List<InputStream>();
            _demux = new Demultiplexor();
            _timeSource = new TimeSource<Timestamped<object>>(_subject, e => e.Timestamp);
            _toDemux = _timeSource
                .Select(e => e.Value)
                .Subscribe(_demux);

            _outputBuffers = new List<IDisposable>();
            _stopwatch = new Stopwatch();
        }

        /// <summary>
        /// Gets or sets the start time for the playback
        /// The setter must be called before any operators that take Scheduler are used
        /// </summary>
        public DateTimeOffset StartTime
        {
            get { return _timeSource.StartTime; }
            set { _timeSource.StartTime = value; }
        }

        /// <summary>
        /// The event types that are known
        /// 
        /// If you do playback.GetObservable&lt;A&gt:();  playback.GetObservable&lt;B&gt:();
        /// the known types will be A and B
        /// 
        /// Only known event types can be formatted to text
        /// Be sure to set the known types before calling Start() or Run()
        /// </summary>
        public Type[] KnownTypes { get; set; }

        public void Dispose()
        {
            foreach (InputStream input in _inputs)
            {
                ((IDisposable)input).Dispose();
            }

            if (null != _pump)
                _pump.Dispose();

            if (null!= _pumpStart)
                _pumpStart.Dispose();

            if (null!= _subject)
                _subject.Dispose();

            if (null!=_toDemux)
                _toDemux.Dispose();
        }

        /// <summary>
        /// Low level method for adding input sequence to the playback
        /// Usually, this will be called only from extension methods of Playback
        /// </summary>
        /// <typeparam name="TInput">Universal type that can can contain events of different actual (static) types</typeparam>
        /// <param name="createInput">How to create the input observalbe</param>
        /// <param name="createDeserializers">How to create the deserializers that understand TInput</param>
        void IPlaybackConfiguration.AddInput<TInput>(
            Expression<Func<IObservable<TInput>>> createInput,
            params Type[] typeMaps)
        {
            var input = new InputStream<TInput>(this, createInput, typeMaps);
            _inputs.Add(input);
        }

        /// <summary>
        /// Call this to get just the events of given type
        /// </summary>
        /// <typeparam name="TOutput">The type of interest</typeparam>
        /// <returns>Sequence of events of type TOutput from all inputs added to the playback</returns>
        public IObservable<TOutput> GetObservable<TOutput>()
        {
            foreach (var i in _inputs)
            {
                i.AddKnownType(typeof(TOutput));
            }
            return _demux.GetObservable<TOutput>();
        }

        /// <summary>
        /// Starts the playback and returns immediately
        /// 
        /// The main use case is real-time feeds.
        /// </summary>
        public void Start()
        {
            if (KnownTypes != null)
            {
                foreach (Type t in KnownTypes)
                {
                    foreach (InputStream i in _inputs)
                    {
                        i.AddKnownType(t);
                    }
                }
            }

            var queues = (from i in _inputs select i.Output).ToArray();
            _mergesort = new PullMergeSort<Timestamped<object>>(e => e.Timestamp.DateTime, queues);
            _timeSource.Connect();
            _pumpStart = new ManualResetEvent(false);
            _pump = new OutputPump<Timestamped<object>>(_mergesort, _subject, _pumpStart);
            _pumpStart.Set();
            _stopwatch.Start();
            foreach (InputStream i in _inputs)
            {
                i.Start();
            }
        }

        /// <summary>
        /// Starts the playback, and blocks until rocessing of input is completed
        /// </summary>
        public void Run()
        {
            Start();

            _pump.Completed.WaitOne();
            _stopwatch.Stop();
        }

        /// <summary>
        /// Scheduler that represents virtual time as per the timestamps on the events
        /// 
        /// Use playback.Scheduler as argument to temporal primitives like Window or Take
        /// </summary>
        public IScheduler Scheduler
        {
            get { return _timeSource.Scheduler; }
        }

        /// <summary>
        /// BufferOutput lets you accumulate a small collection that is the result of stream processing
        /// </summary>
        /// <typeparam name="TOutput">The event type of interest</typeparam>
        /// <param name="observavle">the results to accumulate</param>
        /// <returns></returns>
        public IEnumerable<TOutput> BufferOutput<TOutput>(IObservable<TOutput> observavle)
        {
            var list = new List<TOutput>();
            IDisposable d = observavle.Subscribe(o => list.Add(o));
            _outputBuffers.Add(d);
            return list;
        }

        /// <summary>
        /// The time elapsed 
        /// - from calling Start() or Run(),
        /// - to the current time (if processing is in progress) or the end of processing (e.g. Run() returns)
        /// </summary>
        public TimeSpan ExecutionDuration
        {
            get { return _stopwatch.Elapsed; }
        }

        ~Playback()
        {
            Dispose();
        }

        interface InputStream
        {
            void AddKnownType(Type t);
            IEnumerator<Timestamped<object>> Output { get; }
            void Start();
        }

        class InputStream<TInput> : InputStream, IDisposable
        {
            Playback _parent;
            IObservable<TInput> _source;
            IObserver<TInput> _deserializer;
            BufferQueue<Timestamped<object>> _output;
            IDisposable _subscription;

            public InputStream(
                Playback parent, 
                Expression<Func<IObservable<TInput>>> createInput,
                params Type[] typeMaps)
            {
                _parent = parent;
                _source = createInput.Compile()();
                _output = new BufferQueue<Timestamped<object>>();
                _deserializer = new CompositeDeserializer<TInput>(_output, typeMaps);
            }

            public void AddKnownType(Type t)
            {
                ((IDeserializer)_deserializer).AddKnownType(t);
            }

            public IEnumerator<Timestamped<object>> Output
            {
                get { return _output; }
            }

            public void Start()
            {
                _subscription = _source.Subscribe(_deserializer);
            }

            public void Dispose()
            {
                if (_subscription != null)
                {
                    _subscription.Dispose();
                }
                ((IDisposable)_output).Dispose();
            }
        }
    }
}
