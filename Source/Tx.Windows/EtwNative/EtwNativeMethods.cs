using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Tx.Windows
{
    [SuppressUnmanagedCodeSecurityAttribute]
    static class EtwNativeMethods 
    {
        public const Int32 ErrorNotFound = 0x000000a1;
        public const Int32 ErrorUnreadable = 0x00000570;
        public const uint TraceModeRealTime = 0x00000100;
        public const uint TraceModeEventRecord = 0x10000000;
        public static readonly ulong InvalidHandle = (Environment.OSVersion.Version.Major >= 6 ? 0x00000000FFFFFFFF : 0xFFFFFFFFFFFFFFFF);

        public const UInt16 EVENT_HEADER_FLAG_32_BIT_HEADER = 0x20;
        public const UInt16 EVENT_HEADER_FLAG_64_BIT_HEADER = 0x40;

        [DllImport("advapi32.dll", ExactSpelling = true, EntryPoint = "OpenTraceW", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern UInt64 OpenTrace(ref EVENT_TRACE_LOGFILE logfile);

        [DllImport("advapi32.dll", ExactSpelling = true, EntryPoint = "ProcessTrace")]
        public static extern Int32 ProcessTrace(UInt64[] HandleArray,
                                                 UInt32 HandleCount,
                                                 IntPtr StartTime,
                                                 IntPtr EndTime);

        [DllImport("advapi32.dll", ExactSpelling = true, EntryPoint = "CloseTrace")]
        public static extern Int32 CloseTrace(UInt64 traceHandle);

        [DllImport("tdh.dll", ExactSpelling = true, EntryPoint = "TdhGetEventInformation")]
        public static extern Int32 TdhGetEventInformation(
            ref EVENT_RECORD Event,
            UInt32 TdhContextCount,
            IntPtr TdhContext,
            [Out] IntPtr eventInfoPtr,
            ref Int32 BufferSize);
    }

    [SuppressUnmanagedCodeSecurityAttribute]
    delegate void PEVENT_RECORD_CALLBACK([In] ref EVENT_RECORD eventRecord);

    [Serializable]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct Win32TimeZoneInfo 
    {
        public Int32 Bias;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] StandardName;
        public SystemTime StandardDate;
        public Int32 StandardBias;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] DaylightName;
        public SystemTime DaylightDate;
        public Int32 DaylightBias;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    struct SystemTime 
    {
        public Int16 Year;
        public Int16 Month;
        public Int16 DayOfWeek;
        public Int16 Day;
        public Int16 Hour;
        public Int16 Minute;
        public Int16 Second;
        public Int16 Milliseconds;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    struct TRACE_LOGFILE_HEADER 
    {
        public UInt32 BufferSize;
        public UInt32 Version;
        public UInt32 ProviderVersion;
        public UInt32 NumberOfProcessors;
        public Int64 EndTime;
        public UInt32 TimerResolution;
        public UInt32 MaximumFileSize;
        public UInt32 LogFileMode;
        public UInt32 BuffersWritten;
        public UInt32 StartBuffers;
        public UInt32 PointerSize;
        public UInt32 EventsLost;
        public UInt32 CpuSpeedInMHz;
        public IntPtr LoggerName;
        public IntPtr LogFileName;
        public Win32TimeZoneInfo TimeZone;
        public Int64 BootTime;
        public Int64 PerfFreq;
        public Int64 StartTime;
        public UInt32 ReservedFlags;
        public UInt32 BuffersLost;
    }

    [Serializable]
    enum PROPERTY_FLAGS 
    {
        PropertyStruct = 0x1,
        PropertyParamLength = 0x2,
        PropertyParamCount = 0x4,
        PropertyWBEMXmlFragment = 0x8,
        PropertyParamFixedLength = 0x10
    }

    [Serializable]
    enum TdhInType : ushort 
    {
        Null,
        UnicodeString,
        AnsiString,
        Int8,
        UInt8,
        Int16,
        UInt16,
        Int32,
        UInt32,
        Int64,
        UInt64,
        Float,
        Double,
        Boolean,
        Binary,
        Guid,
        Pointer,
        FileTime,
        SystemTime,
        SID,
        HexInt32,
        HexInt64,  // End of winmeta intypes
        CountedString = 300, // Start of TDH intypes for WBEM
        CountedAnsiString,
        ReversedCountedString,
        ReversedCountedAnsiString,
        NonNullTerminatedString,
        NonNullTerminatedAnsiString,
        UnicodeChar,
        AnsiChar,
        SizeT,
        HexDump,
        WbemSID
    };

    [Serializable]
    enum TdhOutType : ushort 
    {
        Null,
        String,
        DateTime,
        Byte,
        UnsignedByte,
        Short,
        UnsignedShort,
        Int,
        UnsignedInt,
        Long,
        UnsignedLong,
        Float,
        Double,
        Boolean,
        Guid,
        HexBinary,
        HexInt8,
        HexInt16,
        HexInt32,
        HexInt64,
        PID,
        TID,
        PORT,
        IPV4,
        IPV6,
        SocketAddress,
        CimDateTime,
        EtwTime,
        Xml,
        ErrorCode,              // End of winmeta outtypes
        ReducedString = 300,    // Start of TDH outtypes for WBEM
        NoPrint
    };

    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    sealed class EVENT_PROPERTY_INFO 
    {
        [FieldOffset(0)]
        public PROPERTY_FLAGS Flags;
        [FieldOffset(4)]
        public UInt32 NameOffset;

        [StructLayout(LayoutKind.Sequential)]
        public struct NonStructType 
        {
            public TdhInType InType;
            public TdhOutType OutType;
            public UInt32 MapNameOffset;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct StructType 
        {
            public UInt16 StructStartIndex;
            public UInt16 NumOfStructMembers;
            private UInt32 _Padding;
        }

        [FieldOffset(8)]
        public NonStructType NonStructTypeValue;
        [FieldOffset(8)]
        public StructType StructTypeValue;

        [FieldOffset(16)]
        public UInt16 CountPropertyIndex;
        [FieldOffset(18)]
        public UInt16 LengthPropertyIndex;
        [FieldOffset(20)]
        private UInt32 _Reserved;
    }

    [Serializable]
    enum TEMPLATE_FLAGS 
    {
        TemplateEventDdata = 1,
        TemplateUserData = 2
    }

    [Serializable]
    enum DECODING_SOURCE 
    {
        DecodingSourceXmlFile,
        DecodingSourceWbem,
        DecodingSourceWPP
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    sealed class TRACE_EVENT_INFO 
    {
        public Guid ProviderGuid;
        public Guid EventGuid;
        public EVENT_DESCRIPTOR EventDescriptor;
        public DECODING_SOURCE DecodingSource;
        public UInt32 ProviderNameOffset;
        public UInt32 LevelNameOffset;
        public UInt32 ChannelNameOffset;
        public UInt32 KeywordsNameOffset;
        public UInt32 TaskNameOffset;
        public UInt32 OpcodeNameOffset;
        public UInt32 EventMessageOffset;
        public UInt32 ProviderMessageOffset;
        public UInt32 BinaryXmlOffset;
        public UInt32 BinaryXmlSize;
        public UInt32 ActivityIDNameOffset;
        public UInt32 RelatedActivityIDNameOffset;
        public UInt32 PropertyCount;
        public UInt32 TopLevelPropertyCount;
        public TEMPLATE_FLAGS Flags;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    struct EVENT_TRACE_HEADER 
    {
        public UInt16 Size;
        public UInt16 FieldTypeFlags;
        public UInt32 Version;
        public UInt32 ThreadId;
        public UInt32 ProcessId;
        public Int64 TimeStamp;
        public Guid Guid;
        public UInt32 KernelTime;
        public UInt32 UserTime;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    struct EVENT_TRACE 
    {
        public EVENT_TRACE_HEADER Header;
        public UInt32 InstanceId;
        public UInt32 ParentInstanceId;
        public Guid ParentGuid;
        public IntPtr MofData;
        public UInt32 MofLength;
        public UInt32 ClientContext;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct EVENT_TRACE_LOGFILE 
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string LogFileName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string LoggerName;
        public Int64 CurrentTime;
        public UInt32 BuffersRead;
        public UInt32 ProcessTraceMode;
        public EVENT_TRACE CurrentEvent;
        public TRACE_LOGFILE_HEADER LogfileHeader;
        public IntPtr BufferCallback;
        public UInt32 BufferSize;
        public UInt32 Filled;
        public UInt32 EventsLost;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public PEVENT_RECORD_CALLBACK EventRecordCallback;
        public UInt32 IsKernelTrace;
        public IntPtr Context;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct EVENT_DESCRIPTOR 
    {
        public UInt16 Id;
        public byte Version;
        public byte Channel;
        public byte Level;
        public byte Opcode;
        public UInt16 Task;
        public UInt64 Keyword;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct EVENT_HEADER 
    {
        public UInt16 Size;
        public UInt16 HeaderType;
        public UInt16 Flags;
        public UInt16 EventProperty;
        public UInt32 ThreadId;
        public UInt32 ProcessId;
        public Int64 TimeStamp;
        public Guid ProviderId;
        public EVENT_DESCRIPTOR EventDescriptor;
        public UInt64 ProcessorTime;
        public Guid ActivityId;
    }

    [Serializable]
    public enum EventHeaderExtType : ushort
    {
        RelatedActivityId = 1,
        Sid,
        TsSid,
        InstanceInfo,
        StackTrace32,
        StackTrace64
    }
    
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct EventHeaderExtendedDataItem
    {
        UInt16 Reserved1;
        public EventHeaderExtType ExtType;
        UInt16 Reserved2;
        public UInt16 DataSize;
        public IntPtr DataPtr;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct EVENT_RECORD 
    {
        public EVENT_HEADER EventHeader;
        public ETW_BUFFER_CONTEXT BufferContext;
        public UInt16 ExtendedDataCount;
        public UInt16 UserDataLength;
        public IntPtr ExtendedData;
        public IntPtr UserData;
        public IntPtr UserContext;            

        [StructLayout(LayoutKind.Sequential)]
        public struct ETW_BUFFER_CONTEXT 
        {
            public byte ProcessorNumber;
            public byte Alignment;
            public UInt16 LoggerId;
        }
    }
}
