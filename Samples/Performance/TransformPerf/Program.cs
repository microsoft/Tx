using System;
using System.Reactive.Linq;
using System.Reactive.Tx;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Etw;
using Microsoft.Etw.Prototype_Eventing_Provider;
using System.Reactive.Subjects;
using System.Reactive;

namespace TransformPerf
{
    unsafe class Program
    {
        static GCHandle userData;
        static GCHandle recordHandle;
        static EVENT_RECORD record;
        static Type outputType;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(
@"usage: TransformPerf <size> <query>

    size is Small Medium or Large
    query is 1 2 or 3");
                Environment.Exit(0);
            }

            var size = (EventSize)Enum.Parse(typeof(EventSize), args[0]);
            int queryId = int.Parse(args[1]);

            Init(size);

            record = new EVENT_RECORD();
            record.EventHeader.ProviderId = new Guid("3838EF9A-CB6F-4A1C-9033-84C0E8EBF5A7");
            record.EventHeader.EventDescriptor.Id = (ushort)size;
            record.UserData = userData.AddrOfPinnedObject();
            record.UserDataLength = (ushort)Marshal.SizeOf(userData.Target);
            recordHandle = GCHandle.Alloc(record, GCHandleType.Pinned);

            MethodInfo method = typeof(Program).GetMethod("RunForever", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo methodGeneric = method.MakeGenericMethod(outputType);
            methodGeneric.Invoke(null, new object[]{ queryId });
        }

        static void RunForever<TEvent>(int queryId) where TEvent : SystemEvent, new()
        {
            Subject<Timestamped<object>> subject = new Subject<Timestamped<object>>();
            var transform = new EtwManifestDeserializer(subject);
            switch (queryId)
            {
                case 1:
                    {
                        var windows = from w in subject.Window(TimeSpan.FromSeconds(1))
                                      from c in w.Count()
                                      select c;

                        windows.Subscribe(l => Console.WriteLine("Q1 Using EtwManifestTypeMap<T> and Rx for count : {0:n}", l));

                        ProduceEvents(transform);
                        break;
                    }

                case 2:
                    {
                        var query = subject
                            .Scan((long)0, (m, _) => m + 1)
                            .Sample(TimeSpan.FromSeconds(1))
                            .StartWith(0)
                            .Buffer(2, 1);

                        query.Subscribe(b => Console.WriteLine("Q2 total={0:n} diff={1:n}", b[1], b[1] - b[0]));

                        ProduceEvents(transform);
                        break;
                    }

                case 3:
                    {

                        long counter = 0;
                        var query = subject.Subscribe(_ => counter++);

                        var tim = Observable.Interval(TimeSpan.FromSeconds(1))
                            .Subscribe(_ =>
                            {
                                Console.WriteLine("Q3 {0:n}", counter);
                                counter = 0;
                            });

                        ProduceEvents(transform);
                        break;
                    }
            }
        }

        static void Init(EventSize size)
        {
            switch(size)
            {
                case EventSize.Small:
                    userData = GCHandle.Alloc(new SmallEventStruct { ID = 42 }, GCHandleType.Pinned);
                    outputType = typeof(SmallEvent);
                    break;

                case EventSize.Medium:
                    {
                        var str = new MediumEventStruct
                        {
                            ID = 42,
                            number1 = 1,
                            number2 = 2,
                        };

                        for (int i = 0; i < 1023; i++)
                        {
                            str.string1[i] = '1';
                            str.string2[i] = '2';
                        }

                        userData = GCHandle.Alloc(str, GCHandleType.Pinned);
                        outputType = typeof(MediumEvent);
                    }
                    break;

                case EventSize.Large:
                    {
                       var str = new LargeEventStruct
                        {
                            ID = 42,
                            number1 = 1,
                            number2 = 2,
                            number3 = 3,
                            number4 = 4,
                            number5 = 5,
                            number6 = 6,
                            number7 = 7,
                            number8 = 8,
                            number9 = 9,
                            number10 = 10,
                        };

                        for (int i = 0; i < 1023; i++)
                        {
                            str.string1[i] = '1';
                            str.string2[i] = '2';
                            str.string3[i] = '3';
                            str.string4[i] = '4';
                            str.string5[i] = '5';
                            str.string6[i] = '6';
                            str.string7[i] = '7';
                            str.string8[i] = '8';
                            str.string9[i] = '9';
                            str.string10[i] = 'a';
                        }

                       userData = GCHandle.Alloc(str, GCHandleType.Pinned);
                       outputType = typeof(LargeEvent);
                    }
                    break;

                 default:
                    throw new Exception("unknown size");
            }
        }

        static void ProduceEvents(IObserver<EtwNativeEvent> input)
        {
            while (true)
            {
                fixed (EVENT_RECORD* p = &record)
                {
                    EtwNativeEvent evt;
                    evt.record = p;
                    evt._data = (byte*)p;
                    evt._end = ((byte*)p) + record.UserDataLength;
                    evt._length = record.UserDataLength;
                    input.OnNext(evt);
                }
            }
        }
    }
}
