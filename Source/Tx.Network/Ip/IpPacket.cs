namespace Tx.Network
{
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;

    /// <summary>
    /// Data model class that describes the IP packets.
    /// </summary>
    public class IpPacket : IIpPacket
    {
        /// <summary>
        /// Gets the IP packet header.
        /// </summary>
        /// <value>
        /// The IP packet header.
        /// </value>
        public IpPacketHeader PacketHeader { get; set; }

        /// <summary>
        /// Gets the received time.
        /// </summary>
        /// <value>
        /// The received time.
        /// </value>
        public DateTimeOffset ReceivedTime { get; set; }

        /// <summary>
        /// Gets the type of the protocol.
        /// </summary>
        /// <value>
        /// The type of the protocol.
        /// </value>
        public ProtocolType ProtocolType { get; set; }

        /// <summary>
        /// Gets or sets the ip options.
        /// </summary>
        /// <value>
        /// The ip options.
        /// </value>
        public ArraySegment<byte> IpOptions { get; set; }

        /// <summary>
        /// Gets the IP packet data.
        /// </summary>
        /// <value>
        /// The IP packet data.
        /// </value>
        public ArraySegment<byte> PacketData { get; set; }

        /// <summary>
        /// Gets the IP packet data.
        /// </summary>
        /// <value>
        /// The IP packet data.
        /// </value>
        IReadOnlyCollection<byte> IIpPacket.PacketData
        {
            get
            {
                return this.PacketData;
            }
        }
    }
}
