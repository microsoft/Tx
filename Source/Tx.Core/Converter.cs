namespace Tx.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class Converter<TIn, TOut> : IObserver<TIn>
    {
        private readonly IDictionary<Type, Func<TIn, TOut>> serializers = new Dictionary<Type, Func<TIn, TOut>>();

        private readonly IObserver<TOut> next;

        private ITransformBuilder<TOut>[] transformBuilders;

        public Converter(IObserver<TOut> next, params ITransformBuilder<TOut>[] transformBuilders)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }
            if (transformBuilders == null)
            {
                throw new ArgumentNullException(nameof(transformBuilders));
            }

            this.next = next;
            this.transformBuilders = transformBuilders;
        }

        /// <summary>
        /// Provides the observer with new data.
        /// </summary>
        /// <param name="value">The current notification information.</param>
        public void OnNext(TIn value)
        {
            var type = value.GetType();
            Func<TIn, TOut> transform;
            if (!this.serializers.TryGetValue(type, out transform))
            {
                transform = this.BuildTransform(type);
                this.serializers.Add(type, transform);
            }

            if (transform != null)
            {
                var envelope = transform(value);

                this.next.OnNext(envelope);
            }
        }

        /// <summary>
        /// Notifies the observer that the provider has experienced an error condition.
        /// </summary>
        /// <param name="error">An object that provides additional information about the error.</param>
        public void OnError(Exception error)
        {
            this.next.OnError(error);
        }

        /// <summary>
        /// Notifies the observer that the provider has finished sending push-based notifications.
        /// </summary>
        public void OnCompleted()
        {
            this.next.OnCompleted();
        }

        public void RegisterTransformerBuilder(ITransformBuilder<TOut> transformBuilder)
        {
            if (transformBuilder == null)
            {
                throw new ArgumentNullException(nameof(transformBuilder), "Cannot be null.");
            }

            this.transformBuilders = this.transformBuilders
                .Concat(new[] { transformBuilder })
                .ToArray();
        }

        private Func<TIn, TOut> BuildTransform(Type type)
        {
            foreach (var transformBuilder in this.transformBuilders)
            {
                var transform = transformBuilder.Build<TIn, TOut>(type);

                if (transform != null)
                {
                    return transform;
                }
            }

            return null;
        }
    }

    internal static class E
    {
        public static Func<TIn, TOut> Build<TIn, TOut>(this ITransformBuilder<TOut> transformBuilder, Type type)
        {
            var method = typeof(ITransformBuilder<TOut>)
                .GetRuntimeMethod("Build", new Type[0])
                .MakeGenericMethod(type)
                .Invoke(transformBuilder, new object[0]) as Func<TIn, TOut>;

            return method;
        }
    }
}
