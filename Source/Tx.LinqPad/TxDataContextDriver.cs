// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using LINQPad.Extensibility.DataContext;
using Microsoft.CSharp;
using Expression = System.Linq.Expressions.Expression;

namespace Tx.LinqPad
{
    public sealed class TxDataContextDriver : DynamicDataContextDriver
    {
        private const string DataContextTemplate =
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

        private static readonly LocalDataStoreSlot _threadStorageSlot;
        private readonly ParserRegistry _parserRegistry;
        private readonly TypeCache _typeCache;

        static TxDataContextDriver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
            _threadStorageSlot = Thread.AllocateDataSlot();

            CopySampleTraces();
        }

        public TxDataContextDriver()
        {
            _typeCache = new TypeCache();
            _parserRegistry = new ParserRegistry();
        }

        public override string Author
        {
            get { return "Microsoft Open Technologies, Inc."; }
        }

        public override bool DisallowQueryDisassembly
        {
            get { return true; }
        }

        public override string Name
        {
            get { return "Tx (LINQ to Logs and Traces)"; }
        }

        public override IEnumerable<string> GetAssembliesToAdd(IConnectionInfo cxn)
        {
            var assemblies = new List<Assembly>
                {
                    typeof (ObservableCollection<>).Assembly, // System
                    typeof (Expression).Assembly, // System.Core
                    typeof (ISubject<>).Assembly, // System.Reactive.Interfaces
                    typeof (Observer).Assembly, // System.Reactive.Core
                    typeof (Subject<>).Assembly, // System.Reactive.Linq
                    typeof (ThreadPoolScheduler).Assembly, // System.Reactive.PlatformServices
                    typeof (ControlObservable).Assembly, // System.Reactive.Windows.Forms
                    typeof (Playback).Assembly, // Tx.Core
                };

            var properties = new TxProperties(cxn);
            assemblies.AddRange(_parserRegistry.GetAssemblies());

            var assemblyNames = new List<string>(from a in assemblies select a.Location);

            _typeCache.Init(properties.ContextName);
            assemblyNames.AddRange(_typeCache.GetAssemblies(properties.ContextName,
                                                         ReplaceSampleTracesDir(properties.Files),
                                                         ReplaceSampleTracesDir(properties.MetadataFiles)));

            return assemblyNames;
        }

        public override IEnumerable<string> GetNamespacesToAdd(IConnectionInfo cxInfo)
        {
            var namespaces = new List<string>
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
            var properties = new TxProperties(cxInfo);
            return properties.ContextName;
        }

        public override bool ShowConnectionDialog(IConnectionInfo cxInfo, bool isNewConnection)
        {
            try
            {
                var properties = new TxProperties(cxInfo);
                return new ConnectionDialog(properties, _parserRegistry.Filter).ShowDialog() ?? false;
            }
            catch (Exception error)
            {
                TxEventSource.Log.TraceError(error.ToString());

                MessageBox.Show(error.ToString(), "ShowConnectionDialog");

                return false;
            }
        }

        public override ParameterDescriptor[] GetContextConstructorParameters(IConnectionInfo cxInfo)
        {
            var desc = new ParameterDescriptor("playback", typeof (Playback).FullName);
            return new[] {desc};
        }

        public override object[] GetContextConstructorArguments(IConnectionInfo cxInfo)
        {
            var playback = new Playback();
            var properties = new TxProperties(cxInfo);

            if (properties.IsRealTime)
            {
                _parserRegistry.AddSession(playback, properties.SessionName);
            }
            else
            {
                _parserRegistry.AddFiles(playback, ReplaceSampleTracesDir(properties.Files));
            }

            Thread.SetData(_threadStorageSlot, playback);
            return new[] {playback};
        }

