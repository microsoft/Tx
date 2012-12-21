namespace System.Reactive
{
    using System;

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class OccurenceTimeAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class TypeStatisticsAttribute : Attribute
    {
        readonly string _extension;

        public TypeStatisticsAttribute(string extension)
        {
            _extension = extension;
        }


        public string Extension
        {
            get { return _extension; }
        }

    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class FileParserAttribute : Attribute
    {
        readonly string _extension;
        readonly string _description;

        public FileParserAttribute(string extension, string description)
        {
            _extension = extension;
            _description = description;
        }

        public string Extension
        {
            get { return _extension; }
        }

        public string Description
        {
            get { return _description; }
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class RealTimeFeedAttribute : Attribute
    {
        readonly string _extension;
        readonly string _description;

        public RealTimeFeedAttribute(string extension, string description)
        {
            _extension = extension;
            _description = description;
        }

        public string Extension
        {
            get { return _extension; }
        }

        public string Description
        {
            get { return _description; }
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class DeserializerAttribute : Attribute
    {
        readonly Type _attributeType;

        public DeserializerAttribute(Type attributeType)
        {
            _attributeType = attributeType;
        }

        public Type AttributeType { get { return _attributeType; } }
    }
}
