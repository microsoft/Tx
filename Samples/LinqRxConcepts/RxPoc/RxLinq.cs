using System.Collections.Generic;

namespace System.Reactive
{
    /// <summary>
    /// This shows how to create LINQ verbs as extension methods
    /// </summary>
    public static class RxLinq
    {
        public static IObservable<T> ToObservable<T>(this IEnumerable<T> events)
        {
            return new ColdObservable<T>(events);
        }

        public static IObservable<T> Where<T>(this IObservable<T> source, Func<T, bool> filter)
        {
            return new Where<T>(source, filter);
        }

        public static IObservable<TOut> Select<TIn, TOut>(this IObservable<TIn> source, Func<TIn, TOut> transform)
        {
            return new Select<TIn, TOut>(source, transform);
        }

        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> action)
        {
            return source.Subscribe(new ActionObserver<T>(action));
        }
    }
}
