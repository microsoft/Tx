// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Tx.Bond
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;

    public static class BondIdentifierHelpers
    {
        private static readonly byte[] NamespaceBytes = new byte[16]
        {
            (byte) 72,
            (byte) 44,
            (byte) 45,
            (byte) 178,
            (byte) 195,
            (byte) 144,
            (byte) 71,
            (byte) 200,
            (byte) 135,      
            (byte) 248,
            (byte) 26,
            (byte) 21,
            (byte) 191,
            (byte) 193,
            (byte) 48,
            (byte) 251
        };

        public static string GetBondManifestIdentifier(this object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            var type = instance.GetType();

            var manifestId = type.GetBondManifestIdentifier();

            return manifestId;
        }

        public static string GetBondManifestIdentifier(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            var bondMapAttribute = ((GuidAttribute[])type.GetCustomAttributes(typeof(GuidAttribute), false))
                .FirstOrDefault();

            if (bondMapAttribute != null &&
                !string.IsNullOrEmpty(bondMapAttribute.Value))
            {
                return bondMapAttribute.Value;
            }

            return GenerateGuidFromName(type.Name.ToUpperInvariant()).ToString();
        }

        public static string GetBondManifestIdentifier<T>()
        {
            return typeof(T).GetBondManifestIdentifier();
        }

        public static Guid GenerateGuidFromName(string name)
        {
            byte[] array;
            using (var shA1 = SHA1.Create())
            {
                array = Encoding.BigEndianUnicode.GetBytes(name);
                shA1.TransformBlock(BondIdentifierHelpers.NamespaceBytes, 0, BondIdentifierHelpers.NamespaceBytes.Length, BondIdentifierHelpers.NamespaceBytes, 0);
                shA1.TransformFinalBlock(array, 0, array.Length);
                array = shA1.Hash;
            }
            Array.Resize<byte>(ref array, 16);
            array[7] = (byte)((int)array[7] & 15 | 80);
            return new Guid(array);
        }
    }
}
