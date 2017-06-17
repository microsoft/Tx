// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Threading;

namespace System.Reactive
{
    public interface ITimeSource
    {
        IScheduler Scheduler { get; }
    }

    /// <summary>
    ///     TimeSource constructs an "Virtual Time" scheduler based on expression over the event data
    /// </summary>
    /// <typeparam name="T">Type of the events in the sequence</typeparam>
    public class TimeSource<T> : IConnectableObservable<T>, ITimeSource, IDisposable
    {
        private readonly TimeSegmentScheduler _scheduler;
        private readonly IObservable<T> _source;
        private readonly Subject<T> _subject;
        private readonly Func<T, DateTimeOffset> _timeFunction;
        private readonly ManualResetEvent _completed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSource{T}"/> class.
        /// </summary>
        /// <param name="source">The event sequence to use as source</param>
        /// <param name="timeFunction">Expression to extract the timestamp</param>
        public TimeSource(
            IObservable<T> source,
            Func<T, DateTimeOffset> timeFunction)
        {
            if (timeFunction == null)
                throw new ArgumentNullException(nameof(timeFunction));

            _source = source;
            _scheduler = new TimeSegmentScheduler();
            _timeFunction = timeFunction;
            _subject = new Subject<T>();
            _completed = new ManualResetEvent(false);
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

        public WaitHandle Completed
        {
            get { return _completed; }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _subject.Subscribe(observer);
        }

        public IDisposable Connect()
        {
            return _source.Subscribe(OnNext, OnError, OnCompleted);
        }

        public IScheduler Scheduler
        {
            get { return _scheduler; }
        }

        private void OnCompleted()
        {
            _subject.OnCompleted();
            _scheduler.Stop();
            _completed.Set();
        }

        private void OnError(Exception error)
        {
            _subject.OnError(error);
            _completed.Set();
        }

        private void OnNext(T value)
        {
            DateTimeOffset time = _timeFunction(value);

            if (!_scheduler._running)
                _scheduler.Init(time);
            else if (time > _scheduler.Now)
            {
                _scheduler.AdvanceTo(time);
            }

            _subject.OnNext(value);
        }

        public void Dispose()
        {
            if (_completed != null) _completed.Dispose();
        }

        private sealed class TimeSegmentScheduler : IScheduler
        {
            private readonly HistoricalScheduler _historical = new HistoricalScheduler();
            private readonly List<IPostponedWorkItem> _postponed;
            public bool _running;

            public TimeSegmentScheduler()
            {
                _postponed = new List<IPostponedWorkItem>();
            }

            public void Stop()
            {
                _historical.AdvanceBy(TimeSpan.FromTicks(1));
            }

            public DateTimeOffset Now
            {
                get
                {
                    if (!_running)
                    {
                        throw new NotImplementedException();
                    }

                    return _historical.Clock;
                }
            }

            public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime,
                                                Func<IScheduler, TState, IDisposable> action)
            {
                if (_running)
                {
                    return _historical.ScheduleAbsolute(state, dueTime, action);
                }
                return new PostponedWorkItem<TState>(this, state, dueTime, action);
            }

            public IDisposable Schedule<TState>(TState state, TimeSpan relativeTime,
                                                Func<IScheduler, TState, IDisposable> action)
            {
                if (_running)
                {
                    return _historical.ScheduleRelative(state, relativeTime, action);
                }
                return new PostponedWorkItem<TState>(this, state, relativeTime, action);
            }

            public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
            {
                return Schedule(state, TimeSpan.Zero, action);
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

            public void AdvanceTo(DateTimeOffset value)
            {
                _historical.AdvanceTo(value);
            }

            private interface IPostponedWorkItem
            {
                void Reschedule(HistoricalScheduler historical);
            }

            private class PostponedWorkItem<TState> : IPostponedWorkItem, IDisposable
            {
                private readonly Func<IScheduler, TState, IDisposable> _action;
                private readonly TimeSegmentScheduler _parent;

                private readonly TimeSpan _relativeTime; // relative to start of playback

                private readonly TState _state;
                private IDisposable _disposable;
                private DateTimeOffset? _dueTime; // only set one of _dueTime or _relativeTime

                public PostponedWorkItem(TimeSegmentScheduler parent, TState state, DateTimeOffset dueTime,
                                         Func<IScheduler, TState, IDisposable> action)
                {
                    _parent = parent;
                    _state = state;
                    _dueTime = dueTime;
                    _action = action;
                    _parent._postponed.Add(this);
                }

                public PostponedWorkItem(TimeSegmentScheduler parent, TState state, TimeSpan relativeTime,
                                         Func<IScheduler, TState, IDisposable> action)
                {
                    _parent = parent;
                    _state = state;
                    _relativeTime = relativeTime;
                    _action = action;
                    _parent._postponed.Add(this);
                }

                public void Dispose()
                {
                    _parent._postponed.Remove(this);
                    if (_disposable != null)
                        _disposable.Dispose();
                }

                public void Reschedule(HistoricalScheduler historical)
                {
                    if (_dueTime.HasValue)
                    {
                        _disposable = historical.Schedule(_state, _dueTime.Value, _action);
                    }
                    else
                    {
                        if (this._relativeTime > TimeSpan.Zero)
                        {
                            this._disposable = historical.Schedule(this._state, this._relativeTime, this._action);
                        }
                        else
                        {
                            this._disposable = this._action(historical, this._state);
                        }
                    }
                }
            }
        }
    }
}