namespace Tx.Network.UnitTests
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Reactive.Linq;

    public class ReceiveTxSyslog : IDisposable
    {
        public int Counter;
        public UdpReceiver Receiver { get; set; }
        public ReceiveTxSyslog(IPEndPoint listener, uint receivercount)
        {
            Receiver = new UdpReceiver(listener, receivercount);
        }
        public IDisposable RunCount()
        {
            return Receiver.Select(i => new Syslog(i,null)).Subscribe(s => { if (s != null) { Counter++; } });
        }
        public IObservable<Syslog> ObserveTx()
        {
            return Receiver.Select(i => new Syslog(i,null));
        }

        public void Dispose()
        {
            Receiver.Dispose();
        }
    }
}
