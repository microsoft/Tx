
namespace Tx.Network.Snmp.Asn1Types
{
    /// <summary>
    /// Generic traps applicable to V1 Traps only
    /// </summary>
    public enum GenericTrap
    {
        /// <summary>
        /// The cold start
        /// </summary>
        ColdStart = 0,
        /// <summary>
        /// The warm start
        /// </summary>
        WarmStart = 1,
        /// <summary>
        /// The link down
        /// </summary>
        LinkDown = 2,
        /// <summary>
        /// The link up
        /// </summary>
        LinkUp = 3,
        /// <summary>
        /// The authentication failure
        /// </summary>
        AuthenticationFailure = 4,
        /// <summary>
        /// The egp neighbor loss
        /// </summary>
        EgpNeighborLoss = 5,
        /// <summary>
        /// The enterprise specific
        /// </summary>
        EnterpriseSpecific = 6,
    }
}
