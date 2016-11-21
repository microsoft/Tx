namespace Tx.Network
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class to represent UDP datagram which contains the UDP headers and Data.
    /// </summary>
    public class UdpDatagram : IUdpDatagram
    {
        /// <summary>
        /// Gets the IP packet header.
        /// </summary>
        /// <value>
        /// The IP packet header.
        /// </value>
        public IpPacketHeader PacketHeader { get; set; }

        /// <summary>
        /// Gets the UDP datagram header.
        /// </summary>
        /// <value>
        /// The UDP datagram header.
        /// </value>
        public UdpDatagramHeader UdpDatagramHeader { get; set; }


        /// <summary>
        /// Gets the received time.
        /// </summary>
        /// <value>
        /// The received time.
        /// </value>
        public DateTimeOffset ReceivedTime { get; set; }

        /// <summary>
        /// Gets or sets the UDP data.
        /// </summary>
        /// <value>
        /// The UDP data.
        /// </value>
        public ArraySegment<byte> UdpData { get; set; }

        /// <summary>
        /// Gets the UDP data.
        /// </summary>
        /// <value>
        /// The UDP data.
        /// </value>
        IReadOnlyCollection<byte> IUdpDatagram.Data
        {
            get
            {
                return this.UdpData;
            }
        }
    }
}


