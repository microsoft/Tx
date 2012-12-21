using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive;
using Tx.Windows;
using System.Reflection;

namespace TxSamples.TypeStatistics
{
    class Program
    {
        // To run this, first create the session from admin command prompt:
        // logman.exe create trace tcp -rt -nb 2 2 -bs 1024 -p {7dd42a49-5329-4832-8dfd-43d979153a88} 0xffffffffffffffff -ets
        //
        // Make sure VS is started as administrator

        static void Main()
        {
            var stat = new TypeOccurenceStatistics(Assembly.GetExecutingAssembly().GetTypes());
            stat.AddEtlFiles(@"HTTP_Server.etl");
            stat.Run();

            foreach (KeyValuePair<Type, long> pair in stat.Statistics)
            {
                Console.WriteLine("{0,-15} {1}", pair.Key.Name, pair.Value);
            }
        }
    }
}
