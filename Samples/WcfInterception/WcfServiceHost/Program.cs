using System;
using System.ServiceModel;

namespace WcfInterception
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
