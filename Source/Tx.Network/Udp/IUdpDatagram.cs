namespace Tx.Network
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface for UDP datagram data model classes.
    /// </summary>
    public interface IUdpDatagram
    {
        /// <summary>
        /// Gets the IP packet header.
        /// </summary>
        /// <value>
        /// The IP packet header.
        /// </value>
        IpPacketHeader PacketHeader { get; }

        /// <summary>
        /// Gets the UDP datagram header.
        /// </summary>
        /// <value>
        /// The UDP datagram header.
        /// </value>
        UdpDatagramHeader UdpDatagramHeader { get; }

        /// <summary>
        /// Gets the received time.
        /// </summary>
        /// <value>
        /// The received time.
        /// </value>
        DateTimeOffset ReceivedTime { get; }

        /// <summary>
        /// Gets the UDP data.
        /// </summary>
        /// <value>
        /// The UDP data.
        /// </value>
        IReadOnlyCollection<byte> Data { get; }
    }
}