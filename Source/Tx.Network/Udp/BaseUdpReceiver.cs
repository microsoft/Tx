namespace Tx.Network
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Subjects;

    public abstract class BaseUdpReceiver<T> : IObservable<T>, IDisposable
    {
        #region Public Fields
        public IPEndPoint ListenEndPoint { get; private set; }

        public ProtocolType ListenProtocol { get; private set; }
        public uint ConcurrentReceivers { get; private set; }
        #endregion

        #region Private Fields

        private Socket socket;

        private readonly Subject<T> packetSubject;

        private ConcurrentQueue<SocketAsyncEventArgs> receivedDataProcessorsPool;

        private bool subscribed;

        private readonly List<IDisposable> disposeables = new List<IDisposable>();

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a Receiver of Observable Packets
        /// </summary>
        /// <param name="listenEndPoint">IPEndPoint constructed with any valid local Ipv4 IP or IPAddress.Any and UDP port number</param>
        /// <param name="concurrentReceivers">Number of concurrent packet processors to use. Each one is allocated a buffer of 64Kbytes of memory.</param>
        /// <remarks>Concurrent receivers allow for scaling of allocated buffers.
        /// each receiver holds up to 64k bytes and multiple receivers allow for concurrent packet
        /// reception from the underlying socket object.</remarks>
        protected BaseUdpReceiver(IPEndPoint listenEndPoint, uint concurrentReceivers)
        {
            this.ListenProtocol = ProtocolType.Udp;
            this.ConcurrentReceivers = concurrentReceivers;
            this.ListenEndPoint = listenEndPoint;
            this.packetSubject = new Subject<T>();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Subscribes an observer to the observable packet stream.
        /// </summary>
        /// <param name="observer">Observer accepting type IpPacket</param>
        /// <returns>IDisposable object</returns>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            var o = this.packetSubject.Subscribe(observer);
            if (!this.subscribed)
            {
                this.subscribed = true;
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
            this.receivedDataProcessorsPool = new ConcurrentQueue<SocketAsyncEventArgs>();
            var eventArgsHandler = new EventHandler<SocketAsyncEventArgs>(this.ReceiveCompletedHandler);

            // pre-allocate the SocketAsyncEventArgs in a receiver queue to constrain memory usage for buffers
            for (var i = 0; i < this.ConcurrentReceivers; i++)
            {
                var eventArgs = new SocketAsyncEventArgs();
                eventArgs.SetBuffer(new byte[ushort.MaxValue], 0, ushort.MaxValue);
                eventArgs.Completed += eventArgsHandler;
                this.receivedDataProcessorsPool.Enqueue(eventArgs);
                this.disposeables.Add(eventArgs);
            }
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Udp) { ReceiveBufferSize = int.MaxValue };
            this.socket.Bind(this.ListenEndPoint);
            this.GetDataProcessorAndReceive();
        }

        private void ReceiveCompletedHandler(object caller, SocketAsyncEventArgs socketArgs)
        {
            if (!this.disposeCalled)
            {
                this.GetDataProcessorAndReceive(); //call a new processor
                
                T packet;
                var ipPacket = PacketParser.Parse(DateTimeOffset.UtcNow, false, socketArgs.Buffer, 0, socketArgs.Buffer.Length);

                var packetCheck = this.TryParse(ipPacket, out packet);

                if (socketArgs.LastOperation == SocketAsyncOperation.Receive
                    && socketArgs.SocketError == SocketError.Success
                    && packetCheck
                    )
                {
                    this.packetSubject.OnNext(packet);
                }
                socketArgs.SetBuffer(0, ushort.MaxValue);
                this.receivedDataProcessorsPool.Enqueue(socketArgs);
                this.GetDataProcessorAndReceive(); //failed to get a processor at the beginning, try now since an enqueue was performed.
            }
        }

        private void GetDataProcessorAndReceive()
        {
            SocketAsyncEventArgs deqAsyncEvent;
            if (this.receivedDataProcessorsPool.TryDequeue(out deqAsyncEvent))
            {
                if (deqAsyncEvent.Offset != 0 || deqAsyncEvent.Count != ushort.MaxValue)
                {
                    deqAsyncEvent.SetBuffer(0, ushort.MaxValue);
                }
                var sockCheck = this.socket.ReceiveAsync(deqAsyncEvent);
            }
        }

        protected abstract bool TryParse(IpPacket packet, out T envelope);

        #endregion

        #region IDisposable Support

        private bool disposeCalled;

        public void Dispose()
        {
            if (!this.disposeCalled)
            {

                this.disposeCalled = true;

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

                if (this.disposeables != null)
                {
                    foreach (var toDispose in this.disposeables)
                    {
                        toDispose.Dispose();
                    }

                }

                this.packetSubject.OnCompleted();
                this.packetSubject.Dispose();
            }

        }

        #endregion
    }
}