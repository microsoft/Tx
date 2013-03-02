// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Tx.Windows
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class FormatAttribute : Attribute
    {
        private readonly string _formatString;

        public FormatAttribute(string formatString)
        {
            _formatString = formatString;
        }

        public string FormatString
        {
            get { return _formatString; }
        }

        public override string ToString()
        {
            return _formatString;
        }
    }
}