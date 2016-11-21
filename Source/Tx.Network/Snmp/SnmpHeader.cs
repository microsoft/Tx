namespace Tx.Network.Snmp
{
    /// <summary>
    /// Header for Snmp Packet
    /// </summary>
    public struct SnmpHeader
    {
        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public readonly SnmpVersion Version;

        /// <summary>
        /// Gets the community.
        /// </summary>
        /// <value>
        /// The community.
        /// </value>
        public readonly string Community;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnmpHeader" /> struct.
        /// </summary>
        /// <param name="version">The Snmp version.</param>
        /// <param name="community">The Snmp Community.</param>
        public SnmpHeader(SnmpVersion version, string community)
        {
            this.Version = version;
            this.Community = community;
        }
    }
}