        public override List<ExplorerItem> GetSchemaAndBuildAssembly(IConnectionInfo cxInfo,
                                                                     AssemblyName assemblyToBuild, ref string nameSpace,
                                                                     ref string typeName)
        {
            nameSpace = "System.Reactive.Tx";
            typeName = "StreamScopeWrapper";

            var allGeneratedSources = new List<string>();
            var sbContextUsings = new StringBuilder();
            var sbContextProperties = new StringBuilder();

            var properties = new TxProperties(cxInfo);
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
            using (
                var codeProvider = new CSharpCodeProvider(new Dictionary<string, string> {{"CompilerVersion", "v4.0"}}))
            {
                string[] assemblies = GetAssembliesToAdd(cxInfo).ToArray();

                var compilerOptions = new CompilerParameters(assemblies, outputName, true);

                results = codeProvider.CompileAssemblyFromSource(compilerOptions, allGeneratedSources.ToArray());

                if (results.Errors.Count > 0)
                {
                    var sbErr = new StringBuilder();
                    foreach (object o in results.Errors)
                    {
                        sbErr.AppendLine(o.ToString());
                    }
                    // Is there any better troubleshooting mechanism? 
                    MessageBox.Show(sbErr.ToString(), "Error compiling generated code");
                }
            }

            Dictionary<Type, long> stat = _parserRegistry.GetTypeStatistics(
                _typeCache.GetAvailableTypes(
                    properties.ContextName,
                    ReplaceSampleTracesDir(properties.Files),
                    ReplaceSampleTracesDir(properties.MetadataFiles)),
                ReplaceSampleTracesDir(properties.Files));

            return CreateTree(stat);
        }

        private List<ExplorerItem> CreateTree(Dictionary<Type, long> stat)
        {
            var result = new List<ExplorerItem>();

            KeyValuePair<Type, long>[] x = (from pair in stat
                                            orderby pair.Key.Namespace, pair.Key.Name
                                            select pair).ToArray();

            string currentNamespace = null;
            ExplorerItem scope = null;
            foreach (var pair in x)
            {
                if (pair.Key.Namespace != currentNamespace)
                {
                    scope = CreateNamespace(pair.Key.Namespace, result);
                    currentNamespace = pair.Key.Namespace;
                }
                ;

                var eventType = new ExplorerItem(pair.Key.Name, ExplorerItemKind.QueryableObject, ExplorerIcon.Table);
                eventType.ToolTipText = "Occurences: " + pair.Value + "\n";
                foreach (object a in pair.Key.GetCustomAttributes(false).OrderBy(a => a.GetType().Name))
                {
                    eventType.ToolTipText += '\n' + a.ToString() + '\n';
                }

                eventType.DragText = "playback.GetObservable<" + pair.Key.FullName + ">()";
                eventType.Children = new List<ExplorerItem>();
                scope.Children.Add(eventType);

                foreach (PropertyInfo p in pair.Key.GetProperties())
                {
                    var field = new ExplorerItem(p.Name, ExplorerItemKind.Property, ExplorerIcon.Column);
                    eventType.Children.Add(field);
                    field.ToolTipText = p.PropertyType.Name;
                }
            }

            return result;
        }

        private ExplorerItem CreateNamespace(string name, List<ExplorerItem> root)
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

        public override void OnQueryFinishing(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
        {
            var playback = (Playback)context
                .GetType()
                .GetProperty("playback")
                .GetValue(context, new object[] {});

            if (playback != null)
                playback.Run();
        }

        private static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                string assemblyname = args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll";
                string driverDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                IEnumerable<string> assemblies = Directory.EnumerateFiles(driverDir, assemblyname);
                foreach (string path in assemblies)
                {
                    return DataContextDriver.LoadAssemblySafely(path);
                }

                string root = Path.Combine(Path.GetTempPath(), @"LINQPad\");
                assemblies = Directory.EnumerateFiles(root, assemblyname, SearchOption.AllDirectories);
                foreach (string path in assemblies)
                {
                    return DataContextDriver.LoadAssemblySafely(path);
                }
            }
            catch (Exception error)
            {
                TxEventSource.Log.TraceError(error.ToString());
            }

            return null;
        }

        private string[] ReplaceSampleTracesDir(string[] files)
        {
            string prefix = "($SampleTraces)";
            string samplrDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var result = new List<string>();

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

        private static void CopySampleTraces()
        {
            string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string txDir = Path.Combine(myDocuments, @"LINQPad Queries\Tx");

            if (!Directory.Exists(txDir))
                Directory.CreateDirectory(txDir);

            string sourceDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            List<string> files = new List<string>(Directory.GetFiles(sourceDir,"*.etl"));
            files.AddRange(Directory.GetFiles(sourceDir,"*.man"));
            files.AddRange(Directory.GetFiles(sourceDir,"*.blg"));
            files.AddRange(Directory.GetFiles(sourceDir,"*.xel"));

            foreach (string file in files)
            {
                string target = Path.Combine(txDir, Path.GetFileName(file));
                File.Copy(file, target, true);
            }
        }

    }
}