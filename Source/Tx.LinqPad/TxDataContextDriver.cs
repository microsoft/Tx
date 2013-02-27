// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Tx.LinqPad
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using LINQPad.Extensibility.DataContext;
    using Microsoft.CSharp;
    using Microsoft.Win32;
    using System.Reactive.Subjects;
    using System.Reactive;
    using System.Reactive.Linq;

    public sealed class TxDataContextDriver : DynamicDataContextDriver
    {
        const string DataContextTemplate =
@"namespace System.Reactive.Tx
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Reactive.Linq;
    [usings]

    public class StreamScopeWrapper
    {
        Playback _playback;

        public StreamScopeWrapper(Playback playback)
        {
            _playback = playback;
        }

        public Playback playback
        {
            get { return _playback; }
        }

        [properties]
    }
}";

        static readonly LocalDataStoreSlot _threadStorageSlot;
        TypeCache _typeCache;
        ParserRegistry _parserRegistry;

        static TxDataContextDriver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += TxDataContextDriver.AssemblyResolve;
            _threadStorageSlot = Thread.AllocateDataSlot();
        }

        public TxDataContextDriver()
        {
            _typeCache = new TypeCache();
            _parserRegistry = new ParserRegistry();
        }

        public override string Author
        {
            get { return "MS Open Tech"; }
        }

        public override bool DisallowQueryDisassembly
        {
            get { return true; }
        }

        public override string Name
        {
            get { return "Tx (LINQ to Traces)"; }
        }

        public override IEnumerable<string> GetAssembliesToAdd(IConnectionInfo cxn)
        {
            List<Assembly> assemblies = new List<Assembly>()
            {
                typeof(ObservableCollection<>).Assembly, // System
                typeof(Expression).Assembly, // System.Core
                typeof(ISubject<>).Assembly, // System.Reactive.Interfaces
                typeof(Observer).Assembly,   // System.Reactive.Core
                typeof(Subject<>).Assembly,  // System.Reactive.Linq
                typeof(Playback).Assembly,   // Tx.Core
            };

            TxProperties properties = new TxProperties(cxn);
            assemblies.AddRange(_parserRegistry.GetAssemblies());

            _typeCache.Init(properties.ContextName);
            assemblies.AddRange(_typeCache.GetAssemblies(properties.ContextName, 
                ReplaceSampleTracesDir(properties.Files), 
                ReplaceSampleTracesDir(properties.MetadataFiles)));

            return from a in assemblies select a.Location;
        }

        public override IEnumerable<string> GetNamespacesToAdd(IConnectionInfo cxInfo)
        {
            List<string> namespaces = new List<string>()
            {
               "System",
               "System.Linq",
               "System.Linq.Expressions",
               "System.Reactive",
               "System.Reactive.Linq",
            };

            return namespaces.Concat(_parserRegistry.GetNamespaces());
        } 
        
        public override string GetConnectionDescription(IConnectionInfo cxInfo)
        {
            TxProperties properties = new TxProperties(cxInfo);
            return properties.ContextName;
        }

        public override bool ShowConnectionDialog(IConnectionInfo cxInfo, bool isNewConnection)
        {
            TxProperties properties = new TxProperties(cxInfo);
            return new ConnectionDialog(properties, _parserRegistry.Filter).ShowDialog() ?? false;
        }

        public override ParameterDescriptor[] GetContextConstructorParameters(IConnectionInfo cxInfo)
        {
            ParameterDescriptor desc = new ParameterDescriptor("playback", typeof(Playback).FullName);
            return new ParameterDescriptor[] { desc };
        }

        public override object[] GetContextConstructorArguments(IConnectionInfo cxInfo)
        {
            Playback playback = new Playback();
            TxProperties properties = new TxProperties(cxInfo);

            if (properties.IsRealTime)
            {
                _parserRegistry.AddSession(playback, properties.SessionName);
            }
            else
            {
                _parserRegistry.AddFiles(playback, ReplaceSampleTracesDir(properties.Files));
            }

            Thread.SetData(_threadStorageSlot, playback);
            return new[] { playback };
        }

        public override List<ExplorerItem> GetSchemaAndBuildAssembly(IConnectionInfo cxInfo, AssemblyName assemblyToBuild, ref string nameSpace, ref string typeName)
        {
            nameSpace = "System.Reactive.Tx";
            typeName = "StreamScopeWrapper";

            List<string> allGeneratedSources = new List<string>();
            StringBuilder sbContextUsings = new StringBuilder();
            StringBuilder sbContextProperties = new StringBuilder();

            TxProperties properties = new TxProperties(cxInfo);
            _typeCache.Init(properties.ContextName);

            if (properties.IsUsingDirectoryLookup)
            {
                _typeCache.BuildCache(properties.ContextName, properties.Files, properties.MetadataDirectory);
            }
            else
            {
                _typeCache.BuildCache(
                    properties.ContextName, 
                    ReplaceSampleTracesDir(properties.Files),
                    ReplaceSampleTracesDir(properties.MetadataFiles));
            }

            string dataContext = DataContextTemplate
                .Replace("[usings]", sbContextUsings.ToString())
                .Replace("[properties]", sbContextProperties.ToString());
            allGeneratedSources.Add(dataContext);

            CompilerResults results;
            string outputName = assemblyToBuild.CodeBase;
            using (var codeProvider = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } }))
            {
                string[] assemblies = GetAssembliesToAdd(cxInfo).ToArray();

                var compilerOptions = new CompilerParameters(assemblies, outputName, true);

                results = codeProvider.CompileAssemblyFromSource(compilerOptions, allGeneratedSources.ToArray());

                if (results.Errors.Count > 0)
                {
                    StringBuilder sbErr = new StringBuilder();
                    foreach (object o in results.Errors)
                    {
                        sbErr.AppendLine(o.ToString());
                    }
                    // Is there any better troubleshooting mechanism? 
                    System.Windows.MessageBox.Show(sbErr.ToString(), "Error compiling generated code");
                }
            }

            Dictionary<Type, long> stat =  _parserRegistry.GetTypeStatistics(
                _typeCache.GetAvailableTypes(
                    properties.ContextName,
                    ReplaceSampleTracesDir(properties.Files), 
                    ReplaceSampleTracesDir(properties.MetadataFiles)), 
                ReplaceSampleTracesDir(properties.Files));

            return CreateTree(stat);
        }

        List<ExplorerItem> CreateTree(Dictionary<Type, long> stat)
        {
            List<ExplorerItem> result = new List<ExplorerItem>();

            var x = (from pair in stat
                     orderby pair.Key.Namespace, pair.Key.Name
                     select pair).ToArray();

            string currentNamespace = null;
            ExplorerItem scope = null;
            foreach(KeyValuePair<Type, long> pair in x)
            {
                if (pair.Key.Namespace != currentNamespace)
                {
                    scope = CreateNamespace(pair.Key.Namespace, result);
                    currentNamespace = pair.Key.Namespace;
                };

                ExplorerItem eventType = new ExplorerItem(pair.Key.Name, ExplorerItemKind.QueryableObject, ExplorerIcon.Table);
                eventType.ToolTipText = "Occurences: " + pair.Value + "\n";
                foreach (var a in pair.Key.GetCustomAttributes(false).OrderBy(a=>a.GetType().Name))
                {
                    eventType.ToolTipText += '\n' + a.ToString() + '\n';
                }

                eventType.DragText = "playback.GetObservable<" + pair.Key.FullName + ">()";
                eventType.Children = new List<ExplorerItem>();
                scope.Children.Add(eventType);
 
                foreach (PropertyInfo p in pair.Key.GetProperties())
                {
                    ExplorerItem field = new ExplorerItem(p.Name, ExplorerItemKind.Property, ExplorerIcon.Column);
                    eventType.Children.Add(field);
                    field.ToolTipText = p.PropertyType.Name;
                }
            }

            return result;
        }

        ExplorerItem CreateNamespace(string name, List<ExplorerItem> root)
        {
            ExplorerItem item = null;
            List<ExplorerItem> currentScope = root;
            string[] tokens = name.Split('.');

            for (int i = 1; i < tokens.Length; i++)
            {
                string token = tokens[i];
                item = currentScope.FirstOrDefault(ei => ei.Text == token);
                if (item == null)
                {
                    item = new ExplorerItem(token, ExplorerItemKind.Schema, ExplorerIcon.Schema);
                    item.Children = new List<ExplorerItem>();
                    currentScope.Add(item);
                }
                currentScope = item.Children;
            }

            return item;
        }

        public override void PreprocessObjectToWrite(ref object objectToWrite, ObjectGraphInfo info)
        {
            if (null == objectToWrite)
                return;

            Type type = objectToWrite.GetType();
            if (type.IsGenericType && type.GetInterface(typeof(IObservable<>).Name) != null)
            {
                Type[] genericArguments = type.GetGenericArguments();
                Type eventType = genericArguments[genericArguments.Length - 1];

                MethodInfo process = this.GetType().GetMethod("RunSingleOutput", BindingFlags.Static | BindingFlags.Public);
                process = process.MakeGenericMethod(eventType);
                objectToWrite = process.Invoke(null, new object[] { objectToWrite });
                return;
            }
        }

        //// this implements auto-start in "C# Expression" mode
        //// In ohter modes Run or Start must be called from the queries
        public static IEnumerable<T> RunSingleOutput<T>(IObservable<T> output)
        {
            var playback = (Playback)Thread.GetData(_threadStorageSlot);
            var list = playback.BufferOutput(output);

            playback.Run();

            return list;
        }

        static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string assemblyname = args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll";
            string root = System.IO.Path.Combine(System.IO.Path.GetTempPath(), @"LINQPad\");
            var assemblies = Directory.EnumerateFiles(root, assemblyname, SearchOption.AllDirectories);
            foreach (string path in assemblies)
            {
                return Assembly.LoadFrom(path);
            }

            return null;
        }

        string[] ReplaceSampleTracesDir(string[] files)
        {
            string prefix="($SampleTraces)";
            string samplrDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            List<string> result = new List<string>();

            foreach (string f in files)
            {
                if (f.StartsWith(prefix))
                {
                    result.Add(Path.Combine(samplrDir, f.Substring(prefix.Length)));
                }
                else
                {
                    result.Add(f);
                }
            }

            return result.ToArray();
        }

    }
}
