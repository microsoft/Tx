namespace Tx.Bond
{
    using System;
    using System.Reactive;
    using System.Text;
    using System.Web.Script.Serialization;

    using Tx.Core;

    public class JsonTransformBuilder : ITransformBuilder<IEnvelope>
    {
        internal static readonly JavaScriptSerializer DefaultSerializer;

        private readonly JavaScriptSerializer serializer;

        static JsonTransformBuilder()
        {
            DefaultSerializer = new JavaScriptSerializer();
        }

        public JsonTransformBuilder()
            : this(DefaultSerializer)
        {
        }

        public JsonTransformBuilder(JavaScriptSerializer serializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            this.serializer = serializer;
        }

        public Func<TIn, IEnvelope> Build<TIn>()
        {
            return new JsonTransformer<TIn>(this.serializer).Transform;
        }

        internal sealed class JsonTransformer<T>
        {
            private readonly StringBuilder stringBuilder = new StringBuilder(64);

            private readonly string manifestId;

            private readonly JavaScriptSerializer serializer;

            public JsonTransformer(JavaScriptSerializer serializer)
            {
                this.serializer = serializer;
                this.manifestId = typeof(T).GetTypeIdentifier();
            }

            public IEnvelope Transform(T value)
            {
                var now = DateTime.UtcNow;

                this.stringBuilder.Clear();

                this.serializer.Serialize(value, this.stringBuilder);

                var json = this.stringBuilder.ToString();

                var payload = Encoding.UTF8.GetBytes(json);

                var envelope = new Envelope(
                    now,
                    now,
                    Protocol.Json,
                    null,
                    this.manifestId,
                    payload,
                    null);

                return envelope;
            }
        }
    }
}