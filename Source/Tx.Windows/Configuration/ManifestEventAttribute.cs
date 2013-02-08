// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Tx.Windows
{
    using System;
    using System.Text;

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
        public string Opcode { get { return _opcode; } }
        public byte Version { get { return _version; } }
        public string Level { get { return _level; } }
        public string Channel { get { return _channel; } }
        public string[] Keywords { get { return _keywords; } }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("EventId: ");
            sb.AppendLine(_eventId.ToString());
            sb.Append("Opcode: ");
            sb.AppendLine(_opcode.ToString());
            sb.Append("Version: ");
            sb.AppendLine(_version.ToString());
            sb.Append("Level: ");
            sb.AppendLine(_level);
            sb.Append("Channel: ");
            sb.AppendLine(_channel);
            sb.AppendLine("Keywords: ");
            foreach (string keyword in _keywords)
            {
                sb.Append("    ");
                sb.AppendLine(keyword);
            }

            return sb.ToString();
        }
    }
}