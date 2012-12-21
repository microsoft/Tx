using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace EtwLoadGenerator
{
    public enum EventSize
    {
        Small = 1,
        Medium = 2,
        Large = 3,
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    struct SmallEventStruct
    {
        public uint ID;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    unsafe struct MediumEventStruct
    {
        public uint ID;
        public uint number1;
        public uint number2;
        public fixed char string1[1024];
        public fixed char string2[1024];
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    unsafe struct LargeEventStruct
    {
        public uint ID;
        public uint number1;
        public uint number2;
        public uint number3;
        public uint number4;
        public uint number5;
        public uint number6;
        public uint number7;
        public uint number8;
        public uint number9;
        public uint number10;
        public fixed char string1[1024];
        public fixed char string2[1024];
        public fixed char string3[1024];
        public fixed char string4[1024];
        public fixed char string5[1024];
        public fixed char string6[1024];
        public fixed char string7[1024];
        public fixed char string8[1024];
        public fixed char string9[1024];
        public fixed char string10[1024];
    }
}
