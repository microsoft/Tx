namespace Tx.Bond
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Reactive;
    using System.Text;

    using Newtonsoft.Json;

    using Tx.Core;

    public class JsonTransformBuilder : ITransformBuilder<IEnvelope>
    {
        public Func<TIn, IEnvelope> Build<TIn>()
        {
            return new JsonTransformer<TIn>().Transform;
        }

        internal sealed class JsonTransformer<T>
        {
            private readonly StringBuilder stringBuilder = new StringBuilder(64);

            private static readonly JsonSerializer serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
                                                                                                 {
                                                                                                     NullValueHandling = NullValueHandling.Ignore,
                                                                                                     TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple,
                                                                                                     DefaultValueHandling = DefaultValueHandling.Ignore
                                                                                                 });

            private readonly string manifestId;

            public JsonTransformer()
            {
                this.manifestId = typeof(T).GetTypeIdentifier();
            }

            public IEnvelope Transform(T value)
            {
                var now = DateTime.UtcNow;

                this.stringBuilder.Clear();

                using (var writer = new StringWriter(this.stringBuilder, CultureInfo.InvariantCulture))
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    jsonWriter.Formatting = serializer.Formatting;

                    serializer.Serialize(jsonWriter, value, typeof(T));
                }

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