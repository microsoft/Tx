// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace System.Reactive
{
    /// <summary>
    ///     Playback serves two purposes:
    ///     (1) Replay the history from one or more trace/log files, by turning the event occurence records into C# instances
    ///     (2) Deliver events from fixed set of real time sessions as C# instances
    ///     The invariants that Playback preserves are:
    ///     (a) The order within one input file/session (inputs events must be in order of occurence, and have increasing timestamps)
    ///     (b) The illusion of global order - merging different files/streams on timestamp
    /// </summary>
    public abstract class PlaybackBase : ITimeSource, IPlaybackConfiguration, IDisposable
    {
        protected readonly Demultiplexor _demux;
        protected readonly List<IInputStream> _inputs;
        private readonly List<IDisposable> _outputBuffers;
        private readonly Stopwatch _stopwatch;
        private readonly Subject<Timestamped<object>> _subject = new Subject<Timestamped<object>>();
        private readonly TimeSource<Timestamped<object>> _timeSource;
        private readonly IDisposable _toDemux;
        private PullMergeSort<Timestamped<object>> _mergesort;
        private OutputPump<Timestamped<object>> _pump;
        private ManualResetEvent _pumpStart;
        private DateTime _startTime = DateTime.MinValue;

        /// <summary>
        ///     Constructor
        /// </summary>
        protected PlaybackBase()
        {
            EndTime = DateTime.MaxValue;
            _inputs = new List<IInputStream>();
            _demux = new Demultiplexor();
            _timeSource = new TimeSource<Timestamped<object>>(_subject, e => e.Timestamp);
            _toDemux = _timeSource
                .Select(e => e.Value)
                .Subscribe(_demux);

            _outputBuffers = new List<IDisposable>();
            _stopwatch = new Stopwatch();
        }

        /// <summary>
        ///     Gets or sets the start time for the playback
        ///     The setter must be called before any operators that take Scheduler are used
        ///     
        ///     Playback will only deliver event after the given start time
        /// </summary>
        public DateTime StartTime
        {
            get { return _startTime; }
            set
            {
                _startTime = value;
                _timeSource.StartTime = new DateTimeOffset(value); 
            }
        }

        // set this to ignore events past given time
        public DateTime EndTime { get; set; }

        /// <summary>
        ///     The event types that are known
        ///     If you do playback.GetObservable&lt;A&gt;();  playback.GetObservable&lt;B&gt;();
        ///     the known types will be A and B
        ///     Only known event types can be formatted to text
        ///     Be sure to set the known types before calling Start() or Run()
        /// </summary>
        public Type[] KnownTypes { get; set; }

        /// <summary>
        ///     The time elapsed
        ///     - from calling Start() or Run(),
        ///     - to the current time (if processing is in progress) or the end of processing (e.g. Run() returns)
        /// </summary>
        public TimeSpan ExecutionDuration
        {
            get { return _stopwatch.Elapsed; }
        }

        public void Dispose()
        {
            foreach (IInputStream input in _inputs)
            {
                ((IDisposable) input).Dispose();
            }

            if (null != _pump)
                _pump.Dispose();

            if (null != _pumpStart)
                _pumpStart.Dispose();

            if (null != _subject)
                _subject.Dispose();

            if (null != _toDemux)
                _toDemux.Dispose();
        }

        /// <summary>
        ///     Low level method for adding input sequence to the playback
        ///     Usually, this will be called only from extension methods of Playback
        /// </summary>
        /// <typeparam name="TInput">Universal type that can can contain events of different actual (static) types</typeparam>
        /// <param name="createInput">How to create the input observalbe</param>
        /// <param name="typeMaps">The available type map types</param>
        void IPlaybackConfiguration.AddInput<TInput>(
            Expression<Func<IObservable<TInput>>> createInput,
            params Type[] typeMaps)
        {
            var mapInstances = new ITypeMap<TInput>[typeMaps.Length];
            for (int i = 0; i < typeMaps.Length; i++)
            {
                object o = Activator.CreateInstance(typeMaps[i]);
                if (o == null)
                    throw new Exception("Activator.CreateInstance failed for type " + typeMaps[i].Name);

                ITypeMap<TInput> mapInstance = o as ITypeMap<TInput>;
                if (mapInstance == null)
                    throw new Exception("The type " + typeMaps[i].FullName + " must implement one of these interfaces :"
                                        + typeof(ITypeMap<>).Name + ", "
                                        + typeof(IRootTypeMap<,>).Name + ", "
                                        + typeof(IPartitionableTypeMap<,>).Name);

                mapInstances[i] = mapInstance;
            }

            var input = new InputStream<TInput>(createInput, StartTime, EndTime, mapInstances);
            _inputs.Add(input);
        }

        /// <summary>
        ///     Low level method for adding input sequence to the playback
        ///     Usually, this will be called only from extension methods of Playback
        /// </summary>
        /// <typeparam name="TInput">Universal type that can can contain events of different actual (static) types</typeparam>
        /// <param name="createInput">How to create the input observalbe</param>
        /// <param name="typeMaps">The available type maps (local instances)</param>
        void IPlaybackConfiguration.AddInput<TInput>(
            Expression<Func<IObservable<TInput>>> createInput,
            params ITypeMap<TInput>[] typeMaps)
        {
            var input = new InputStream<TInput>(createInput, StartTime, EndTime, typeMaps);
            _inputs.Add(input);
        }

        /// <summary>
        ///     Scheduler that represents virtual time as per the timestamps on the events
        ///     Use playback.Scheduler as argument to temporal primitives like Window or Take
        /// </summary>
        public IScheduler Scheduler
        {
            get { return _timeSource.Scheduler; }
        }

        public IObservable<Timestamped<object>> GetAll(params Type[] types)
        {
            foreach (IInputStream i in _inputs)
            {
                foreach (Type t in types)
                {
                     i.AddKnownType(t);
                }
            }
            return _timeSource;
        }

        /// <summary>
        ///     Starts the playback and returns immediately
        ///     The main use case is real-time feeds.
        /// </summary>
        public void Start()
        {
            foreach (IInputStream i in _inputs)
            {
                i.StartTime = StartTime;
                i.EndTime = EndTime;

                if (KnownTypes == null)
                    continue;

                foreach (Type t in KnownTypes)
                {
                    i.AddKnownType(t);
                }
            }

            if (_inputs.Count == 0)
                throw new Exception("No input sequences were added to the Playback");

           if (_inputs.Count > 1)
            {
                IEnumerator<Timestamped<object>>[] queues = (from i in _inputs select i.Output).ToArray();
                _mergesort = new PullMergeSort<Timestamped<object>>(e => e.Timestamp.DateTime, queues);
                _timeSource.Connect();
                _pumpStart = new ManualResetEvent(false);
                _pump = new OutputPump<Timestamped<object>>(_mergesort, _subject, _pumpStart);
                _pumpStart.Set();

                _stopwatch.Start();
                foreach (IInputStream i in _inputs)
                {
                    i.Start();
                }
            }
            else
            {
                _timeSource.Connect();
                IInputStream singleInput = _inputs[0];
                _stopwatch.Start();
                singleInput.Start(_subject);
            }

        }

        /// <summary>
        ///     Starts the playback, and blocks until rocessing of input is completed
        /// </summary>
        public void Run()
        {
            Start();

            _timeSource.Completed.WaitOne();
            _stopwatch.Stop();
        }

        /// <summary>
        ///     BufferOutput lets you accumulate a small collection that is the result of stream processing
        /// </summary>
        /// <typeparam name="TOutput">The event type of interest</typeparam>
        /// <param name="observable">the results to accumulate</param>
        /// <returns></returns>
        public IEnumerable<TOutput> BufferOutput<TOutput>(IObservable<TOutput> observable)
        {
            var list = new List<TOutput>();
            IDisposable d = observable.Subscribe(list.Add);
            _outputBuffers.Add(d);
            return list;
        }

        ~PlaybackBase()
        {
            Dispose();
        }

        protected interface IInputStream
        {
            IEnumerator<Timestamped<object>> Output { get; }
            void AddKnownType(Type t);
            void Start();
            void Start(IObserver<Timestamped<object>> observer);

            DateTime StartTime { get; set; }
            DateTime EndTime { get; set; }
        }

        protected class InputStream<TInput> : IInputStream, IDisposable
        {
            private readonly BufferQueue<Timestamped<object>> _output;
            private readonly IObservable<TInput> _source;
            private IDisposable _subscription;
            private CompositeDeserializer<TInput> _deserializer;

            public InputStream(
                Expression<Func<IObservable<TInput>>> createInput,
                DateTime startTime,
                DateTime endTime,
                params ITypeMap<TInput>[] typeMaps)
            {
                _source = createInput.Compile()();
                _output = new BufferQueue<Timestamped<object>>();
                _deserializer = new CompositeDeserializer<TInput>(_output, typeMaps);
            }

            public DateTime StartTime
            {
                get { return _deserializer.StartTime; }
                set { _deserializer.StartTime = value; }
            }

            public DateTime EndTime
            {
                get { return _deserializer.EndTime; }
                set { _deserializer.EndTime = value; }
            }


            public void Dispose()
            {
                if (_subscription != null)
                {
                    _subscription.Dispose();
                }
                ((IDisposable) _output).Dispose();
            }

            public void AddKnownType(Type t)
            {
                ((IDeserializer) _deserializer).AddKnownType(t);
            }

            public IEnumerator<Timestamped<object>> Output
            {
                get { return _output; }
            }

            public void Start()
            {
                _subscription = _source.Subscribe(_deserializer);
            }

            public void Start(IObserver<Timestamped<object>> observer)
            {
                _deserializer.SetOutput(observer);
                _subscription = _source.Subscribe(_deserializer);
            }
        }
    }

    public class Playback : PlaybackBase, IPlayback
    {
        /// <summary>
        ///     Call this to get just the events of given type
        /// </summary>
        /// <typeparam name="TOutput">The type of interest</typeparam>
        /// <returns>Sequence of events of type TOutput from all inputs added to the playback</returns>
        public IObservable<TOutput> GetObservable<TOutput>()
        {
            foreach (IInputStream i in _inputs)
            {
                i.AddKnownType(typeof(TOutput));
            }
            return _demux.GetObservable<TOutput>();
        }      
    }
}
