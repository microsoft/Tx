namespace Tests.Tx.BinaryEtw
{
    using System;
    using System.Collections.Generic;

    using global::Tx.Bond;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BondEtwObserverTests
    {
        [TestMethod]
        public void WriteToBinaryEtw()
        {
            using (var observer = new BondEtwObserver())
            {
                observer.OnNext(new GeneralPartitionableTypeMapTests.TestBondClass{ EventId = "A" });
                observer.OnNext("A");
                observer.OnNext(null);

                observer.OnCompleted();
            }
        }

        [TestMethod]
        public void WriteToBinaryEtw_2()
        {
            using (var observer = new BondEtwObserver(
                new Dictionary<Type, string>
                {
                    { typeof(GeneralPartitionableTypeMapTests.TestBondClass), "Manifest" }
                }, 
                TimeSpan.FromMinutes(1)))
            {
                observer.OnNext(new GeneralPartitionableTypeMapTests.TestBondClass { EventId = "A" });
                observer.OnNext("A");
                observer.OnNext(null);

                observer.OnCompleted();
            }
        }
    }
}
