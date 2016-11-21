
namespace Tx.Network.Snmp
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal static class VarBindExtensions
    {
        /// <summary>
        /// Searches the first sub oid.
        /// </summary>
        /// <param name="subOid">The sub oid.</param>
        /// <param name="varBind">The variable bind.</param>
        /// <param name="varBinds">The variable bind List.</param>
        /// <returns>Boolean value true if subOid is found else false</returns>
        public static bool SearchFirstSubOidWith(this IReadOnlyCollection<VarBind> varBinds, ObjectIdentifier subOid, out VarBind varBind)
        {
            bool isFound = false;
            varBind = default(VarBind);
            foreach (var item in varBinds)
            {
                if (item.Oid.IsSubOid(subOid))
                {
                    varBind = item;
                    isFound = true;
                    break;
                }
            }

            return isFound;
        }

        /// <summary>
        /// Searches the last sub oid with.
        /// </summary>
        /// <param name="subOid">The sub oid.</param>
        /// <param name="varBind">The variable bind.</param>
        /// <param name="varBinds">The variable bind List.</param>
        /// <returns>Boolean value true if subOid is found else false</returns>
        public static bool SearchLastSubOidWith(this ReadOnlyCollection<VarBind> varBinds, ObjectIdentifier subOid, out VarBind varBind)
        {
            bool isFound = false;
            varBind = default(VarBind);
            for (int i = varBinds.Count - 1; i >= 0; i--)
            {
                if (varBinds[i].Oid.IsSubOid(subOid))
                {
                    varBind = varBinds[i];
                    isFound = true;
                    break;
                }
            }

            return isFound;
        }

        /// <summary>
        /// Gets all oids starting with.
        /// </summary>
        /// <param name="subOid">The sub oid.</param>
        ///  <param name="varBinds">The variable bind List.</param>
        /// <returns>IEnumerable of VarBind</returns>
        public static IEnumerable<VarBind> GetAllOidsStartingWith(this ReadOnlyCollection<VarBind> varBinds, ObjectIdentifier subOid)
        {
            for (int i = 0; i < varBinds.Count; i++)
            {
                if (varBinds[i].Oid.IsSubOid(subOid))
                {
                    yield return varBinds[i];
                }
            }
        }
    }
}
