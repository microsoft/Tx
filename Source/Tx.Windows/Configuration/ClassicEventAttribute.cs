namespace Tx.Windows
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ClassicEventAttribute : Attribute
    {
        readonly Guid _eventGuid;
        readonly int _opcode;
        readonly byte _version;

        public ClassicEventAttribute(string eventGuid, int opcode, byte version)
        {
            _eventGuid = new Guid(eventGuid);
            _opcode = opcode;
            _version = version;
        }

        public Guid EventGuid { get { return _eventGuid; } }
        public int Opcode { get { return _opcode; } }
        public byte Version { get { return _version; } }
    }
}