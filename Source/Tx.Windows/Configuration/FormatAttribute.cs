using System;

namespace Tx.Windows
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class FormatAttribute : Attribute
    {
        readonly string _formatString;

        public FormatAttribute(string formatString)
        {
            _formatString = formatString;
        }

        public string FormatString
        {
            get { return _formatString; }
        }
    }
}
