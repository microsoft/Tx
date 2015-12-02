using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tx.Network.Asn1
{
    // This implements BasicEncodingReader and suppurting types
    // this allows reading data serialized as per the Basic Encoding Rules
    // http://en.wikipedia.org/wiki/X.690#BER_encoding
    public enum Asn1Class : int
    {
        Universal = 0,
        Application = 1,
        ContextSpecific = 2,
        Private = 3
    };

    public enum Asn1Tag : int
    {
        EndOfContent = 0,
        Boolean = 1,
        Integer = 2,
        BitString = 3,
        OctetString = 4,
        Null = 5,
        ObjectIdentifier = 6,
        Sequence = 16,
        SetOf = 17, 
        NumericString = 18, 
        PrintableString = 19, 
        T61String = 20, 
        VideotexString = 21, 
        IA5String = 22, 
        UTCTime = 23, 
        GeneralizedTime = 24, 
        GraphicString = 25, 
        VisibleString = 26,
        GeneralString = 27, 
        UniversalString = 28, 
        CharacterString = 29, 
        BMPString = 30 
    }

    public class Asn1Type 
    {
        public byte Byte { get; private set; }

        public Asn1Type(byte rawByte)
        {
            this.Byte = rawByte;
        }
        public Asn1Class Class { get { return (Asn1Class)((Byte & 0xC0) >> 6); } } // Bits 8 & 7

        public bool IsPrimitive { get { return ((Byte & 0x20) >> 5) == 0; } } // Bit 6
        public bool IsConstructed { get { return ((Byte & 0x20) >> 5) != 0; } } // Bit 6
        public Asn1Tag Tag { get { return (Asn1Tag)(Byte & 0x1F); } } // Bits 5-1
    }
    public class BasicEncodingReader
    {
        BinaryReader _reader;
        public BasicEncodingReader(byte[] datagram)
        {
            MemoryStream stream = new MemoryStream(datagram);
            _reader = new BinaryReader(stream);
        }
        public BasicEncodingReader(Stream stream)
        {
            _reader = new BinaryReader(stream);
        }

        public long Position { get { return _reader.BaseStream.Position; } }
        public long Length { get { return _reader.BaseStream.Length; } }
        public Asn1Type ReadType()
        {
            if (_reader.BaseStream.Position >= _reader.BaseStream.Length)
                return new Asn1Type((byte)Asn1Tag.EndOfContent);

            byte b = _reader.ReadByte();
            return new Asn1Type(b);
        }

        public int ReadLength()
        {
            int length = _reader.ReadByte();
            if (length == 0x80)
                throw new NotSupportedException("Indefinite lengths are not implemented.");

            if (length <= 127)
                return length;

            // The length is ecoded in the long form.
            int numLengthBytes = (length & 0x7F);
            byte[] bytes = _reader.ReadBytes(numLengthBytes);

            length = 0;
            int shift = 0;

            for (int i = numLengthBytes - 1; i >= 0; i--)
                length += bytes[i] << (shift++ * 8);

            return length;
        }

        public void Skip(int length)
        {
            var bytes = _reader.ReadBytes(length);
        }

        public byte ReadByte()
        {
            return _reader.ReadByte();
        }

        public int ReadInteger()
        {
            Asn1Type type = ReadType();
            if (!type.IsPrimitive && type.Tag == Asn1Tag.Integer)
                throw new Exception("Expected integer");

            int length = ReadLength();
            return ReadInteger(length);
        }

        public int ReadInteger(int length)
        {
            byte[] bytes = _reader.ReadBytes(length);

            int result = 0;
            int shift = 0;

            for (int i = length - 1; i >= 0; i--)
                result += bytes[i] << (shift++ * 8);

            return result;
        }

        public string ReadOctetString()
        {
            Asn1Type type = ReadType();
            if (!type.IsPrimitive && type.Tag == Asn1Tag.OctetString)
                throw new Exception("Expected OctetString");

            int length = ReadLength();
            return ReadOctetString(length);
        }
        public string ReadOctetString(int length)
        {
            byte[] bytes = _reader.ReadBytes(length);
            string s = Encoding.UTF8.GetString(bytes);
            return s;
        }

        //public string ReadOID(int length)
        //{
        //    List<byte> bytes = new List<byte>();
        //    byte first = _reader.ReadByte();

        //    bytes.Add((byte)(first / 40));
        //    bytes.Add((byte)(first % 40));

        //    for (int i = 1; i < length; i++)
        //        bytes.Add(_reader.ReadByte());

        //    StringBuilder sb = new StringBuilder();
        //    for (int i = 0; i < bytes.Count; i++)
        //    {
        //        if (i > 0) sb.Append('.');
        //        sb.Append(bytes[i]);
        //    }

        //    return sb.ToString();
        //}

        public string ReadOID(int length)
        {
            List<int> tokens = new List<int>();
            byte first = _reader.ReadByte();

            tokens.Add((byte)(first / 40));
            tokens.Add((byte)(first % 40));

            for (int i = 1; i < length; i++)
            {
                int value = 0;

                while(true)
                {
                    byte b = _reader.ReadByte();
                    value += b & 0x7F;

                    if (b < 0x80) break;

                    value <<= 7;
                    i++;
                }

                tokens.Add(value);
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < tokens.Count; i++)
            {
                if (i > 0) sb.Append('.');
                sb.Append(tokens[i]);
            }

            return sb.ToString();
        }

        public object ReadPrimiveType(Asn1Type type, int length)
        {
            switch (type.Tag)
            {
                case Asn1Tag.Null:
                    return null;

                case Asn1Tag.Integer:
                    return ReadInteger(length);

                case Asn1Tag.OctetString:
                    return ReadOctetString(length);

                case Asn1Tag.ObjectIdentifier:
                    return ReadOID(length);

                default:
                    Skip(length);
                    return "NYI: " + type.Tag;
            }
        }

        public List<object> ReadConstructedType()
        {
            var type = ReadType();
            if (!type.IsConstructed)
                throw new Exception("Expected constructed type");

            int length = ReadLength();
            return ReadConstructedType(length);
        }
        public List<object> ReadConstructedType(int length)
        {
            var values = new List<object>();
            long endPosition = Position + length;

            while(Position < endPosition)
            {
                Asn1Type type = ReadType();
                if (type.Byte == 0) break;

                length = ReadLength();

                if (type.IsPrimitive)
                    values.Add(ReadPrimiveType(type, length));
                else
                    values.Add(ReadConstructedType(length));
            }

            return values;
        }

        public static string ReadAllText(byte[] datagram)
        {
            BasicEncodingReader reader = new BasicEncodingReader(datagram);
            var all = reader.ReadConstructedType();

            StringBuilder sb= new StringBuilder();
            FormatContent(ref sb, all, "");
            return sb.ToString();
        }
        static void FormatContent(ref StringBuilder sb, List<object> list, string prefix)
        {
            foreach (object o in list)
            {
                if (o == null)
                {
                    sb.Append(prefix);
                    sb.AppendLine("(null)");
                }
                else if (o.GetType() == typeof(List<object>))
                {
                    sb.Append(prefix);
                    sb.AppendLine("------");
                    FormatContent(ref sb, (List<object>)o, prefix + "    ");
                }
                else
                {
                    sb.Append(prefix);
                    sb.Append(o.GetType().Name);
                    sb.Append(": ");
                    sb.Append(o);
                    sb.AppendLine();
                }
            }
        }
    }
}
