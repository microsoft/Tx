using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Reactive;

namespace Tx.LinqPad
{
    static class ParserRegistry
    {
        static MethodInfo[] _addSessions;
        static MethodInfo[] _addFiles;
        static Dictionary<string, Type> _typeStats;
        static Type[] _availableTypes;

        public static void Init()
        {
            if (_addFiles != null)
                return;

            string dir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var types = from file in Directory.GetFiles(dir, "*.dll")
                        from t in Assembly.LoadFrom(file).GetTypes()
                        where t.IsPublic
                        select t;

            var methods = from t in types
                          from m in t.GetMethods()
                          where m.GetAttribute<FileParserAttribute>() != null
                          select m;

            _addFiles = methods.ToArray();

            methods = from t in types
                          from m in t.GetMethods()
                          where m.GetAttribute<RealTimeFeedAttribute>() != null
                          select m;

            _addSessions = methods.ToArray();
        }

        public static string Filter
        {
            get
            {
                Init();
                var attributes = (from mi in _addFiles
                                 select mi.GetAttribute<FileParserAttribute>()).ToArray();

                StringBuilder sb = new StringBuilder("All Files|");
                foreach (FileParserAttribute a in attributes)
                {
                    sb.Append('*');
                    sb.Append(a.Extension);
                    sb.Append(';');
                }
                foreach (FileParserAttribute a in attributes)
                {
                    sb.Append('|');
                    sb.Append(a.Description);
                    sb.Append('|');
                    sb.Append('*');
                    sb.Append(a.Extension);
                    sb.Append(';');
                }

                return sb.ToString().Replace(";|", "|");
            }
        }

        public static IEnumerable<Assembly> GetAssemblies()
        {
            Init();

            return (from m in _addFiles select m.DeclaringType.Assembly).Distinct();
        }

        public static IEnumerable<string> GetNamespaces()
        {
            Init();

            return (from m in _addFiles select m.DeclaringType.Namespace).Distinct();
        }

        public static Dictionary<Type, long> GetTypeStatistics(string[] files)
        {
            Init();

            TypeOccurenceStatistics stat = new TypeOccurenceStatistics(TypeCache.AvailableTypes);
            AddFiles(stat, files);
            stat.Run();
             
            return stat.Statistics;
        }

        public static void AddFiles(IPlaybackConfiguration playback, string[] files)
        {
            var filesByExtension = new Dictionary<string, List<string>>();
            foreach (string f in files)
            {
                string extension = Path.GetExtension(f);

                List<string> sameExtension;
                if (!filesByExtension.TryGetValue(extension, out sameExtension))
                {
                    sameExtension = new List<string>();
                    filesByExtension.Add(extension, sameExtension);
                }

                sameExtension.Add(f);
            }

            foreach (string ext in filesByExtension.Keys)
            {
                MethodInfo addMethod = (from mi in _addFiles
                                        where mi.GetAttribute<FileParserAttribute>().Extension == ext
                                        select mi).FirstOrDefault();

                addMethod.Invoke(null, new object[] { playback, filesByExtension[ext].ToArray() });
            }
        }

        public static void AddSession(IPlaybackConfiguration playback, string session)
        {
            foreach (MethodInfo addMethod in _addSessions)
            {
                addMethod.Invoke(null, new object[] { playback, session });
            }
        }

        static T GetAttribute<T>(ICustomAttributeProvider provider)
        {
            return (T)(provider.GetCustomAttributes(typeof(T), false))[0];
        }
    }
}
