namespace Tx.Network
{
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;

    /// <summary>
    /// Interface for IP packet data model classes.
    /// </summary>
    public interface IIpPacket
    {
        /// <summary>
        /// Gets the IP packet header.
        /// </summary>
        /// <value>
        /// The IP packet header.
        /// </value>
        IpPacketHeader PacketHeader { get; }

        /// <summary>
        /// Gets the received time.
        /// </summary>
        /// <value>
        /// The received time.
        /// </value>
        DateTimeOffset ReceivedTime { get; }

        /// <summary>
        /// Gets the type of the protocol.
        /// </summary>
        /// <value>
        /// The type of the protocol.
        /// </value>
        ProtocolType ProtocolType { get; }

        /// <summary>
        /// Gets the IP packet data.
        /// </summary>
        /// <value>
        /// The IP packet data.
        /// </value>
        IReadOnlyCollection<byte> PacketData { get; }
    }
}
