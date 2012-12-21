using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace System.Reactive
{
    public static class CustomAttributeProviderExtensions
    {
        public static TAttribute GetAttribute<TAttribute>(this ICustomAttributeProvider provider)
        {
            var attributes = provider.GetCustomAttributes(typeof(TAttribute), false);
            if (attributes.Length == 0)
                return default(TAttribute);

            return (TAttribute)attributes[0];
        }

        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this ICustomAttributeProvider provider)
        {
            return (provider.GetCustomAttributes(typeof(TAttribute), false)).Cast<TAttribute>();
        }
    }
}
