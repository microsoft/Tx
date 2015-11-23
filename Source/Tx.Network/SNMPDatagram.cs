using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.DirectoryServices.Protocols;

namespace Tx.Network
{

    enum UniversalClassTags : byte
    {


        EOC = 0,
        BOOLEAN = 1,
        INTEGER = 2,
        BIT_STRING = 3,
        OCTET_STRING = 4,
        NULL = 5,
        OBJECT_IDENTIFIER = 6,
        Object_Descriptor = 7,
        EXTERNAL = 8,
        REAL = 9,
        ENUMERATED = 10,
        EMBEDDEDDV = 11,
        UTF8String = 12,
        RELATIVE_OID = 13,
        SEQUENCE_and_SEQUENCE_OF = 16,
        SET_and_SET_OF = 17,
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
        CHARACTER_STRING = 29,
        BMPString = 30
    }

    class SNMPDatagram
    {
        struct Encoding_Structure_ShortLen
        {
            IdByte ID;
            LengthShortForm Length;
            byte[] Contents;
            byte[] EndOfContent; //only if the length requires it.
        }
        struct Encoding_Structure_LongLen
        {
            IdByte ID;
            LengthLongForm Length;
            byte[] Contents;
            byte[] EndOfContent;
        }

        struct IdByte
        {
            ClassBits classbits;
            PC PC;
            object TagNumber;


        }

        enum ClassBits : byte
        {
            Universal = 0,
            Application,
            ContextSpecific,
            Private
        }

        enum PC : byte
        {
            Primitive = 0,
            Constructed = 1
        }

        struct LongTagNumbers
        {
            byte Leading; //first byte as classbits PCbit 11111
            byte Bit8; // 1 except for the last byte
            byte Bits7To1; //byte values with bit 7 the MSB. bytes are alligned  as 1:bits/1:bits/1:bits/lastByte
            byte LastOctet;
        }

        struct DefLengthShortForm
        {
            //0,Octets count 7-1 (big endian?)
            byte Length;
        }

        struct DefLengthLongForm
        {
            //all 1's nto allowed; 
            byte FirstByte;
            byte[] Length;

        }

        struct IndefFormLength
        {
            byte FirstByte; //should be 1000/0000
            byte[] LengthBytes; //same as Long form?
            byte eocByte1;
            byte eocByte2;
        }

        ushort EOC = (ushort)0;

        enum BERBool : int
        {
            BERTrue = 66047, //can be any non-zero vallue
            BERFalse = 65792
        }

        static object MakeBERInt(int i)
        {
            var j = i >> 31 - 1 != 0 ? (~(i & (int.MaxValue >> 1))) + 1 : i;


            return j;
        }
        static long MakeBERLong(long i)
        {
            var j = i >> 63 != 0 ? (~(i & (long.MaxValue >> 1))) + 1 : i;


            return j;
        }

        static double MakeBERDouble(double i)
        {
            if (i == 0) return double.NaN;

            throw new NotImplementedException();
        }

     
    }
}
