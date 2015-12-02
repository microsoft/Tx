using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tx.Network.Asn1;

namespace Tx.Network.Snmp
{
    public enum PduType
    {
        Get = 0,
        GetResponse = 1,
        Trap = 2
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

        public PDU(byte[] datagram)
        {
            MemoryStream stream = new MemoryStream(datagram);
            BasicEncodingReader _reader = new BasicEncodingReader(stream);

            Asn1Type seqType = _reader.ReadType();
            int seqLength = _reader.ReadLength();

            Version = _reader.ReadInteger();
            Community = _reader.ReadOctetString();

            Asn1Type t = _reader.ReadType();
            PduType = (PduType)(t.Byte & 0x1F);

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
                // Cut the last token from the OID ... need Mark's help to understnad how this is used
                string oid = (string)seq[0];
                int index = oid.LastIndexOf('.');
                if (oid.Length - index > 2)
                    oid = oid.Substring(0, index);

                VarBinds.Add(oid, seq[1]);
            }
        }

        public object GetVar(string oid)
        {
            return VarBinds[oid];
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
}
