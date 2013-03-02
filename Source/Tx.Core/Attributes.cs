// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Reactive
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class OccurenceTimeAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class TypeStatisticsAttribute : Attribute
    {
        private readonly string _extension;

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
        private readonly string _description;
        private readonly string[] _extensions;

        public FileParserAttribute(string description, params string[] extensions)
        {
            _extensions = extensions;
            _description = description;
        }

        public string[] Extensions
        {
            get { return _extensions; }
        }

        public string Description
        {
            get { return _description; }
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class RealTimeFeedAttribute : Attribute
    {
        private readonly string _description;
        private readonly string _extension;

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
        private readonly Type _attributeType;

        public DeserializerAttribute(Type attributeType)
        {
            _attributeType = attributeType;
        }

        public Type AttributeType
        {
            get { return _attributeType; }
        }
    }
}