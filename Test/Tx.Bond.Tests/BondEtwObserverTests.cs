using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tx.Bond.Tests
{
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
