// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Text;

namespace Tx.Windows
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ManifestEventAttribute : Attribute
    {
        private readonly string _channel;
        private readonly uint _eventId;
        private readonly string[] _keywords;
        private readonly string _level;
        private readonly string _opcode;
        private readonly Guid _providerGuid;
        private readonly byte _version;

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

        public Guid ProviderGuid
        {
            get { return _providerGuid; }
        }

        public uint EventId
        {
            get { return _eventId; }
        }

        public string Opcode
        {
            get { return _opcode; }
        }

        public byte Version
        {
            get { return _version; }
        }

        public string Level
        {
            get { return _level; }
        }

        public string Channel
        {
            get { return _channel; }
        }

        public string[] Keywords
        {
            get { return _keywords; }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("EventId: ");
            sb.AppendLine(_eventId.ToString(CultureInfo.InvariantCulture));
            sb.Append("Opcode: ");
            sb.AppendLine(_opcode);
            sb.Append("Version: ");
            sb.AppendLine(_version.ToString(CultureInfo.InvariantCulture));
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