using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using Tx.Windows;
using Tx.Windows.AspNetTrace;
using Tx.Windows.Microsoft_Windows_HttpService;

namespace Tests.Tx
{
    [TestClass]
    public class BingCoreUx
    {
        string EtlFileName
        {
            get
            {
                string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return Path.Combine(dir, @"AspTrace.etl");
            }
        }
        
        [TestMethod]
        public void ConcatenateRequest()
        {
            var pb = new Playback();
            pb.AddEtlFiles(EtlFileName);

            var recvReq = from e in pb.GetObservable<RecvReq>()
                          select new AspRequestInstance
                          {
                              RequestId = new Guid(0, 0, 0, BitConverter.GetBytes(e.RequestId)),
                              RecvReq = e.Header.Timestamp
                          };

            var start = from e in pb.GetObservable<Start>()
                        select new AspRequestInstance
                        {
                            RequestId = e.ContextId,
                            Start = e.Header.Timestamp
                        };


            var startHandler = from e in pb.GetObservable<StartHandler>()
                               select new AspRequestInstance
                               {
                                   RequestId = e.ContextId,
                                   StartHandler = e.Header.Timestamp
                               };

            var httpHandlerEnter = from e in pb.GetObservable<HttpHandlerEnter>()
                                   select new AspRequestInstance
                                   {
                                       RequestId = e.ContextId,
                                       HttpHandlerEnter = e.Header.Timestamp
                                   };

            var httpHandlerLeave = from e in pb.GetObservable<HttpHandlerLeave>()
                                   select new AspRequestInstance
                                   {
                                       RequestId = e.ContextId,
                                       HttpHanlerLeave = e.Header.Timestamp
                                   };

            var endHandler = from e in pb.GetObservable<EndHandler>()
                             select new AspRequestInstance
                             {
                                 RequestId = e.ContextId,
                                 EndHandler = e.Header.Timestamp
                             };

            var end = from e in pb.GetObservable<End>()
                      select new AspRequestInstance
                      {
                          RequestId = e.ContextId,
                          End = e.Header.Timestamp
                      };

            var sameSchema = recvReq
                .Merge(start)
                .Merge(startHandler)
                .Merge(httpHandlerEnter)
                .Merge(httpHandlerLeave)
                .Merge(endHandler)
                .Merge(end);

            var requests = from r in sameSchema
                           group r by r.RequestId into gs
                           from i in gs.Scan((v, i) => v.Merge(i)).Where(v => v.IsCompleted).Take(1)
                           select i;

            int counter = 0;
            requests.Subscribe(r=>
            {
                counter++;
            });

            pb.Run();

            Assert.AreEqual(9530, counter);
 
        }
    }
}
