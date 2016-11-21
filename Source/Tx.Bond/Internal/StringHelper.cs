// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Tx.Bond
{
    using System.Collections.Generic;

    internal static class StringHelper
    {
        internal static IEnumerable<string> WholeChunks(this string str, int chunkSize)
        {
            for (int i = 0; i < str.Length; i += chunkSize)
            {
                yield return str.Length > i + chunkSize ? str.Substring(i, chunkSize) : str.Substring(i);
            }
        }
    }
}
