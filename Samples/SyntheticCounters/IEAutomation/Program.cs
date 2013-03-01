using SHDocVw;
using System;
using System.Threading;

namespace UITest
{
    class Program
    {
        const int navNoReadFromCache = 0x4;
        static AutoResetEvent completed = new AutoResetEvent(false);

        static void OnDocumentComplete(object pDisp, ref object URL)
        {
            completed.Set();
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: IEAutomation url [url...]");
                Console.WriteLine();
                Console.WriteLine("Example: IEAutomation http://bing.com/maps http://bing.com/news");
                Environment.Exit(1);
            }

            InternetExplorer ie = (InternetExplorer)Activator.CreateInstance(Type.GetTypeFromProgID("InternetExplorer.Application"));

            ie.Visible = true;
            ie.DocumentComplete += OnDocumentComplete;

            while (true)
            {
                foreach (string page in args)
                {
                    ie.Navigate(page, navNoReadFromCache);
                    completed.WaitOne();
                }
            }
        }
    }
}
