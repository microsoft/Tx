namespace Tx.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;

    public class Transformer<T> : IObserver<T> where T : class
    {
        private readonly IDictionary<Type, Func<T, IEnvelope>> serializers = new Dictionary<Type, Func<T, IEnvelope>>();

        private readonly IObserver<IEnvelope> next;

        private ITransformerBuilder<IEnvelope>[] transformerBuilders;

        public Transformer(IObserver<IEnvelope> next, params ITransformerBuilder<IEnvelope>[] transformerBuilders)
        {
            this.next = next;
            this.transformerBuilders = transformerBuilders;
        }

        public void OnNext(T value)
        {
            var type = value.GetType();
            Func<T, IEnvelope> transformer;
            if (!this.serializers.TryGetValue(type, out transformer))
            {
                transformer = this.BuildTransformer(type);
                this.serializers.Add(type, transformer);
            }

            if (transformer != null)
            {
                var envelope = transformer(value);

                this.next.OnNext(envelope);
            }
        }

        public void OnError(Exception error)
        {
            this.next.OnError(error);
        }

        public void OnCompleted()
        {
            this.next.OnCompleted();
        }

        public void RegisterTransformerBuilder(ITransformerBuilder<IEnvelope> transformerBuilder)
        {
            if (transformerBuilder == null)
            {
                throw new ArgumentNullException("transformerBuilder");
            }

            this.transformerBuilders = this.transformerBuilders
                .Concat(new[] { transformerBuilder })
                .ToArray();
        }

        private Func<T, IEnvelope> BuildTransformer(Type type)
        {
            foreach (var transformBuilder in this.transformerBuilders)
            {
                var transformer = transformBuilder.Build<T, IEnvelope>(type);

                if (transformer != null)
                {
                    return transformer;
                }
            }

            return null;
        }
    }

    internal static class E
    {
        public static Func<TIn, TOut> Build<TIn, TOut>(this ITransformerBuilder<TOut> transformerBuilder, Type type)
        {
            var method = typeof(ITransformerBuilder<TOut>)
                .GetMethod("Build")
                .MakeGenericMethod(type)
                .Invoke(transformerBuilder, new object[0]) as Func<TIn, TOut>;

            return method;
        }
    }
}
