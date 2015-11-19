using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tx.Network
{

    enum UniversalClassTags: byte { 

        
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
            byte Class;
            byte PC;
            byte TagNumber;


        }

        struct ClassBits
        {
            byte Universal;
            byte Application;
            byte ContextSpecific;
            byte Private;
        }

        struct PC
        {
            byte Primitive;
            byte Constructed;
        }

        struct LongTagNumbers
        {
            byte Leading; //first byte as classbits PCbit 11111
            byte Bit8; // 1 except for the last byte
            byte Bits7To1; //byte values with bit 7 the MSB. bytes are alligned  as 1:bits/1:bits/1:bits/lastByte
            byte LastOctet;
        }

        struct LengthShortForm
        {
            //0,Octets count 7-1 (big endian?)
            byte Length;
        }

        struct LengthLongForm
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
        


    }
}
