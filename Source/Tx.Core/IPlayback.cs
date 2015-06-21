// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Reactive
{
    using System.Reactive.Concurrency;

    public interface IObservableDemultiplexor
    {
        IObservable<T> GetObservable<T>();
    }

    public interface IPlayback : IObservableDemultiplexor, IPlaybackConfiguration
    {
        IScheduler Scheduler { get; }

        void Run();

        void Start();
    }
}
