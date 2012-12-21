using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Collections.Generic;

namespace System.Reactive
{
    public interface ITimeSource
    {
        IScheduler Scheduler { get; }
    }

    /// <summary>
    /// TimeSource constructs an "Virtual Time" scheduler based on expression over the event data
    /// </summary>
    /// <typeparam name="T">Type of the events in the sequence</typeparam>
    public class TimeSource<T> : IConnectableObservable<T>, ITimeSource
    {
        TimeSegmentScheduler _scheduler;
        Func<T, DateTimeOffset> _timeFunction;
        IObservable<T> _source;
        Subject<T> _subject;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="timeFunction">Expression to extract the timestamp</param>
        public TimeSource(
            IObservable<T> source, 
            Func<T, DateTimeOffset> timeFunction)
        {
            if (timeFunction == null)
                throw new ArgumentNullException("timeFunction");

            _source = source;
            _scheduler = new TimeSegmentScheduler();
            _timeFunction = timeFunction;
            _subject = new Subject<T>();
        }

        public DateTimeOffset StartTime
        {
            get { return _scheduler.Now; }
            set
            {
                if (!_scheduler._running)
                    _scheduler.Init(value);
                else
                    _scheduler.AdvanceTo(value);
            }
        }   
 
        void  OnCompleted()
        {
            _subject.OnCompleted();
        }

        void  OnError(Exception error)
        {
            _subject.OnError(error);
        }

        void  OnNext(T value)
        {
            DateTimeOffset time = _timeFunction(value);

            if (!_scheduler._running)
                _scheduler.Init(time);
            else
                if (time > _scheduler.Now)
                {
                    _scheduler.AdvanceTo(time);
                }

            _subject.OnNext(value);      
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _subject.Subscribe(observer);
        }

        public IScheduler Scheduler
        {
            get { return _scheduler; }
        }

        public IDisposable Connect()
        {
            return _source.Subscribe(OnNext, OnError, OnCompleted);
        }

        class TimeSegmentScheduler : IScheduler
        {
            HistoricalScheduler _historical = new HistoricalScheduler();
            List<IPostponedWorkItem> _postponed;
            public bool _running;

            public TimeSegmentScheduler()
            {
                _postponed = new List<IPostponedWorkItem>();
            }

            public void Init(DateTimeOffset startTime)
            {
                _historical.AdvanceTo(startTime);
                _running = true; 
                foreach (IPostponedWorkItem item in _postponed)
                {
                    item.Reschedule(_historical);
                }

            }

            interface IPostponedWorkItem
            {
                void Reschedule(HistoricalScheduler historical);
            }

            class PostponedWorkItem<TState> : IPostponedWorkItem, IDisposable
            {
                TimeSegmentScheduler _parent;
                
                DateTimeOffset? _dueTime; // only set one of _dueTime or _relativeTime
                TimeSpan _relativeTime;   // relative to start of playback
                
                TState _state;
                Func<IScheduler, TState, IDisposable> _action;
                IDisposable _disposable;

                public PostponedWorkItem(TimeSegmentScheduler parent, TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
                {
                    _parent = parent;
                    _state = state;
                    _dueTime = dueTime;
                    _action = action;
                    _parent._postponed.Add(this);
                }
                public PostponedWorkItem(TimeSegmentScheduler parent, TState state, TimeSpan relativeTime, Func<IScheduler, TState, IDisposable> action)
                {
                    _parent = parent;
                    _state = state;
                    _relativeTime = relativeTime;
                    _action = action;
                    _parent._postponed.Add(this);
                }

                public void Reschedule(HistoricalScheduler historical)
                {
                    if (_dueTime.HasValue)
                        _disposable = historical.Schedule(_state, _dueTime.Value, _action);
                    else
                        _disposable = historical.Schedule(_state, _relativeTime, _action);
                }

                public void Dispose()
                {
                    _parent._postponed.Remove(this);
                    _disposable.Dispose();
                }
            }

            public DateTimeOffset Now
            {
                get
                {
                    if (!_running)
                        throw new NotImplementedException();
                    
                    return _historical.Clock;
                }
            }

            public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
            {
                if (_running)
                {
                    return _historical.ScheduleAbsolute<TState>(state, dueTime, action);
                }
                else
                {
                    return new PostponedWorkItem<TState>(this, state, dueTime, action);
                }
            }

            public IDisposable Schedule<TState>(TState state, TimeSpan relativeTime, Func<IScheduler, TState, IDisposable> action)
            {
                if (_running)
                {
                    return _historical.ScheduleRelative<TState>(state, relativeTime, action);
                }
                else
                {
                    return new PostponedWorkItem<TState>(this, state, relativeTime, action);
                }
            }

            public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
            {
                throw new NotImplementedException();
            }

            public void AdvanceTo(DateTimeOffset value)
            {
                _historical.AdvanceTo(value);
            }
        }
    }
}
