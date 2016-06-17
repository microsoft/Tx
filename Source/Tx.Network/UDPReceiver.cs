﻿namespace Tx.Network
{
    using System;
    using System.Collections.Concurrent;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Subjects;


    public class UdpReceiver : IObservable<IpPacket>, IDisposable
    {
        #region Public Fields
        public IPEndPoint ListenEndPoint { get; private set; }
        public Socket Socket { get; private set; }
        public ProtocolType ListenProtocol { get; private set; }
        public uint ConcurrentReceivers { get; private set; }
        #endregion

        #region Private Fields
        Subject<IpPacket> _packetSubject { get; set; }
        ConcurrentQueue<SocketAsyncEventArgs> _receivedDataProcessorsPool { get; set; }
        bool _subscribed { get; set; }

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
            ListenProtocol = ProtocolType.Udp;
            this.ConcurrentReceivers = ConcurrentReceivers;
            this.ListenEndPoint = ListenEndPoint;
            _packetSubject = new Subject<IpPacket>();
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
            if (!_subscribed)
            {
                _subscribed = true;
                Start();
            }
            return o;
        }
        #endregion

        #region Private Methods
        private void Start()
        {
            _receivedDataProcessorsPool = new ConcurrentQueue<SocketAsyncEventArgs>();
            var eventArgsHandler = new EventHandler<SocketAsyncEventArgs>(ReceiveCompletedHandler);

            // pre-allocate the SocketAsyncEventArgs in a receiver queue to constrain memory usage for buffers
            for (var i = 0; i < ConcurrentReceivers; i++)
            {
                var eventArgs = new SocketAsyncEventArgs();
                eventArgs.SetBuffer(new byte[ushort.MaxValue], 0, ushort.MaxValue);
                eventArgs.Completed += eventArgsHandler;
                _receivedDataProcessorsPool.Enqueue(eventArgs);
            }
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Udp) { ReceiveBufferSize = int.MaxValue };
            Socket.Bind(ListenEndPoint);
            GetDataProcessorAndReceive();
        }

        private void ReceiveCompletedHandler(object caller, SocketAsyncEventArgs socketArgs)
        {
            if (!disposeCalled)
            {
                GetDataProcessorAndReceive(); //call a new processor
                                              //var packet = new IP(socketArgs.Buffer);
                var packet = new IpPacket();
                var packetCheck = IsDestinationListenEndpoint(socketArgs.Buffer, out packet);

                if (socketArgs.LastOperation == SocketAsyncOperation.Receive
                    && socketArgs.SocketError == SocketError.Success
                    && packetCheck
                    )
                {
                    _packetSubject.OnNext(packet);
                }
                socketArgs.SetBuffer(0, ushort.MaxValue);
                _receivedDataProcessorsPool.Enqueue(socketArgs);
                GetDataProcessorAndReceive(); //failed to get a processor at the beginning, try now since an enqueue was performed.
            }
        }

        private void GetDataProcessorAndReceive()
        {
            SocketAsyncEventArgs deqAsyncEvent = null;
            if (_receivedDataProcessorsPool.TryDequeue(out deqAsyncEvent))
            {
                if (deqAsyncEvent.Offset != 0 || deqAsyncEvent.Count != ushort.MaxValue)
                {
                    deqAsyncEvent.SetBuffer(0, ushort.MaxValue);
                }
                var sockCheck = Socket.ReceiveAsync(deqAsyncEvent);
            }

        }

        private bool IsDestinationListenEndpoint(byte[] Buffer, out IpPacket packet)
        {
            var upacket = new UdpDatagram(Buffer);
            packet = upacket;
            if (packet.Protocol != ProtocolType.Udp) return false;

            return upacket.DestinationIpAddress.Equals(ListenEndPoint.Address) && upacket.DestinationPort == ListenEndPoint.Port;
        }
        #endregion

        #region IDisposable Support
        bool disposeCalled;
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                disposeCalled = true;
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Dispose();
                
                while(_receivedDataProcessorsPool.Count > 0)
                {
                    var dequeued = new SocketAsyncEventArgs();
                    _receivedDataProcessorsPool.TryDequeue(out dequeued);
                    dequeued.Dispose();
                }

                _packetSubject.OnCompleted();
                _packetSubject.Dispose();
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
