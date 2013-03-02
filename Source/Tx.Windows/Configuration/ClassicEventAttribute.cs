// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Tx.Windows
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ClassicEventAttribute : Attribute
    {
        private readonly Guid _eventGuid;
        private readonly int _opcode;
        private readonly byte _version;

        public ClassicEventAttribute(string eventGuid, int opcode, byte version)
        {
            _eventGuid = new Guid(eventGuid);
            _opcode = opcode;
            _version = version;
        }

        public Guid EventGuid
        {
            get { return _eventGuid; }
        }

        public int Opcode
        {
            get { return _opcode; }
        }

        public byte Version
        {
            get { return _version; }
        }
    }
}