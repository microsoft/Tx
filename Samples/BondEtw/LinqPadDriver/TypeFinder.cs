namespace Tx.Bond.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reactive;
    using System.Reflection;

    using Tx.Bond;

    public static class TypeFinder
    {
        private static ICollection<Assembly> GetAssemblies(string folder)
        {
            var assembliesFiles = new[] { "*.exe", "*.dll" }
                .SelectMany(i => Directory.GetFiles(folder, i))
                .Select(
                    assemblyName =>
                        {
                            Assembly assembly = null;
                            try
                            {
                                assembly = Assembly.LoadFrom(assemblyName);
                            }
                            catch (Exception exception)
                            {
                                Trace.TraceError(exception.ToString());
                            }
                            return assembly;
                        })
                   .Where(assembly => assembly != null)
                .ToArray();

            return assembliesFiles;
        }

        private static ICollection<Type> GetTypes(this Assembly assembly, Func<Type, bool> predicate)
        {
            var result = assembly
                .GetTypes()
                .Where(predicate)
                .ToArray();

            return result;
        }

        private static bool IsTypeMapType(this Type type)
        {
            if (type.IsPublic &&
                type.IsClass &&
                type.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Any(i => i.GetParameters().Length == 0) &&
                type.IsAbstract == false &&
                type.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITypeMap<>)))
            {
                return true;
            }

            return false;
        }

        //public static bool IsBondType(this Type type)
        //{
        //    var attribute = type.GetAttribute<global::Bond.SchemaAttribute>();

        //    return attribute != null && !type.IsGenericTypeDefinition;
        //}

        public static Type[] LoadTypeMaps(string folder)
        {
            Type[] typeMapsTypes = null;

            try
            {
                typeMapsTypes = GetAssemblies(folder)
                    .SelectMany(
                        assembly =>
                        assembly.GetTypes(
                            type => type.IsTypeMapType() && typeof(ITypeMap<IEnvelope>).IsAssignableFrom(type)))
                    .ToArray();
            }
            catch
            {
                // Ignored.
            }

            return typeMapsTypes;
        }
    }
}
