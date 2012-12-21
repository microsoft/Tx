using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Microsoft.TraceInsight.Samples.WCF
{
    class Program
    {
        static void Main(string[] args)
        {
            bool useInput = true;
            if (args != null && args.Length > 0 && string.Compare(args[0], "test") == 0)
            {
                useInput = false;
            }

            StockQuoteServiceClient client = new StockQuoteServiceClient();

            Console.WriteLine("Press Enter to start sending requests");
            if (useInput)
            {
                Console.ReadLine();
            }

            DailyStockQuote quote;
            Random rand = new Random();
            Stopwatch sw = new Stopwatch();

            while(!Console.KeyAvailable)
            {
                int action = rand.Next() % 4;

                switch (action)
                {
                    case 0:
                        Console.WriteLine("Getting quote for MSFT.");
                        sw.Reset();
                        sw.Start();
                        quote = client.GetQuote("msft");
                        sw.Stop();
                        PrintQuote(quote);
                        break;
                    case 1:
                        Console.WriteLine("Getting quote for AAPL");
                        sw.Reset();
                        sw.Start();
                        quote = client.GetQuote("aapl");
                        sw.Stop();
                        PrintQuote(quote);
                        break;
                    case 2:
                        Console.WriteLine("Buying MSFT...");
                        sw.Reset();
                        sw.Start();
                        client.PurchaseStock("msft", 100);
                        sw.Stop();
                        break;
                    case 3:
                        Console.WriteLine("Buying AAPL...");
                        sw.Reset();
                        sw.Start();
                        client.PurchaseStock("aapl", 15);
                        sw.Stop();
                        break;
                }

                Console.WriteLine("{0}: Done. Duration = {1}", DateTime.Now.ToLongTimeString(), sw.Elapsed);
                Console.WriteLine("-----------------------------------------------------");
            }

            // Always close the client.
            client.Close();

            Console.WriteLine();
            if (useInput)
            {
                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();
            }
            else
            {
                Thread.Sleep(90000);
            }
        }

        static void PrintQuote(DailyStockQuote quote)
        {
            Console.WriteLine("{0} -> [{1}, {2}, {3}, {4}, {5}]", quote.Symbol, quote.Open, quote.High, quote.Low, quote.Close, quote.Volume);
        }
    }
}
