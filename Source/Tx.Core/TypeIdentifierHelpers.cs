namespace System.Reactive
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;
    using System.Reflection;

    public static class TypeIdentifierHelpers
    {
        private static readonly byte[] NamespaceBytes =
        {
            72,
            44,
            45,
            178,
            195,
            144,
            71,
            200,
            135,
            248,
            26,
            21,
            191,
            193,
            48,
            251
        };

        /// <summary>
        /// Gets the type identifier.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>The type identifier.</returns>
        /// <exception cref="System.ArgumentNullException">instance is null.</exception>
        public static string GetTypeIdentifier(this object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var type = instance.GetType();

            var manifestId = type.GetTypeIdentifier();

            return manifestId;
        }

        /// <summary>
        /// Gets the type identifier.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The type identifier.</returns>
        /// <exception cref="System.ArgumentNullException">type is null.</exception>
        public static string GetTypeIdentifier(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var bondMapAttribute = ((GuidAttribute[])type.GetTypeInfo().GetCustomAttributes(typeof(GuidAttribute), false))
                .FirstOrDefault();

            if (bondMapAttribute != null &&
                !string.IsNullOrEmpty(bondMapAttribute.Value))
            {
                return bondMapAttribute.Value;
            }

            return GenerateGuidFromName(type.Name.ToUpperInvariant()).ToString();
        }

        /// <summary>
        /// Gets the type identifier.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <returns>The type identifier.</returns>
        public static string GetTypeIdentifier<T>()
        {
            return typeof(T).GetTypeIdentifier();
        }

        /// <summary>
        /// Generates the type identifier based on the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The type identifier.</returns>
        public static Guid GenerateGuidFromName(string name)
        {
            byte[] array;
            using (var shA1 = SHA1.Create())
            {
                array = Encoding.BigEndianUnicode.GetBytes(name);

                byte[] buffer = new byte[array.Length + NamespaceBytes.Length];

                Buffer.BlockCopy(NamespaceBytes, 0, buffer, 0, NamespaceBytes.Length);
                Buffer.BlockCopy(array, 0, buffer, NamespaceBytes.Length, array.Length);

                array = shA1.ComputeHash(buffer);
            }

            Array.Resize(ref array, 16);
            array[7] = (byte)(array[7] & 15 | 80);
            return new Guid(array);
        }
    }
}
