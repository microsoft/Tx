using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Tx.Network.Asn1;

namespace Tx.Network.Snmp
{
    /// <summary>
    /// Type of the Protocol Data Unit, as defined in
    /// http://tools.ietf.org/search/rfc3416#page-22
    /// </summary>
    public enum PduType
    {
        GetRequest = 0,
        GetNextRequest = 1,
        Response = 2,
        SetRequest = 3,
        Trap = 4, // This existed in SNMPv1 and was obsoleted in SNMPv2
        GetBulkRequest = 5,
        InformRequest = 6,
        SNMPv2Trap = 7,
        Report = 8
    }
    public class PDU
    {
        public PduType PduType { get; private set; }
        public int Version { get; private set; }

        public string Community { get; private set; }
        public int RequestId { get; private set; }
        public int ErrorStatus { get; private set; }
        public int ErrorIndex { get; private set; }
        public SortedDictionary<string, object> VarBinds { get; private set; }
        public string TrapId { get { return (string)GetVar("1.3.6.1.6.3.1.1.4.1.0", null); } }
        public UdpDatagram UDP { get; private set; }

        public PDU(UdpDatagram datagram)
        {
            UDP = datagram;
            MemoryStream stream = new MemoryStream(datagram.UdpData);
            BasicEncodingReader _reader = new BasicEncodingReader(stream);

            Asn1Type seqType = _reader.ReadType();
            int seqLength = _reader.ReadLength();

            Version = _reader.ReadInteger();
            Community = _reader.ReadOctetString();

            Asn1Type t = _reader.ReadType();
            PduType = (PduType)(t.Byte & 0x1F);
            if (PduType == Snmp.PduType.Trap)
                throw new NotImplementedException("SNMP v1 traps are not yet implemented");

            int len = _reader.ReadLength();

            RequestId = _reader.ReadInteger();
            ErrorStatus = _reader.ReadInteger();
            ErrorIndex = _reader.ReadInteger();

            Asn1Type type = _reader.ReadType();
            if (type.Class != Asn1Class.Universal && type.Tag != Asn1Tag.Sequence)
                throw new Exception("Sequence expected");

            int length = _reader.ReadLength();

            VarBinds = new SortedDictionary<string, object>();
            var list = _reader.ReadConstructedType(length);
            foreach (List<object> seq in list)
            {
                string oid = (string)seq[0];
                VarBinds.Add(oid, seq[1]);
            }
        }

        /// <summary>
        /// Returns value by OID, or throws exception if the OID is not found
        /// </summary>
        /// <param name="oid">Key in VarBinds</param>
        /// <returns>Value</returns>
        public object GetVar(string oid)
        {
            return VarBinds[oid];
        }

        /// <summary>
        /// Returns value by OID, or default value if the OID is not found
        /// </summary>
        /// <param name="oid">Key in VarBinds</param>
        /// <param name="defaultValue">Value to use if the OID is not found</param>
        /// <returns>Value from VarBinds or defaultValue</returns>
        public object GetVar(string oid, object defaultValue)
        {
            var result = defaultValue;
            VarBinds.TryGetValue(oid, out result);
            return result;
        }

        public object GetFirstByPrefix(string oidPrefix)
        {
            var pair = VarBinds.Where(p => p.Key.StartsWith(oidPrefix)).FirstOrDefault();
            return pair.Value;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("version: ");
            sb.Append(Version);
            sb.AppendLine();
            sb.Append("community: ");
            sb.AppendLine(Community);
            sb.AppendLine();
            sb.Append("request-id: ");
            sb.Append(RequestId);
            sb.AppendLine();
            sb.Append("error-status: ");
            sb.Append(ErrorStatus);
            sb.AppendLine();
            sb.Append("error-index: ");
            sb.Append(ErrorIndex);
            sb.AppendLine();
            sb.AppendLine(" VarBinds:");

            foreach(var vb in VarBinds)
            {
                sb.Append("     OID=");
                sb.AppendLine(vb.Key);
                sb.Append("   Value=");
                if (vb.Value == null)
                    sb.AppendLine("(null)");
                else
                    sb.Append(vb.Value.ToString());
                sb.AppendLine();
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }

    public static class Enterprises
    {
        // Enterprise names as per http://www.iana.org/assignments/enterprise-numbers/enterprise-numbers
        static Dictionary<string, string> _names;
        const string Prefix = "1.3.6.1.4.1.";
        public static string GetName(string oid)
        {
            if (oid == null || !oid.StartsWith("1.3.6.1.4.1."))
                return null;

            string enterprise = null;
			int index = oid.IndexOf('.', Prefix.Length);
			string token = oid.Substring(Prefix.Length, index-Prefix.Length);
			if (!_names.TryGetValue(token, out enterprise))
				enterprise = "Unknown (" + token + ")";

            return enterprise;
        }

        static Enterprises()
        {
            _names = new Dictionary<string, string>();
            _names.Add("9", "Cisco");
            _names.Add("21296", "Infinera");
            _names.Add("3780", "Level3");
            _names.Add("6027", "Force10");
            _names.Add("30065", "Arista");
            _names.Add("2636", "Juniper");
            _names.Add("8072", "net-snmp");
        }
    }
}
