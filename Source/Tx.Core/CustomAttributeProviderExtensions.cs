// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.Reactive
{
    public static class CustomAttributeProviderExtensions
    {
        public static TAttribute GetAttribute<TAttribute>(this ICustomAttributeProvider provider)
        {
            object[] attributes = provider.GetCustomAttributes(typeof (TAttribute), false);
            if (attributes.Length == 0)
                return default(TAttribute);

            return (TAttribute) attributes[0];
        }

        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this ICustomAttributeProvider provider)
        {
            return (provider.GetCustomAttributes(typeof (TAttribute), false)).Cast<TAttribute>();
        }
    }
}