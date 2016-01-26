
namespace Tx.Network.Snmp
{
    using System;

    /// <summary>
    /// SNMP specific Tags. These are matched with a Class = Application.
    /// </summary>
    public enum Asn1SnmpTag
    {
        /// <summary>
        /// 4 byte IPv4 address.
        /// </summary>
        IpAddress = 0,

        /// <summary>
        /// Unsigned integer.
        /// </summary>
        Counter32 = 1,

        /// <summary>
        /// Same as Counter32.
        /// </summary>
        Counter = 1,

        /// <summary>
        /// Unsigned integer.
        /// </summary>
        Gauge32 = 2,

        /// <summary>
        /// Same as Gauge32.
        /// </summary>
        Gauge = 2,

        /// <summary>
        /// Hold a unsigned integer holding a count of 1/100th seconds since the epoch. Where
        /// epoch is context sensitive.
        /// </summary>
        TimeTicks = 3,

        /// <summary>
        /// Not currently supported. But it is supposed to hold other Asn1 BER encodings.
        /// </summary>
        Opaque = 4,

        /// <summary>
        /// Legacy don't use.
        /// </summary>
        NsapAddress = 5,

        /// <summary>
        /// Unsigned long. 
        /// </summary>
        Counter64 = 6,

        /// <summary>
        /// General Unsigned integer. 
        /// </summary>
        UInt32 = 7,

        /// <summary>
        /// A sequace of other values. Also used to encode PDUs and VarBindLists.
        /// </summary>
        Sequence = 16,

        /// <summary>
        /// The not SNMP data
        /// This is custom Tag added just to identify if data is Snmp or Asn1
        /// </summary>
        NotSnmpData = 99,

        /// <summary>
        /// Special value for when this isn't an snmp specific tag.
        /// </summary>
        NotSnmpSpecific = Int32.MaxValue
    }
}
