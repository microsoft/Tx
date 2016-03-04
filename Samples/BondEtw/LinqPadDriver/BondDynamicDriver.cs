namespace BondInEtwLinqpadDriver
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Subjects;
    using System.Reflection;
    using System.Text;
    using System.Threading;

    using BondInEtwDriver;

    using LINQPad.Extensibility.DataContext;

    using Microsoft.CSharp;

    using Tx.Binary;
    using Tx.Bond;
    using Tx.Bond.Extensions;

    using Expression = System.Linq.Expressions.Expression;
    using System.Windows;
    using Bond;

    using Tx.Bond.LinqPad;

    /// <summary>
    /// Dynamic driver class.
    /// </summary>
    public sealed class BondDynamicDriver : DynamicDataContextDriver
    {
        private readonly List<Assembly> assemblies = new List<Assembly>
        {
            typeof (ObservableCollection<>).Assembly, // System
            typeof (Expression).Assembly, // System.Core
            typeof (ISubject<>).Assembly, // System.Reactive.Interfaces
            typeof (Observer).Assembly, // System.Reactive.Core
            typeof (Subject<>).Assembly, // System.Reactive.Linq
            typeof (ThreadPoolScheduler).Assembly, // System.Reactive.PlatformServices
            typeof (Playback).Assembly, // Tx.Core
            typeof (Attribute).Assembly, // mscorlib
            typeof (BondDataType).Assembly, // Bond
            typeof (RequiredAttribute).Assembly // Bond.Attributes
        };

        private readonly List<string> namespaces = new List<string>
        {
            @"System",
            @"System.Linq",
            @"System.Linq.Expressions",
            @"System.Reactive",
            @"System.Reactive.Linq"
        };

        private const string DataContextTemplate = @"namespace System.Reactive.Tx
              {
                    using System;
                    using System.Linq;
                    using System.Linq.Expressions;
                    using System.Collections.Generic;
                    using System.Reactive;
                    using System.Reactive.Linq;
                    [usings]

                    public class PlaybackClass
                    {
                        Playback _playback;

                        public PlaybackClass(Playback playback)
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

        /// <summary>
        /// Memory slot to store local data.
        /// </summary>
        private static readonly LocalDataStoreSlot LocalDataStoreSlot;
        private readonly TypeCache _typeCache;
        
        /// <summary>
        /// Static constructor
        /// </summary>
        static BondDynamicDriver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

            LocalDataStoreSlot = Thread.AllocateDataSlot();
        }

        public BondDynamicDriver()
        {
            _typeCache = new TypeCache();
        }

        /// <summary>
        /// Gets a user friendly name for the driver.
        /// </summary>
        public override string Name
        {
            get { return "Self-contained Bond-In-ETW driver"; }
        }

        /// <summary>
        /// Gets author's name.
        /// </summary>
        public override string Author
        {
            get
            {
                return @"Sergey Baranchenkov; Swetha Machanavajhala";
            }
        }

        public override Version Version
        {
            get
            {
                return new Version(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion);
            }
        }

        /// <summary>
        /// Get connection details.
        /// </summary>
        /// <param name="cxInfo"> Connection information, as entered by the user. </param>
        /// <returns> text to display in the root Schema Explorer node for a given connection information. </returns>
        public override string GetConnectionDescription(IConnectionInfo cxInfo)
        {
            var bondInEtwProperties = new BondInEtwProperties(cxInfo);
            return bondInEtwProperties.ContextName;
        }

        /// <summary>
        /// Displays a dialog prompting the user for connection details.
        /// </summary>
        /// <param name="cxInfo"> Connection information, as entered by the user. </param>
        /// <param name="isNewConnection"> The isNewConnection parameter will be true if the user 
        /// is creating a new connection rather than editing an existing connection. </param>
        /// <returns> True if the user clicked OK. If it returns false, any changes to the IConnectionInfo object will be rolled back. </returns>
        public override bool ShowConnectionDialog(IConnectionInfo cxInfo, bool isNewConnection)
        {
            var bondInEtwProperties = new BondInEtwProperties(cxInfo);
            return new ConnectionDialog(bondInEtwProperties).ShowDialog() ?? false;
        }

        /// <summary>
        /// Returns a list of additional assemblies to reference when building queries. 
        /// </summary>
        /// <param name="cxInfo"> Connection information, as entered by the user. </param>
        /// <returns> List of additional assemblies. </returns>
        public override IEnumerable<string> GetAssembliesToAdd(IConnectionInfo cxInfo)
        {
            var bondInEtwProperties = new BondInEtwProperties(cxInfo);
            IEnumerable<string> result = null;

            try
            {
                var driverDirectory = Path.GetDirectoryName(Assembly.GetAssembly(this.GetType()).Location);

                result = GetAssemblyTypes(driverDirectory)
                    .Concat(TypeCache.GetTypes(bondInEtwProperties.ContextName))
                    .Select(type => type.Assembly)
                    .Concat(assemblies)
                    .Distinct()
                    .Select(assembly => assembly.Location)
                    .ToList();      
            }
            catch
            {
                // Ignore
            }

            return result;
        }       

        /// <summary>
        /// Returns a list of additional namespaces that should be imported automatically into all 
        /// queries that use this driver.
        /// </summary>
        /// <param name="cxInfo"> Connection information, as entered by the user. </param>
        /// <returns> List of additional namespaces. </returns>
        public override IEnumerable<string> GetNamespacesToAdd(IConnectionInfo cxInfo)
        {
            var bondInEtwProperties = new BondInEtwProperties(cxInfo);

            IEnumerable<string> result = null;

            try
            {
                var driverDirectory = Path.GetDirectoryName(Assembly.GetAssembly(this.GetType()).Location);

                result = GetAssemblyTypes(driverDirectory)
                    .Concat(TypeCache.GetTypes(bondInEtwProperties.ContextName))
                    .Select(type => type.Namespace)
                    .Where(@namespace => @namespace != null)
                    .Concat(namespaces)
                    .Distinct();
            }
            catch
            {
                // Ignore
            }

            return result;
        }

        /// <summary>
        /// Returns names and types of parameters that should be passed into the data context constructor.
        /// </summary>
        /// <param name="cxInfo"> Connection information, as entered by the user. </param>
        /// <returns> Names and types of parameters that should be passed into the data context constructor. </returns>
        public override ParameterDescriptor[] GetContextConstructorParameters(IConnectionInfo cxInfo)
        {
            var parameter = new ParameterDescriptor("playback", typeof (Playback).FullName);
            return new[] { parameter };
        }

        /// <summary>
        /// Returns argument values to pass to into the data context's constructor, based on a given connection info.
        /// </summary>
        /// <param name="cxInfo"> Connection information, as entered by the user. </param>
        /// <returns> Argument values to pass to into the data context's constructor. </returns>
        public override object[] GetContextConstructorArguments(IConnectionInfo cxInfo)
        {
            var playback = new Playback();

            var bondInEtwProperties = new BondInEtwProperties(cxInfo);

            try
            {
                var bondTypemap = new GeneralPartitionableTypeMap();

                var typeMaps = TypeFinder
                    .LoadTypeMaps(TypeCache.ResolveCacheDirectory(bondInEtwProperties.ContextName))
                    .Where(i => i != typeof(GeneralPartitionableTypeMap))
                    .Select(Activator.CreateInstance)
                    .OfType<ITypeMap<BinaryEnvelope>>()
                    .Concat(new[] { bondTypemap })
                    .ToArray();

                foreach (var file in bondInEtwProperties.Files)
                {
                    var file1 = file;

                    ((IPlaybackConfiguration)playback).AddInput(
                        () => BinaryEtwObservable.FromFiles(file1),
                        typeMaps);
                }

                Thread.SetData(LocalDataStoreSlot, playback);
                return new object[] { playback };
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, "Error compiling generated code.");
            }

            return new object[0];
        }

        /// <summary>
        /// Builds an assembly containing a typed data context, and returns data for the Schema Explorer.
        /// </summary>
        /// <param name="cxInfo"> Connection information, as entered by the user </param>
        /// <param name="assemblyToBuild"> Name and location of the target assembly to build </param>
        /// <param name="nameSpace"> The suggested namespace of the typed data context. You must update this
        /// parameter if you don't use the suggested namespace. </param>
        /// <param name="typeName"> The suggested type name of the typed data context. You must update this
        /// parameter if you don't use the suggested type name. </param>
        /// <returns> Schema which will be subsequently loaded into the Schema Explorer. </returns>
        public override List<ExplorerItem> GetSchemaAndBuildAssembly(
            IConnectionInfo cxInfo, AssemblyName assemblyToBuild, ref string nameSpace, ref string typeName)
        {
            nameSpace = @"System.Reactive.Tx";
            typeName = "PlaybackClass";

            var sourceCode = new List<string>();
            var sbContextUsings = new StringBuilder();
            var sbContextProperties = new StringBuilder();

            var dataContext = DataContextTemplate.Replace(@"[usings]", sbContextUsings.ToString())
                                                 .Replace(@"[properties]", sbContextProperties.ToString());

            var bondInEtwProperties = new BondInEtwProperties(cxInfo);
            _typeCache.Init(bondInEtwProperties.ContextName, bondInEtwProperties.Files);
         
            sourceCode.Add(dataContext);

            //Build Assembly - CSharpCodeProvider to compile generated code.

            var outputName = assemblyToBuild.CodeBase;

            using (
                var codeProvider =
                    new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v4.0" } }))
            {
                string[] assemblies = this.GetAssembliesToAdd(cxInfo).ToArray();

                var compilerOptions = new CompilerParameters(assemblies, outputName, true);

                var results = codeProvider.CompileAssemblyFromSource(compilerOptions, sourceCode.ToArray());

                if (results.Errors.Count > 0)
                {
                    var sbErrors = new StringBuilder();

                    foreach (var error in results.Errors)
                    {
                        sbErrors.Append(error);
                    }

                    MessageBox.Show(sbErrors.ToString(), "Error compiling generated code.");
                }
            }

//            var bondInEtwRegistry = new BondInEtwRegistry(tempFolder);
//            var stat = bondInEtwRegistry.GetTypeStatistics(bondInEtwProperties.Files);

            var controller = new EventStatisticController(_typeCache.CacheDirectory);
            var stat = controller.GetTypeStatistics(_typeCache, bondInEtwProperties.Files[0]);

            return this.CreateEventTree(stat);
        }

        /// <summary>
        /// This method is called after the query's main thread has finished running the user's code,
        /// but before the query has stopped. If you've spun up threads that are still writing results, you can 
        /// use this method to wait out those threads.
        /// </summary>
        /// <param name="cxInfo"> Connection information, as entered by the user. </param>
        /// <param name="context"> Context </param>
        /// <param name="executionManager"> execution manager </param>
        public override void OnQueryFinishing(
            IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
        {
            var playback = (Playback)context.GetType().GetProperty("playback").GetValue(context, new object[] { });

            if (playback != null)
            {
                playback.Run();
            }
        }

        /// <summary>
        /// Create schema tree that appears on the left pane below the driver connection name.
        /// </summary>
        /// <param name="stat"> Available event types and statistics. </param>
        /// <returns> Tree of available types and their statistics. </returns>
        private List<ExplorerItem> CreateEventTree(Dictionary<Type, EventStatistics> stat)
        {
            var result = new List<ExplorerItem>();

            KeyValuePair<Type, EventStatistics>[] eventTypes =
                (from pair in stat orderby pair.Key.Name select pair).ToArray();

            ExplorerItem scope = null;
            string currentName = null;

            foreach (var eventType in eventTypes)
            {
                if (eventType.Key.Name == currentName)
                {
                    continue;
                }

                if (scope == null)
                {
                    scope = new ExplorerItem(eventType.Key.Name, ExplorerItemKind.Schema, ExplorerIcon.Schema)
                    {
                        Children = new List<ExplorerItem>()
                    };

                    result.Add(scope);
                    currentName = eventType.Key.Name;
                }
                result = scope.Children;
            }

            // Defining the features for each event type.
            foreach (var pair in eventTypes)
            {
                ExplorerItem eventType;

                if (pair.Value.EventsPerSecond == 0 && pair.Value.AverageByteSize == 0)
                {
                    eventType = new ExplorerItem(pair.Key.Name, ExplorerItemKind.QueryableObject, ExplorerIcon.Table)
                    {
                        ToolTipText =
                            "Statistics " + "\n" + "Occurences: " + pair.Value.EventCount + "\n" + "ByteSize: " + pair.Value.ByteSize + "\n",

                        DragText = "playback.GetObservable<" + pair.Key.Name + ">()",
                        Children = new List<ExplorerItem>(),
                        IsEnumerable = true
                    };
                }
                else
                {
                    eventType = new ExplorerItem(pair.Key.Name, ExplorerItemKind.QueryableObject, ExplorerIcon.Table)
                    {
                        ToolTipText =
                            "Statistics " + "\n" + "Occurences: " + pair.Value.EventCount + "\n" + "Events per second: " + pair.Value.EventsPerSecond + "\n" +
                                "ByteSize: " + pair.Value.ByteSize + "\n" + "Average Byte Size: " + pair.Value.AverageByteSize + "\n",


                        DragText = "playback.GetObservable<" + pair.Key.Name + ">()",
                        Children = new List<ExplorerItem>(),
                        IsEnumerable = true
                    };
                }

                scope.Children.Add(eventType);

                // Get the nested properties (columns) for each event type.

                foreach (var property in pair.Key.GetProperties())
                {
                    var propertyType = new ExplorerItem(property.Name, ExplorerItemKind.Property, ExplorerIcon.Column)
                    {
                        ToolTipText = "Property Type: " + property.PropertyType.Name,
                        DragText = property.Name,
                        Children = new List<ExplorerItem>(),
                        IsEnumerable = true
                    };

                    eventType.Children.Add(propertyType);

                    if (!(property.PropertyType.IsPrimitive || property.PropertyType.Namespace.Contains("System")))
                    {
                        this.AddNestedPropertyToTree(property.PropertyType.GetProperties(), propertyType);
                    }
                }
            }

            return result;
        }

        private static Type[] GetAssemblyTypes(string targetPath)
        {
            var assemblies = new[] { @"*.dll" }.
                SelectMany(i => Directory.GetFiles(targetPath, i)).ToArray();

            var types = new List<Type>();
            foreach (var assemblyName in assemblies)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(assemblyName);
                    types.AddRange(assembly.GetTypes());
                }
                catch (ReflectionTypeLoadException exception)
                {
                    var stringBuilder = new StringBuilder();

                    stringBuilder.AppendLine(exception.ToString());

                    foreach (var loaderException in exception.LoaderExceptions)
                    {
                        stringBuilder.AppendLine(loaderException.ToString());
                    }

                    MessageBox.Show(stringBuilder.ToString());
                    throw;
                }
                catch (BadImageFormatException e)
                {
                    // e.g. x64 assemblies.
                    MessageBox.Show(e.ToString());
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    throw;
                }
            }

            var assemblyTypes = types.ToArray();
            return assemblyTypes;
        }

        private void AddNestedPropertyToTree(PropertyInfo[] property, ExplorerItem propertyType)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            if (propertyType == null)
            {
                throw new ArgumentNullException("propertyType");
            }

            var oldPropertyType = propertyType;

            foreach (var prop in property)
            {
                propertyType = new ExplorerItem(prop.Name, ExplorerItemKind.Property, ExplorerIcon.Column)
                {
                    ToolTipText = "Property Type: " + prop.PropertyType.Name,
                    DragText = prop.Name,
                    Children = new List<ExplorerItem>(),
                    IsEnumerable = true
                };

                oldPropertyType.Children.Add(propertyType);

                if (!(prop.PropertyType.IsPrimitive || prop.PropertyType.Namespace.Contains("System")))
                {
                    this.AddNestedPropertyToTree(prop.PropertyType.GetProperties(), propertyType);
                }
            }
        }

        /// <summary>
        /// Needed to reference other assemblies that are not part of the .NET framework.
        /// </summary>
        /// <param name="sender"> sender </param>
        /// <param name="resolveEventArgs"> args </param>
        /// <returns> Resolved references. </returns>
        private static Assembly ResolveAssembly(object sender, ResolveEventArgs resolveEventArgs)
        {
            var assemblyname = resolveEventArgs.Name.Substring(0, resolveEventArgs.Name.IndexOf(',')) + @".dll";

            // System.Reactive.Debugger.dll is something that wont find hence return null
            if (string.Compare(assemblyname, @"system.reactive.debugger.dll", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return null;
            }

            var driverDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (driverDirectory != null)
            {
                var assemblies = Directory.EnumerateFiles(driverDirectory, assemblyname);

                foreach (var path in assemblies)
                {
                    return Assembly.LoadFrom(path);
                }

                var root = Path.Combine(Path.GetTempPath(), @"LINQPad\");

                assemblies = Directory.EnumerateFiles(root, assemblyname, SearchOption.AllDirectories);

                foreach (var path in assemblies)
                {
                    return Assembly.LoadFrom(path);
                }

                var dataContextFolder = AppDomain.CurrentDomain.BaseDirectory;

                assemblies = Directory.EnumerateFiles(dataContextFolder, assemblyname, SearchOption.AllDirectories);

                return assemblies
                    .Select(Assembly.LoadFrom)
                    .FirstOrDefault();
            }

            return null;
        }

    }
}
