namespace Tx.Windows
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ManifestEventAttribute : Attribute
    {
        readonly Guid _providerGuid;
        readonly uint _eventId;
        readonly byte _version;
        readonly string _opcode;
        readonly string _level;
        readonly string _channel;
        readonly string[] _keywords;

        public ManifestEventAttribute(string providerGuid, uint eventId, byte version)
        {
            _providerGuid = new Guid(providerGuid);
            _eventId = eventId;
            _version = version;
        }

        public ManifestEventAttribute(string providerGuid, uint eventId, byte version,
               string opcode, string level, string channel, params string[] keywords)
        {
            _providerGuid = new Guid(providerGuid);
            _eventId = eventId;
            _version = version;
            _opcode = opcode;
            _level = level;
            _channel = channel;
            _keywords = keywords;
        }

        public Guid ProviderGuid { get { return _providerGuid; } }
        public uint EventId { get { return _eventId; } }
        public byte Version { get { return _version; } }
        public string Opcode { get { return _opcode; } }
        public string Level { get { return _level; } }
        public string Channel { get { return _channel; } }
        public string[] Keywords { get { return _keywords; } }
    }
}