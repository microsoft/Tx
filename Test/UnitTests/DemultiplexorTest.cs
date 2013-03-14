using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reactive;

namespace Tests.Tx
{
    [TestClass]
    public class DemultiplexorTest
    {
        [TestMethod]
        public void DemuxTwo()
        {
            Demultiplexor demux = new Demultiplexor();

            var intObserver = new CountObserver<int>();
            demux.GetObservable<int>().Subscribe(intObserver);

            var stringObserver = new CountObserver<string>();
            demux.GetObservable<string>().Subscribe(stringObserver);

            demux.OnNext(1);
            demux.OnNext(2);
            demux.OnNext(3);

            demux.OnNext("foo");
            demux.OnNext("bar");

            Assert.AreEqual(3, intObserver.Count);
            Assert.AreEqual(2, stringObserver.Count);
        }

        class CountObserver<T> : IObserver<T>
        {
            int _count;

            public int Count
            {
                get { return _count; }
            }

            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
                throw error;
            }

            public void OnNext(T value)
            {
                _count++;
            }
        }

    }
}
