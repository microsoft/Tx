namespace Tx.Network.Snmp
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    using Tx.Network.Snmp.Asn1Types;

    public class SnmpDatagram
    {
        public SnmpDatagram(DateTimeOffset receivedTime, string sourceIpAddress, SnmpHeader header, IReadOnlyList<VarBind> varBinds)
        {
            this.ReceivedTime = receivedTime;
            this.SourceIpAddress = sourceIpAddress;
            this.Header = header;
            this.VarBinds = varBinds;
        }

        public DateTimeOffset ReceivedTime { get; internal set; }

        public string SourceIpAddress { get; internal set; }

        public SnmpHeader Header { get; internal set; }

        public IReadOnlyList<VarBind> VarBinds { get; internal set; }
    }

    public class SnmpDatagramV1 : SnmpDatagram
    {
        public SnmpDatagramV1(
            DateTimeOffset receivedTime, 
            string sourceIpAddress, 
            SnmpHeader header, 
            IReadOnlyList<VarBind> varBinds)
            : base(receivedTime, sourceIpAddress, header, varBinds)
        {
        }

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
    }

    public class SnmpDatagramV2C : SnmpDatagram
    {
        public SnmpDatagramV2C(
            DateTimeOffset receivedTime, 
            string sourceIpAddress, 
            SnmpHeader header, 
            IReadOnlyList<VarBind> varBinds,
            PduType pduType,
            int requestId,
            SnmpErrorStatus errorStatus,
            int errorIndex)
            : base(receivedTime, sourceIpAddress, header, varBinds)
        {
            this.PduType = pduType;
            this.RequestId = requestId;
            this.ErrorStatus = errorStatus;
            this.ErrorIndex = errorIndex;
        }

        /// <summary>
        /// Gets the type of the pdu.
        /// </summary>
        /// <value>
        /// The type of the pdu.
        /// </value>
        public readonly PduType PduType;

        /// <summary>
        /// Gets the request identifier.
        /// </summary>
        /// <value>
        /// The request identifier.
        /// </value>
        public readonly int RequestId;

        /// <summary>
        /// Gets the error status.
        /// </summary>
        /// <value>
        /// The error status.
        /// </value>
        public readonly SnmpErrorStatus ErrorStatus;

        /// <summary>
        /// Gets the index of the error.
        /// </summary>
        /// <value>
        /// The index of the error.
        /// </value>
        public readonly int ErrorIndex;
    }
}