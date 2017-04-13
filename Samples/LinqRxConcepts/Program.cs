using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Dynamic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Xml.Linq;

namespace LinqAndRxConcepts
{
    class Program
    {
        static void Main(string[] args)
        {
            ExensionMethods();
            FunctionsAndAnonymousMethods();
            LinqToObjects();
            LinqOperators();
            RxOperators();
            PushInsidePull();
        }

        static void ExensionMethods()
        {
            var r = new Random();

            byte[] buffer = new byte[42];
            r.NextBytes(buffer);

            // ToHexDump is example of extension method 
            var hex = buffer.ToHexDump();
        }

        static void FunctionsAndAnonymousMethods()
        {
            // Functions only use the argument (not context data) and don't have side effects
            Func<int, bool> isEven = i => (i % 2) == 0;
            Console.WriteLine(isEven(5));

            // Anonymous methods can use variables in the context and leave side effects
            int previous = 0;
            Func<int, bool> isIncrementing = i =>
            {
                bool result = i > previous;
                previous = i;
                return result;
            };

            Console.WriteLine(isIncrementing(1));
            Console.WriteLine(isIncrementing(2));
            Console.WriteLine(isIncrementing(2));
            Console.WriteLine(isIncrementing(3));
        }


        static void LinqToObjects()
        {
            // building a pipeline by using extension methods
            IEnumerable<string> pipeline = EvtxEnumerable.ReadLog("Security")
                .Take(1000)     
                .Where(e => e.Id == 4688)
                .Select(e => e.ToXml())
                .ToArray();

            // the same query, using comprehension syntax
            IEnumerable<string> query = (
                            from e in EvtxEnumerable.ReadLog("Security").Take(1000)
                            where e.Id == 4688
                            select e.ToXml()
                        ).ToArray();

            // Stop on a breakpoint in the following line and inspect 
            // the variables "pipeline" and "query"
        }
        static void LinqOperators()
        {
            IEnumerable<EventRecord> all = EvtxEnumerable.ReadLog("Security");
            var processStart = all.Filter(e => e.Id == 4688).Take(10);

            foreach(var ps in processStart)
            {
                Console.WriteLine(ps.Properties[5].Value);
            }
        }

        static void RxOperators()
        {
            // This sample illustrates how Push LINQ works, 
            // using IObservable<T> as interface to compose pipelines

            IObservable<EventRecord> all = EvtxEnumerable.ReadLog("Security").ToObservable();

            var result = all
                .Where(e => e.Id == 4688)
                .Select(e => e.ToXml())
                .Select(xml => Xml2Dynamic(xml))
                .Select(d => Dynamic2Csv(d));

            result.Subscribe(csv => Console.WriteLine(csv));
        }

        static void PushInsidePull()
        {
            // This sample shows running Rx pipeline inside pull environment
            // It is a stepping stone to build Cosmos Extractor that can host Rx rules
            IEnumerable<string> all = EvtxEnumerable.ReadLog("Security")
                .Take(1000)
                .Select(e => e.ToXml())
                .ToArray();

            // mouse-hover on the following .Where to see that it is 
            // push (real-time) implementation
            var result = all.ReplayRealTimeRule(
                o => o.Where(e => e.Contains("4688")) 
                );

            foreach (var xml in result)
                Console.WriteLine(xml);
        }

        static ExpandoObject Xml2Dynamic(string xml)
        {
            XElement xe = XElement.Parse(xml);
            XElement eventData = xe.Element(ElementNames.EventData);

            dynamic d = new ExpandoObject();
            foreach (XElement data in eventData.Elements(ElementNames.Data))
            {
                string name = data.Attribute("Name").Value;
                string value = data.Value;
                ((IDictionary<string, object>)d).Add(name, value);
            }
            return d;
        }

        static string Dynamic2Csv(ExpandoObject d)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (var pair in (IDictionary<string, object>)d)
            {
                if (!first)
                    sb.Append(", ");

                sb.Append(pair.Key);
                sb.Append(" = ");
                sb.Append(pair.Value);
                first = false;
            }
            var s = sb.ToString();
            return s;
        }
    }
    class ElementNames
    {
        private static readonly XNamespace ns = "http://schemas.microsoft.com/win/2004/08/events/event";
        public static readonly XName EventData = ns + "EventData";
        public static readonly XName Data = ns + "Data";
    }
}
