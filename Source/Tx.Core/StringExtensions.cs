// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Reactive
{
    public static class StringExtensions
    {
        public static string GetSubstring(this string source, string startMarker, string endMarker)
        {
            int startIndex = source.IndexOf(startMarker, StringComparison.Ordinal);
            int begin = startIndex + startMarker.Length;

            string result;
            if (endMarker == null)
            {
                result = source.Substring(begin);
            }
            else
            {
                int end = source.IndexOf(endMarker, begin, StringComparison.Ordinal);
                result = source.Substring(begin, end - begin);
            }

            return result;
        }
    }
}
