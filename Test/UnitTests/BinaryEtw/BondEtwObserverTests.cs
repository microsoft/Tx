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
            using (var observer = new BinaryEtwObserverOld())
            {
                observer.OnNext(new TestBondClass{ EventId = "A" });
                observer.OnNext("A");
                observer.OnNext(null);

                observer.OnCompleted();
            }
        }

        [TestMethod]
        public void WriteToBinaryEtw_2()
        {
            using (var observer = new BinaryEtwObserverOld(
                "UnitTests",
                new Type[] 
                {
                    typeof(TestBondClass),
                }, 
                TimeSpan.FromMinutes(1)))
            {
                observer.OnNext(new TestBondClass { EventId = "A" });
                observer.OnNext("A");
                observer.OnNext(null);

                observer.OnCompleted();
            }
        }
    }
}
