namespace Tx.Bond
{
    using System;
    using System.Linq;
    using System.Net;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal sealed class IpAddressConverter : JsonConverter
    {
        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var address = (IPAddress)value;

            writer.WriteStartObject();

            writer.WritePropertyName("Address");

            writer.WriteValue(address.ToString());

            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType != typeof(IPAddress))
            {
                throw new InvalidOperationException("Can only deserialize IP Address.");
            }

            var jsonObject = JObject.Load(reader);

            var properties = jsonObject.Properties().ToList();

            var addressString = properties.FirstOrDefault(prop => prop.Name.Equals("Address", StringComparison.Ordinal));
            IPAddress address;
            if (addressString == null || !IPAddress.TryParse(addressString.Value.Value<string>(), out address))
            {
                throw new ArgumentException("Address property must be non-null and parseable as IPAddress.");
            }

            return address;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IPAddress);
        }
    }
}