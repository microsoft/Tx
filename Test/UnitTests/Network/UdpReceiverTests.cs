namespace Tests.Tx.Network
{
    using global::Tx.Network;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UdpReceiverTests
    {
        [TestMethod]
        public void Construct()
        {
            using (new UdpReceiver("127.128.1.1"))
            {
            }
        }
    }
}
