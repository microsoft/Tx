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
        internal static readonly JsonSerializer DefaultSerializer;

        private readonly JsonSerializer serializer;

        static JsonTransformBuilder()
        {
            var serializationSettings = new JsonSerializerSettings
            {
                CheckAdditionalContent = false,
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
            serializationSettings.Converters.Add(new IpAddressConverter());
            DefaultSerializer = JsonSerializer.CreateDefault(serializationSettings);
        }

        public JsonTransformBuilder()
            : this(DefaultSerializer)
        {
        }

        public JsonTransformBuilder(JsonSerializer serializer)
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

            private readonly JsonSerializer serializer;

            public JsonTransformer(JsonSerializer serializer)
            {
                this.serializer = serializer;
                this.manifestId = typeof(T).GetTypeIdentifier();
            }

            public IEnvelope Transform(T value)
            {
                var now = DateTime.UtcNow;

                this.stringBuilder.Clear();

                using (var writer = new StringWriter(this.stringBuilder, CultureInfo.InvariantCulture))
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    jsonWriter.Formatting = this.serializer.Formatting;

                    DefaultSerializer.Serialize(jsonWriter, value, typeof(T));
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