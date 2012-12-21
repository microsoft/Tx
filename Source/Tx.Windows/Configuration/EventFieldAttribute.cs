namespace Tx.Windows
{
    using System;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public sealed class EventFieldAttribute : Attribute
    {
        readonly string _originalType;
        readonly string _length;
        readonly Type _structType;

        public EventFieldAttribute(string inType)
        {
            if (string.IsNullOrEmpty(inType))
            {
                throw new ArgumentNullException("inType");
            }

            _originalType = inType;
        }
        
        public EventFieldAttribute(string inType, string length = null)
        {
            if (string.IsNullOrEmpty(inType))
            {
                throw new ArgumentNullException("inType");
            }

            _originalType = inType;
            _length = length;
        }

        public EventFieldAttribute(int version, int ordinal, Type structType, string length = null)
        {
            _originalType = "struct";
            _length = length;
            _structType = structType;
        }

        public string OriginalType { get { return _originalType; } }
        public string Length { get { return _length; } }
        public Type StructType { get { return _structType; } }
    }
}
