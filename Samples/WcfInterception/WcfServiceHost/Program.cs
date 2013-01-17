// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.ServiceModel;

namespace Tx.Samples.WCFInterception
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ServiceHost host = new ServiceHost(typeof(Service), new Uri("net.tcp://localhost:900/StockQuoteService")))
            {
                host.Open();

                Console.WriteLine("Done, press ENTER to exit");
                Console.ReadLine();
            }
        }
    }
}
