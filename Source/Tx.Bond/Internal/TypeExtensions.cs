// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Tx.Bond
{
    using System;
    using System.IO;

    internal static class TypeExtensions
    {
        public static byte[] ToByteArray(this ArraySegment<byte> segment)
        {
            if (segment.Offset == 0 && segment.Count == segment.Array.Length)
            {
                return segment.Array;
            }

            var array = new byte[segment.Count - segment.Offset];

            Buffer.BlockCopy(segment.Array, segment.Offset, array, 0, array.Length);

            return array;
        }

        public static string TryGetManifestData(this Type type)
        {
            string manifest = null;
            try
            {
                var resourceName = Path.GetFileNameWithoutExtension(type.Assembly.ManifestModule.Name) + "." + type.Name + ".bond";

                var stream = type.Assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    using (stream)
                    using (var reader = new StreamReader(stream))
                    {
                        manifest = reader.ReadToEnd();
                    }
                }
            }
            catch
            {
                // ignored
            }

            return manifest;
        }
    }
}
