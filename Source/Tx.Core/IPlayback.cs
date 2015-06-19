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
