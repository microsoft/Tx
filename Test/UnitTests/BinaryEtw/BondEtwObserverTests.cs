namespace Tests.Tx.BinaryEtw
{
    using System;

    using global::Tx.Bond;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BondEtwObserverTests
    {
        [TestMethod]
        public void WriteToBinaryEtw()
        {
            var observer = new SimpleWriter(new BinaryEventSourceObserver());
            
            observer.OnNext(new TestBondClass{ EventId = "A" });
            observer.OnNext("A");

            observer.OnCompleted();            
        }
    }
}
