namespace Tx.Network
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Subjects;
    using System.Threading;

    public class UdpReceiver : IObservable<IpPacket>, IDisposable
    {
        #region Public Fields
        public IPEndPoint ListenEndPoint { get; private set; }

        public ProtocolType ListenProtocol { get; private set; }
        public uint ConcurrentReceivers { get; private set; }
        #endregion

        #region Private Fields

        private Socket socket;

        Subject<IpPacket> _packetSubject { get; set; }

        private int runningFlag = 0;

        private int disposeFlag = 0;

        private bool IsDisposed
        {
            get
            {
                return this.disposeFlag == 1;
            }
        }

        private List<IDisposable> disposeables = new List<IDisposable>();
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a Receiver of Observable Packets
        /// </summary>
        /// <param name="ListenAddress">IpAddress to listen on. Accepts any valid local Ipv4 IP or IPAddress.Any</param>
        /// <param name="ListenPort">Local UDP port to listen for. Default is 514</param>
        /// <param name="ConcurrentReceivers">Number of concurrent packet processors to use</param>
        /// <remarks>Concurrent receivers allow for scaling of allocated buffers.
        /// each receiver holds up to 64k bytes and multiple receivers allow for concurrent packet
        /// reception from the underlying socket object.</remarks>
        public UdpReceiver(IPAddress ListenAddress, int ListenPort = 514, uint ConcurrentReceivers = 10)
            : this(new IPEndPoint(ListenAddress, ListenPort), ConcurrentReceivers)
        { }

        /// <summary>
        /// Constructs a Receiver of Observable Packets
        /// </summary>
        /// <param name="ListenAddress">String representation of an IpAddress to listen on. Accepts any valid local Ipv4 IP</param>
        /// <param name="ListenPort">Local UDP port to listen for. Default is 514</param>
        /// <param name="ConcurrentReceivers">Number of concurrent packet processors to use</param>
        /// <remarks>Concurrent receivers allow for scaling of allocated buffers.
        /// each receiver holds up to 64k bytes and multiple receivers allow for concurrent packet
        /// reception from the underlying socket object.</remarks>
        public UdpReceiver(string ListenAddress, int ListenPort = 514, uint ConcurrentReceivers = 10)
            : this(new IPEndPoint(IPAddress.Parse(ListenAddress), ListenPort), ConcurrentReceivers)
        { }

        /// <summary>
        /// Constructs a Receiver of Observable Packets
        /// </summary>
        /// <param name="ListenEndPoint">IPEndPoint constructed with any valid local Ipv4 IP or IPAddress.Any and UDP port number</param>
        /// <param name="ConcurrentReceivers">Number of concurrent packet processors to use. Each one is allocated a buffer of 64Kbytes of memory.</param>
        /// <remarks>Concurrent receivers allow for scaling of allocated buffers.
        /// each receiver holds up to 64k bytes and multiple receivers allow for concurrent packet
        /// reception from the underlying socket object.</remarks>
        public UdpReceiver(IPEndPoint ListenEndPoint, uint ConcurrentReceivers = 10)
        {
            this.ListenProtocol = ProtocolType.Udp;
            this.ConcurrentReceivers = ConcurrentReceivers;
            this.ListenEndPoint = ListenEndPoint;
            this._packetSubject = new Subject<IpPacket>();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Subscribes an observer to the observable packet stream.
        /// </summary>
        /// <param name="observer">Observer accepting type IpPacket</param>
        /// <returns>IDisposable object</returns>
        public IDisposable Subscribe(IObserver<IpPacket> observer)
        {
            var o = _packetSubject.Subscribe(observer);

            if (Interlocked.CompareExchange(ref this.runningFlag, 1, 0) == 0)
            {
                this.Start();
            }

            return o;
        }

        /// <summary>
        /// Gets the amount of data that has been received from the network and is available to be read.
        /// </summary>
        /// <value>
        /// The amount of data that has been received from the network and is available to be read.
        /// </value>
        public int AvailableBytes
        {
            get
            {
                return this.socket.Available;
            }
        }

        #endregion

        #region Private Methods
        private void Start()
        {
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Udp) { ReceiveBufferSize = int.MaxValue };
            this.socket.Bind(this.ListenEndPoint);

            var eventArgsHandler = new EventHandler<SocketAsyncEventArgs>(this.ReceiveCompletedHandler);
            for (uint i = 0; i < this.ConcurrentReceivers; i++)
            {
                var arg = new SocketAsyncEventArgs();
                arg.SetBuffer(new byte[ushort.MaxValue], 0, ushort.MaxValue);
                arg.Completed += eventArgsHandler;

                this.disposeables.Add(arg);
                this.socket.ReceiveAsync(arg);
            }
        }

        private void ReceiveCompletedHandler(object caller, SocketAsyncEventArgs socketArgs)
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (socketArgs.SocketError == SocketError.Success)
            {
                var upacket = new UdpDatagram(socketArgs.Buffer);
                if (upacket.Protocol == ProtocolType.Udp
                    && upacket.DestinationIpAddress.Equals(this.ListenEndPoint.Address)
                    && upacket.DestinationPort == this.ListenEndPoint.Port)
                {
                    upacket.ReceivedTime = DateTimeOffset.UtcNow;
                    this._packetSubject.OnNext(upacket);
                }
            }

            socketArgs.SetBuffer(0, ushort.MaxValue);
            this.socket.ReceiveAsync(socketArgs);
        }
        #endregion

        #region IDisposable Support

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Interlocked.CompareExchange(ref this.disposeFlag, 1, 0) == 0)
            {
                if (this.socket != null)
                {
                    try
                    {
                        this.socket.Shutdown(SocketShutdown.Both);
                        this.socket.Dispose();
                    }
                    catch
                    {
                        // ignored
                    }

                    this.socket = null;
                }

                if (disposeables != null)
                {
                    foreach(var toDispose in disposeables)
                    {
                        toDispose.Dispose();
                    }
                }

                _packetSubject.OnCompleted();
                _packetSubject.Dispose();
            }
        }
       
        #endregion
    }
}
