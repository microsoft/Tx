// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Tx.Windows
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public sealed class EventFieldAttribute : Attribute
    {
        private readonly string _length;
        private readonly string _originalType;
        private readonly Type _structType;

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

        public EventFieldAttribute(Type structType, string length = null)
        {
            _originalType = "struct";
            _length = length;
            _structType = structType;
        }

        public string OriginalType
        {
            get { return _originalType; }
        }

        public string Length
        {
            get { return _length; }
        }

        public Type StructType
        {
            get { return _structType; }
        }
    }
}