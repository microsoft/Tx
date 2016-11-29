namespace Tests.Tx.BinaryEtw
{
    using global::Tx.Bond;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BondEtwObserverTests
    {
        [TestMethod]
        public void WriteToBinaryEtw()
        {
            var observer = new BondJsonConverter(new BinaryEventSourceObserver());
            
            observer.OnNext(new TestBondClass{ EventId = "A" });
            observer.OnNext("A");

            observer.OnCompleted();            
        }
    }
}
