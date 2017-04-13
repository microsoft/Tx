using System.Collections.Generic;

namespace System.Linq
{
    public static class PullLinq
    {
        /// <summary>
        /// Filter does the same exact thing as .Where, and is intended to show how .Where actually works
        /// </summary>
        public static IEnumerable<T> Filter<T>(this IEnumerable<T> input, Func<T, bool> filter)
        {
            return new Filter<T>(input, filter);
        }

        /// <summary>
        /// Invoke a push rule for every element of pull sequence
        /// </summary>
        /// <typeparam name="TIn">Input event type</typeparam>
        /// <typeparam name="TOut">Output event type</typeparam>
        /// <param name="input">the input sequence</param>
        /// <param name="pushPipe">function that creates IObservable pipeline</param>
        /// <returns></returns>
        public static IEnumerable<TOut> ReplayRealTimeRule<TIn, TOut>(
            this IEnumerable<TIn> input, 
            Func<IObservable<TIn>, IObservable<TOut>> pushPipe)
        {
            return new PushInsidePull<TIn, TOut>(input, pushPipe);
        }
    }
}
