namespace Tx.Network
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Linq;
    using System.Reactive.Disposables;
    using System.Threading.Tasks;

    public abstract class BaseUdpReceiver<T> : IObservable<T>
    {
        #region Public Fields
        public IPEndPoint ListenEndPoint { get; private set; }
        #endregion

        #region Private Fields

        private Func<int> availableSocketBytesGetter;

        private readonly IObservable<T> observable;

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
            this.ListenEndPoint = listenEndPoint;

            this.observable = Observable.Create<T>(observer =>
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Udp) { ReceiveBufferSize = int.MaxValue };
                socket.Bind(listenEndPoint);

                this.availableSocketBytesGetter = () => socket.Available;

                var eventArgsHandler = new EventHandler<SocketAsyncEventArgs>(this.ReceiveCompletedHandler);
                var disposables = new List<IDisposable> {
                        Disposable.Create(() => this.availableSocketBytesGetter = () => 0),
                        Disposable.Create(() => socket.Shutdown(SocketShutdown.Both)),
                        socket };
                for (var i = 0; i < concurrentReceivers; i++)
                {
                    var eventArgs = new SocketAsyncEventArgs();
                    eventArgs.SetBuffer(new byte[ushort.MaxValue], 0, ushort.MaxValue);
                    eventArgs.Completed += eventArgsHandler;
                    eventArgs.UserToken = observer;
                    disposables.Add(eventArgs);

                    socket.ReceiveAsync(eventArgs);
                }

                return new CompositeDisposable(disposables);
            }).Publish().RefCount();
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
            return this.observable.Subscribe(observer);
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
                return this.availableSocketBytesGetter();
            }
        }

        #endregion

        #region Private Methods

        private void ReceiveCompletedHandler(object caller, SocketAsyncEventArgs socketArgs)
        {
            var socket = caller as Socket;
            if (socket == null || socketArgs.SocketError != SocketError.Success)
            {
                return;
            }
            
            do
            {
                var ipPacket = PacketParser.Parse(DateTimeOffset.UtcNow, false, socketArgs.Buffer, 0, socketArgs.Buffer.Length);

                T packet;
                if (this.TryParse(ipPacket, out packet))
                {
                    ((IObserver<T>)socketArgs.UserToken).OnNext(packet);
                }

                socketArgs.SetBuffer(0, ushort.MaxValue);
            } while (!socket.ReceiveAsync(socketArgs));
        }

        protected abstract bool TryParse(IpPacket packet, out T envelope);

        #endregion
    }
}