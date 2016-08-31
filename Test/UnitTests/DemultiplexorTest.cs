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

        [TestMethod]
        public void GetObservableInheritenceTest1()
        {
            var itemA = new TestClassA();
            var itemB = new TestClassB();
            var itemC = new TestClassC();
            var interfaceObserver = new CountObserver<ITestClassA>();
            var itemAObserver = new CountObserver<TestClassA>();
            var itemBObserver = new CountObserver<TestClassB>();
            var itemCObserver = new CountObserver<TestClassC>();
            
            var demux = new Demultiplexor();

            demux.GetObservable<ITestClassA>().Subscribe(interfaceObserver);
            demux.GetObservable<TestClassA>().Subscribe(itemAObserver);
            demux.GetObservable<TestClassB>().Subscribe(itemBObserver);
            demux.GetObservable<TestClassC>().Subscribe(itemCObserver);

            demux.OnNext(itemA);
            demux.OnNext(itemB);
            demux.OnNext(itemC);

            Assert.AreEqual(3, interfaceObserver.Count);
            Assert.AreEqual(3, itemAObserver.Count);
            Assert.AreEqual(2, itemBObserver.Count);
            Assert.AreEqual(1, itemCObserver.Count);
        }

        [TestMethod]
        public void TestLateGetObservableRefreshesCache()
        {
            var itemA = new TestClassA();
            var itemB = new TestClassB();
            var itemC = new TestClassC();
            var interfaceObserver = new CountObserver<ITestClassA>();
            var itemAObserver = new CountObserver<TestClassA>();
            var itemBObserver = new CountObserver<TestClassB>();
            var itemCObserver = new CountObserver<TestClassC>();

            var demux = new Demultiplexor();

            demux.GetObservable<TestClassA>().Subscribe(itemAObserver);
            demux.GetObservable<TestClassB>().Subscribe(itemBObserver);
            demux.GetObservable<TestClassC>().Subscribe(itemCObserver);

            demux.OnNext(itemA);
            demux.GetObservable<ITestClassA>().Subscribe(interfaceObserver);
            demux.OnNext(itemB);
            demux.OnNext(itemC);

            Assert.AreEqual(2, interfaceObserver.Count);
            Assert.AreEqual(3, itemAObserver.Count);
            Assert.AreEqual(2, itemBObserver.Count);
            Assert.AreEqual(1, itemCObserver.Count);
        }

        interface ITestClassA
        {
            int ValueA { get; set; }   
        }

        class TestClassA : ITestClassA
        {
            public int ValueA { get; set; }
        }

        class TestClassB : TestClassA
        {
            public string ValueB { get; set; }
        }

        class TestClassC : TestClassB
        {
            public double ValueC { get; set; }
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
