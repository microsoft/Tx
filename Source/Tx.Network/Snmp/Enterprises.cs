namespace Tx.Network
{
    using System.Collections.Generic;

    using Tx.Network.Snmp;

    /// <summary>
    /// class to provide extension methods to calculate Enterprises
    /// </summary>
    public static class Enterprises
    {
        /// <summary>
        /// Enterprise names as per
        /// http://www.iana.org/assignments/enterprise-numbers/enterprise-numbers
        /// </summary>
        private static readonly IDictionary<uint, string> Names =
            new Dictionary<uint, string>
                {
                    {9, "Cisco"},
                    {21296, "Infinera"},
                    {3780, "Level3"},
                    {6027, "Force10"},
                    {30065, "Arista"},
                    {2636, "Juniper"},
                    {8072, "net-snmp"},
                };

        /// <summary>
        /// The prefix
        /// </summary>
        private static readonly ObjectIdentifier prefixOid = new ObjectIdentifier("1.3.6.1.4.1");

        /// <summary>
        /// Method to return Enterprise name.
        /// </summary>
        /// <param name="oid">The ObjectIdentifier as string.</param>
        /// <returns>Enterprise name</returns>
        public static string GetEnterpriseName(this string oid)
        {
            if (string.IsNullOrEmpty(oid) || oid.Length < 6)
            {
                return null;
            }

            return GetEnterpriseName(new ObjectIdentifier(oid));
        }

        /// <summary>
        /// Method to return Enterprise name.
        /// </summary>
        /// <param name="oid">The ObjectIdentifier object.</param>
        /// <returns>Enterprise name</returns>
        public static string GetEnterpriseName(this ObjectIdentifier oid)
        {
            if (oid.IsSubOid(prefixOid) && oid.Oids.Count > 6)
            {
                string enterprise;
                if (!Names.TryGetValue(oid.Oids[6], out enterprise))
                {
                    enterprise = "Unknown (" + oid.Oids[6] + ")";
                }

                return enterprise;
            }

            return null;
        }
    }
}