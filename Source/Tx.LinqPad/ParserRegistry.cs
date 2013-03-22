// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reflection;
using System.Text;

namespace Tx.LinqPad
{
    internal class ParserRegistry
    {
        private readonly MethodInfo[] _addFiles;
        private readonly MethodInfo[] _addSessions;

        public ParserRegistry()
        {
            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            IEnumerable<Type> types = from file in Directory.GetFiles(dir, "*.dll")
                                      from t in Assembly.LoadFrom(file).GetTypes()
                                      where t.IsPublic
                                      select t;

            IEnumerable<MethodInfo> methods = from t in types
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

        public string Filter
        {
            get
            {
                FileParserAttribute[] attributes = (from mi in _addFiles
                                                    select mi.GetAttribute<FileParserAttribute>()).ToArray();

                var sb = new StringBuilder("All Files|");
                foreach (string ext in attributes.SelectMany(a => a.Extensions))
                {
                    sb.Append('*');
                    sb.Append(ext);
                    sb.Append(';');
                }
                foreach (FileParserAttribute a in attributes)
                {
                    sb.Append('|');
                    sb.Append(a.Description);
                    sb.Append('|');

                    foreach (string ext in a.Extensions)
                    {
                        sb.Append('*');
                        sb.Append(ext);
                        sb.Append(';');
                    }
                }

                return sb.ToString().Replace(";|", "|");
            }
        }

        public IEnumerable<Assembly> GetAssemblies()
        {
            return (from m in _addFiles select m.DeclaringType.Assembly).Distinct();
        }

        public IEnumerable<string> GetNamespaces()
        {
            return (from m in _addFiles select m.DeclaringType.Namespace).Distinct();
        }

        public Dictionary<Type, long> GetTypeStatistics(Type[] types, string[] files)
        {
            var stat = new TypeOccurenceStatistics(types);
            AddFiles(stat, files);
            stat.Run();

            return stat.Statistics;
        }

        public void AddFiles(IPlaybackConfiguration playback, string[] files)
        {
            var filesByExtension = new Dictionary<string, List<string>>();
            foreach (string f in files)
            {
                string extension = Path.GetExtension(f).ToLowerInvariant();

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
                                        where mi.GetAttribute<FileParserAttribute>().Extensions.Contains(ext)
                                        select mi).FirstOrDefault();

                addMethod.Invoke(null, new object[] {playback, filesByExtension[ext].ToArray()});
            }
        }

        public void AddSession(IPlaybackConfiguration playback, string session)
        {
            foreach (MethodInfo addMethod in _addSessions)
            {
                addMethod.Invoke(null, new object[] {playback, session});
            }
        }

        private T GetAttribute<T>(ICustomAttributeProvider provider)
        {
            return (T) (provider.GetCustomAttributes(typeof (T), false))[0];
        }
    }
}