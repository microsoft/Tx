using Microsoft.SqlServer.XEvent;
using Microsoft.SqlServer.XEvent.Configuration;
using System.Collections.Generic;
using System;
using System.Threading;
using Tx.SqlServer;
using System.Reactive;
using System.Dynamic;
using System.Reactive.Subjects;
using Microsoft.SqlServer.XEvent.Linq;

[assembly: XEventPackage("mytestpackage",
         "{9FD3BD5A-2C46-4682-9DA8-CF3400E634E6}",
         "This is a test package that demonstrates usage of managed XEvent")]

namespace TxSamples.XEventGenerator
{
    enum enumCol
    {
        a = 0,
        b = 1,
    }

    [XEvent("myevent", "{5F580BCE-3F37-49A9-A34D-E52B778D05A0}", Channel.Admin, "resourceKey")]
    class MyXEvent : BaseXEvent<MyXEvent>
    {
        public MyXEvent() // required constructor for the deserialization to work
        {}

        public MyXEvent(int fldVal, enumCol v)
        {
            myfieldval = fldVal;
            x = v;
        }

        [NonPublished]
        public int myfieldval;

        public enumCol x;
    }

    [XETarget("mytarget", "")]
    class MyTarget : XeSubject
    {
        static Subject<PublishedEvent> _subject = new Subject<PublishedEvent>();

        protected override Subject<PublishedEvent> Instance
        {
            get { return _subject; }
        }

        public override string GetSerializedState()
        {
            return "state";
        }

        public override void Initialize(List<KeyValuePair<string, object>> targetParameters, bool verificationOnly)
        {
            ; // place to set breakpoint
        }
    }
    
    class Program
    {
        static void Main()
        {
            Playback playback = new Playback();
            playback.AddXeTarget<MyTarget>();
            playback.GetObservable<MyXEvent>().Subscribe(e => Console.WriteLine(e.x));
            playback.Start();

            SessionConfiguration cfg = new SessionConfiguration(@"xeconfig.xml");

            for (int i = 0; i < 10; i++)
            {
                if (MyXEvent.IsEnabled)
                {
                    MyXEvent evt = new MyXEvent(3, enumCol.b);
                    evt.Publish();
                }
            }

            Console.ReadLine();
        }
    }
}

