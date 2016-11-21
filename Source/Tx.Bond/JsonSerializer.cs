namespace Tx.Bond
{
    //using System;
    //using System.Globalization;
    //using System.IO;
    //using System.Reactive;
    //using System.Reactive.V2;
    //using System.Text;

    //using Newtonsoft.Json;

    //public class JsonSerializer<T> : IObserver<T>
    //{
    //    private readonly IObserver<IEnvelope> next;

    //    private readonly StringBuilder stringBuilder = new StringBuilder(64);

    //    private static readonly JsonSerializer serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
    //    {
    //        NullValueHandling = NullValueHandling.Ignore,
    //        TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple,
    //        DefaultValueHandling = DefaultValueHandling.Ignore
    //    });

    //    private readonly string manifestId;

    //    public JsonSerializer(IObserver<IEnvelope> next)
    //    {
    //        if (next == null)
    //        {
    //            throw new ArgumentNullException("next");
    //        }

    //        this.next = next;
    //        this.manifestId = typeof(T).GetTypeIdentifier();
    //    }

    //    public void OnNext(T value)
    //    {
    //        var now = DateTime.UtcNow;

    //        this.stringBuilder.Clear();

    //        using (var writer = new StringWriter(this.stringBuilder, CultureInfo.InvariantCulture))
    //        using (var jsonWriter = new JsonTextWriter(writer))
    //        {
    //            jsonWriter.Formatting = serializer.Formatting;

    //            serializer.Serialize(jsonWriter, value, typeof(T));
    //        }

    //        var json = this.stringBuilder.ToString();

    //        var payload = Encoding.UTF8.GetBytes(json);

    //        var envelope = new Envelope(
    //            now, 
    //            now, 
    //            Protocol.Json, 
    //            null, 
    //            this.manifestId, 
    //            payload, 
    //            null);

    //        this.next.OnNext(envelope);
    //    }

    //    public void OnError(Exception error)
    //    {
    //        this.next.OnError(error);
    //    }

    //    public void OnCompleted()
    //    {
    //        this.next.OnCompleted();
    //    }
    //}

    //public class JsonTransformerBuilder : ITransformBuilder<IEnvelope>
    //{
    //    public Func<TIn, IEnvelope> Build<TIn>()
    //    {
    //        return new JsonTransformer<TIn>().Transform;
    //    }
    //}

    //public class JsonTransformer<T>
    //{
    //    private readonly IObserver<IEnvelope> next;

    //    private readonly StringBuilder stringBuilder = new StringBuilder(64);

    //    private static readonly JsonSerializer serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
    //    {
    //        NullValueHandling = NullValueHandling.Ignore,
    //        TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple,
    //        DefaultValueHandling = DefaultValueHandling.Ignore
    //    });

    //    private readonly string manifestId;

    //    public JsonTransformer()
    //    {
    //        this.manifestId = typeof(T).GetTypeIdentifier();
    //    }

    //    public IEnvelope Transform(T value)
    //    {
    //        var now = DateTime.UtcNow;

    //        this.stringBuilder.Clear();

    //        using (var writer = new StringWriter(this.stringBuilder, CultureInfo.InvariantCulture))
    //        using (var jsonWriter = new JsonTextWriter(writer))
    //        {
    //            jsonWriter.Formatting = serializer.Formatting;

    //            serializer.Serialize(jsonWriter, value, typeof(T));
    //        }

    //        var json = this.stringBuilder.ToString();

    //        var payload = Encoding.UTF8.GetBytes(json);

    //        var envelope = new Envelope(
    //            now,
    //            now,
    //            Protocol.Json,
    //            null,
    //            this.manifestId,
    //            payload,
    //            null);

    //        return envelope;
    //    }
    //}
}