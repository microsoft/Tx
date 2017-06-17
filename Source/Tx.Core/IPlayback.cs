// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Reactive
{
    using System.Reactive.Concurrency;

    /// <summary>
    /// Contract for demiltiplexors providing sequences of events of specific types.
    /// </summary>
    public interface IObservableDemultiplexor
    {
        /// <summary>
        ///     Call this to get just the events of given type
        /// </summary>
        /// <typeparam name="T">The type of interest</typeparam>
        IObservable<T> GetObservable<T>();
    }

    public interface IPlayback : IObservableDemultiplexor, IPlaybackConfiguration
    {
        IScheduler Scheduler { get; }

        void Run();

        void Start();
    }
}
