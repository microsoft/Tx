namespace Tx.Network.Snmp
{
    using Asn1Types;
    using System.Collections.ObjectModel;
    using System.Net;
    /// <summary>
    /// Class to represent PDU for Snmp V1
    /// </summary>
    public struct SnmpV1PDU
    {
        /// <summary>
        /// Gets the type of the pdu.
        /// </summary>
        /// <value>
        /// The type of the pdu.
        /// </value>
        public readonly PduType PduType;

        /// <summary>
        /// The enterprise
        /// </summary>
        public readonly ObjectIdentifier Enterprise;

        /// <summary>
        /// The agent address
        /// </summary>
        public readonly IPAddress AgentAddress;

        /// <summary>
        /// The generic v1 trap
        /// </summary>
        public readonly GenericTrap GenericV1Trap;

        /// <summary>
        /// The specific trap
        /// </summary>
        public readonly int SpecificTrap;

        /// <summary>
        /// The time stamp
        /// </summary>
        public readonly uint TimeStamp;

        /// <summary>
        /// Gets the variable binds.
        /// </summary>
        /// <value>
        /// The variable binds.
        /// </value>
        public readonly ReadOnlyCollection<VarBind> VarBinds;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnmpV1PDU"/> struct.
        /// </summary>
        /// <param name="pduType">Type of the pdu.</param>
        /// <param name="varBinds">The variable binds.</param>
        /// <param name="enterprise">The enterprise.</param>
        /// <param name="agentAddress">The agent address.</param>
        /// <param name="genericV1Trap">The generic v1 trap.</param>
        /// <param name="specificTrap">The specific trap.</param>
        /// <param name="timeStamp">The time stamp.</param>
        internal SnmpV1PDU(PduType pduType, VarBind[] varBinds, ObjectIdentifier enterprise, IPAddress agentAddress, GenericTrap genericV1Trap, int specificTrap, uint timeStamp)
        {
            PduType = pduType;
            VarBinds = new ReadOnlyCollection<VarBind>(varBinds);
            Enterprise = enterprise;
            AgentAddress = agentAddress;
            GenericV1Trap = genericV1Trap;
            SpecificTrap = specificTrap;
            TimeStamp = timeStamp;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnmpV1PDU"/> struct.
        /// </summary>
        /// <param name="pduType">Type of the pdu.</param>
        /// <param name="enterprise">The enterprise.</param>
        /// <param name="agentAddress">The agent address.</param>
        /// <param name="genericV1Trap">The generic v1 trap.</param>
        /// <param name="specificTrap">The specific trap.</param>
        /// <param name="timeStamp">The time stamp.</param>
        /// <param name="varBinds">The variable binds.</param>
        public SnmpV1PDU(PduType pduType, ObjectIdentifier enterprise, IPAddress agentAddress, GenericTrap genericV1Trap, int specificTrap, uint timeStamp, VarBind[] varBinds)
        {
            PduType = pduType;
            VarBinds = new ReadOnlyCollection<VarBind>((VarBind[])varBinds.Clone());
            Enterprise = enterprise;
            AgentAddress = agentAddress;
            GenericV1Trap = genericV1Trap;
            SpecificTrap = specificTrap;
            TimeStamp = timeStamp;
        }
    }
}
