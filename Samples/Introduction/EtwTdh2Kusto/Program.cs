using Kusto.Data.Common;
using Kusto.Data.Net.Client;
using System;
using System.IO;
using System.Reactive.Linq;
using Tx.Windows;

namespace Etw2Kusto
{
    class Program
    {
        static string _cluster;
        static string _database;
        static string _tableName;
        static string _filePattern;
        static string _sessionName;
        static string _connectionString;
        static void Main(string[] args)
        {
            ParseArgs(args);

            ResetTable(_connectionString, _tableName, typeof(EtwEvent));

            try
            {
                if (_filePattern != null)
                    UploadFiles();

                else if (_sessionName != null)
                    UploadRealTime();

                else ExitWithMissingArgument("file or session");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

        }

        static void UploadFiles()
        {
            string dir = Path.GetDirectoryName(Path.GetFullPath(_filePattern));
            string pattern = Path.GetFileName(_filePattern);
            string[] files = Directory.GetFiles(dir, pattern);

            var etw = EtwTdhObservable.FromFiles(files);
            var transformed = etw
                .Select(e => new EtwEvent(e));

            var ku = new BlockingKustoUploader<EtwEvent>(
                _connectionString, _tableName, 10000, TimeSpan.MaxValue);

            using (transformed.Subscribe(ku))
            {
                ku.Completed.WaitOne();
            }
        }

        static void UploadRealTime()
        {
            var etw = EtwTdhObservable.FromSession(_sessionName);
            var transformed = etw
                .Select(e => new EtwEvent(e));

            var ku = new BlockingKustoUploader<EtwEvent>(
                _connectionString, _tableName, 10000, TimeSpan.FromSeconds(10));

            using (transformed.Subscribe(ku))
            {
                Console.WriteLine("Listening to real-time session '{0}'. Press Enter to termintate", _sessionName);
                Console.ReadLine();
            }
        }

        static void ParseArgs(string[] args)
        {
            foreach (var a in args)
            {
                int index = a.IndexOf(":");
                if (index < 0)
                    ExitWithInvalidArgument(a);

                string name = a.Substring(0, index);
                string value = a.Substring(index + 1);

                switch (name.ToLowerInvariant())
                {
                    case "cluster":
                        _cluster = value;
                        break;

                    case "database":
                        _database = value;
                        break;

                    case "table":
                        _tableName = value;
                        break;

                    case "file":
                        _filePattern = value;
                        break;

                    case "session":
                        _sessionName = value;
                        break;

                    default:
                        ExitWithInvalidArgument(a);
                        break;
                }
            }

            if (_cluster == null)
                ExitWithMissingArgument("cluster");

            if (_database == null)
                ExitWithMissingArgument("database");

            if (_tableName == null)
                ExitWithMissingArgument("table");

            if (_filePattern != null && _sessionName != null)
            {
                Console.WriteLine("Uploading from both file and session is not supprted. Only one of these is supprted");
                Console.WriteLine();
                PrintHelpAndExit();
            }

            _connectionString = String.Format("Data Source=https://{0}.kusto.windows.net:443;Initial Catalog={1};AAD Federated Security=True;",
                _cluster,
                _database);
        }

        static void ExitWithMissingArgument(string argument)
        {
            Console.WriteLine("Missing {0} argument", argument);
            Console.WriteLine();
            PrintHelpAndExit();
        }

        static void ExitWithInvalidArgument(string argument)
        {
            Console.WriteLine("Invalid argument {0}", argument);
            Console.WriteLine();
            PrintHelpAndExit();
        }

        static void PrintHelpAndExit()
        {
            Console.WriteLine(
@"Etw2Kusto is tool for uploading raw ETW events into Kusto. Usage examples:

1) Real-time session

    Etw2Kusto cluster:CDOC database:GeorgiTest table:EtwTcp session:tcp

To use real-time mode:
- the tool must be run with administrative permissions 
- the session has to be created ahead of time with system tools like logman.exe or Perfmon

Example is: 

logman.exe create trace tcp -rt -nb 2 2 -bs 1024 -p {7dd42a49-5329-4832-8dfd-43d979153a88} 0xffffffffffffffff -ets

2) Previously recorded Event Trace Log (.etl files)

    Etw2Kusto cluster:CDOC database:GeorgiTest table:EtwTcp file:*.etl
");

            Environment.Exit(1);
        }

        static void ResetTable(string connectionString, string tableName, Type type)
        {
            using (var admin = KustoClientFactory.CreateCslAdminProvider(connectionString.Replace("ingest-", "")))
            {
                string dropTable = CslCommandGenerator.GenerateTableDropCommand(tableName, true);
                admin.ExecuteControlCommand(dropTable);

                string createTable = CslCommandGenerator.GenerateTableCreateCommand(tableName, type);
                admin.ExecuteControlCommand(createTable);

                string enableIngestTime = CslCommandGenerator.GenerateIngestionTimePolicyAlterCommand(tableName, true);
                admin.ExecuteControlCommand(enableIngestTime);
            }
        }
    }
}