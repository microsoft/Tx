// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace Tx.Windows
{
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    internal class PdhLogHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public PdhLogHandle()
            : base(true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            return PdhNativeMethods.PdhCloseLog(handle, PdhNativeMethods.PDH_FLAGS_CLOSE_QUERY) == 0;
        }
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    internal class PdhQueryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public PdhQueryHandle()
            : base(true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            return PdhNativeMethods.PdhCloseQuery(handle) == 0;
        }
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    internal class PdhCounterHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public PdhCounterHandle()
            : base(true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            return PdhNativeMethods.PdhRemoveCounter(handle) == 0;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct PDH_FMT_COUNTERVALUE
    {
        [FieldOffset(0)] public uint CStatus;

        [FieldOffset(8)] public int longValue;

        [FieldOffset(8)] public double doubleValue;

        [FieldOffset(8)] public Int64 largeValue;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PDH_FMT_COUNTERVALUE_ITEM
    {
        //[MarshalAs(UnmanagedType.LPWStr)]
        //public string szName;

        public IntPtr szName;
        public PDH_FMT_COUNTERVALUE FmtValue;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct PDH_COUNTER_INFO
    {
        public UInt32 dwLength;
        public UInt32 dwType;
        public UInt32 CVersion;
        public UInt32 CStatus;
        public Int64 lScale;
        public Int64 lDefaultScale;
        public IntPtr dwUserData;
        public IntPtr dwQueryUserData;
        public IntPtr szFullPath;
        public IntPtr szMachineName;
        public IntPtr szObjectName;
        public IntPtr szInstanceName;
        public IntPtr szParentInstance;
        public UInt32 dwInstanceIndex;
        public IntPtr szCounterName;
        public IntPtr szExplainText;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PDH_TIME_INFO
    {
        public UInt64 StartTime;
        public UInt64 EndTime;
        public uint SampleCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PDH_RAW_COUNTER
    {
        public uint CStatus;
        public FILETIME TimeStamp;
        public Int64 FirstValue;
        public Int64 SecondValue;
        public uint MultiCount;
    }

    internal enum PdhStatus : uint
    {
        PDH_CSTATUS_VALID_DATA = 0x00000000,
        PDH_CSTATUS_NEW_DATA = 0x00000001,
        PDH_CSTATUS_NO_MACHINE = 0x800007D0,
        PDH_CSTATUS_NO_INSTANCE = 0x800007D1,
        PDH_MORE_DATA = 0x800007D2,
        PDH_CSTATUS_ITEM_NOT_VALIDATED = 0x800007D3,
        PDH_RETRY = 0x800007D4,
        PDH_NO_DATA = 0x800007D5,
        PDH_CALC_NEGATIVE_DENOMINATOR = 0x800007D6,
        PDH_CALC_NEGATIVE_TIMEBASE = 0x800007D7,
        PDH_CALC_NEGATIVE_VALUE = 0x800007D8,
        PDH_DIALOG_CANCELLED = 0x800007D9,
        PDH_END_OF_LOG_FILE = 0x800007DA,
        PDH_ASYNC_QUERY_TIMEOUT = 0x800007DB,
        PDH_CANNOT_SET_DEFAULT_REALTIME_DATASOURCE = 0x800007DC,
        PDH_CSTATUS_NO_OBJECT = 0xC0000BB8,
        PDH_CSTATUS_NO_COUNTER = 0xC0000BB9,
        PDH_CSTATUS_INVALID_DATA = 0xC0000BBA,
        PDH_MEMORY_ALLOCATION_FAILURE = 0xC0000BBB,
        PDH_INVALID_HANDLE = 0xC0000BBC,
        PDH_INVALID_ARGUMENT = 0xC0000BBD,
        PDH_FUNCTION_NOT_FOUND = 0xC0000BBE,
        PDH_CSTATUS_NO_COUNTERNAME = 0xC0000BBF,
        PDH_CSTATUS_BAD_COUNTERNAME = 0xC0000BC0,
        PDH_INVALID_BUFFER = 0xC0000BC1,
        PDH_INSUFFICIENT_BUFFER = 0xC0000BC2,
        PDH_CANNOT_CONNECT_MACHINE = 0xC0000BC3,
        PDH_INVALID_PATH = 0xC0000BC4,
        PDH_INVALID_INSTANCE = 0xC0000BC5,
        PDH_INVALID_DATA = 0xC0000BC6,
        PDH_NO_DIALOG_DATA = 0xC0000BC7,
        PDH_CANNOT_READ_NAME_STRINGS = 0xC0000BC8,
        PDH_LOG_FILE_CREATE_ERROR = 0xC0000BC9,
        PDH_LOG_FILE_OPEN_ERROR = 0xC0000BCA,
        PDH_LOG_TYPE_NOT_FOUND = 0xC0000BCB,
        PDH_NO_MORE_DATA = 0xC0000BCC,
        PDH_ENTRY_NOT_IN_LOG_FILE = 0xC0000BCD,
        PDH_DATA_SOURCE_IS_LOG_FILE = 0xC0000BCE,
        PDH_DATA_SOURCE_IS_REAL_TIME = 0xC0000BCF,
        PDH_UNABLE_READ_LOG_HEADER = 0xC0000BD0,
        PDH_FILE_NOT_FOUND = 0xC0000BD1,
        PDH_FILE_ALREADY_EXISTS = 0xC0000BD2,
        PDH_NOT_IMPLEMENTED = 0xC0000BD3,
        PDH_STRING_NOT_FOUND = 0xC0000BD4,
        PDH_UNABLE_MAP_NAME_FILES = 0x80000BD5,
        PDH_UNKNOWN_LOG_FORMAT = 0xC0000BD6,
        PDH_UNKNOWN_LOGSVC_COMMAND = 0xC0000BD7,
        PDH_LOGSVC_QUERY_NOT_FOUND = 0xC0000BD8,
        PDH_LOGSVC_NOT_OPENED = 0xC0000BD9,
        PDH_WBEM_ERROR = 0xC0000BDA,
        PDH_ACCESS_DENIED = 0xC0000BDB,
        PDH_LOG_FILE_TOO_SMALL = 0xC0000BDC,
        PDH_INVALID_DATASOURCE = 0xC0000BDD,
        PDH_INVALID_SQLDB = 0xC0000BDE,
        PDH_NO_COUNTERS = 0xC0000BDF,
        PDH_SQL_ALLOC_FAILED = 0xC0000BE0,
        PDH_SQL_ALLOCCON_FAILED = 0xC0000BE1,
        PDH_SQL_EXEC_DIRECT_FAILED = 0xC0000BE2,
        PDH_SQL_FETCH_FAILED = 0xC0000BE3,
        PDH_SQL_ROWCOUNT_FAILED = 0xC0000BE4,
        PDH_SQL_MORE_RESULTS_FAILED = 0xC0000BE5,
        PDH_SQL_CONNECT_FAILED = 0xC0000BE6,
        PDH_SQL_BIND_FAILED = 0xC0000BE7,
        PDH_CANNOT_CONNECT_WMI_SERVER = 0xC0000BE8,
        PDH_PLA_COLLECTION_ALREADY_RUNNING = 0xC0000BE9,
        PDH_PLA_ERROR_SCHEDULE_OVERLAP = 0xC0000BEA,
        PDH_PLA_COLLECTION_NOT_FOUND = 0xC0000BEB,
        PDH_PLA_ERROR_SCHEDULE_ELAPSED = 0xC0000BEC,
        PDH_PLA_ERROR_NOSTART = 0xC0000BED,
        PDH_PLA_ERROR_ALREADY_EXISTS = 0xC0000BEE,
        PDH_PLA_ERROR_TYPE_MISMATCH = 0xC0000BEF,
        PDH_PLA_ERROR_FILEPATH = 0xC0000BF0,
        PDH_PLA_SERVICE_ERROR = 0xC0000BF1,
        PDH_PLA_VALIDATION_ERROR = 0xC0000BF2,
        PDH_PLA_VALIDATION_WARNING = 0x80000BF3,
        PDH_PLA_ERROR_NAME_TOO_LONG = 0xC0000BF4,
        PDH_INVALID_SQL_LOG_FORMAT = 0xC0000BF5,
        PDH_COUNTER_ALREADY_IN_QUERY = 0xC0000BF6,
        PDH_BINARY_LOG_CORRUPT = 0xC0000BF7,
        PDH_LOG_SAMPLE_TOO_SMALL = 0xC0000BF8,
        PDH_OS_LATER_VERSION = 0xC0000BF9,
        PDH_OS_EARLIER_VERSION = 0xC0000BFA,
        PDH_INCORRECT_APPEND_TIME = 0xC0000BFB,
        PDH_UNMATCHED_APPEND_COUNTER = 0xC0000BFC,
        PDH_SQL_ALTER_DETAIL_FAILED = 0xC0000BFD,
        PDH_QUERY_PERF_DATA_TIMEOUT = 0xC0000BFE,
        PDH_UNKNOWN = 0xFFFFFFFF
    }

    [Flags]
    internal enum PdhFormat : uint
    {
        PDH_FMT_RAW = 0x00000010,
        PDH_FMT_ANSI = 0x00000020,
        PDH_FMT_UNICODE = 0x00000040,
        PDH_FMT_LONG = 0x00000100,
        PDH_FMT_DOUBLE = 0x00000200,
        PDH_FMT_LARGE = 0x00000400,
        PDH_FMT_NOSCALE = 0x00001000,
        PDH_FMT_1000 = 0x00002000,
        PDH_FMT_NODATA = 0x00004000
    }

    internal enum PdhDetailLevel : uint
    {
        PERF_DETAIL_NOVICE = 100, // The uninformed can understand it
        PERF_DETAIL_ADVANCED = 200, // For the advanced user
        PERF_DETAIL_EXPERT = 300, // For the expert user
        PERF_DETAIL_WIZARD = 400 // For the system designer
    }

    [SuppressUnmanagedCodeSecurity]
    internal class PdhNativeMethods
    {
        #region A few common flags and status codes

        public const UInt32 PDH_FLAGS_CLOSE_QUERY = 1;
        public const UInt32 PDH_NO_MORE_DATA = 0xC0000BCC;
        public const UInt32 PDH_INVALID_DATA = 0xC0000BC6;
        public const UInt32 PDH_ENTRY_NOT_IN_LOG_FILE = 0xC0000BCD;

        #endregion

        [DllImport("pdh.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern PdhStatus PdhOpenQuery(
            string szDataSource,
            IntPtr dwUserData,
            out PdhQueryHandle phQuery);

        /// Opens a query against a bound input source.
        [DllImport("pdh.dll", SetLastError = true)]
        private static extern PdhStatus PdhOpenQueryH(
            PdhLogHandle hDataSource,
            IntPtr dwUserData,
            out PdhQueryHandle phQuery);

        /// Binds multiple logs files together.
        /// 
        /// Use this along with the API's ending in 'H' to string multiple files together.
        [DllImport("pdh.dll", SetLastError = true)]
        private static extern PdhStatus PdhBindInputDataSource(
            out PdhLogHandle phDataSource,
            string szLogFileNameList);

        [DllImport("pdh.dll", SetLastError = true)]
        public static extern PdhStatus PdhCloseLog(
            IntPtr hLog,
            long dwFlags);

        [DllImport("pdh.dll", SetLastError = true)]
        public static extern PdhStatus PdhCloseQuery(
            IntPtr hQuery);

        [DllImport("pdh.dll", SetLastError = true)]
        public static extern PdhStatus PdhRemoveCounter(
            IntPtr hQuery);

        [DllImport("pdh.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern PdhStatus PdhAddCounter(
            PdhQueryHandle hQuery,
            string szFullCounterPath,
            IntPtr dwUserData,
            out PdhCounterHandle phCounter);

        [DllImport("pdh.dll", SetLastError = true)]
        public static extern PdhStatus PdhCollectQueryData(
            PdhQueryHandle phQuery);

        [DllImport("pdh.dll", SetLastError = true)]
        public static extern PdhStatus PdhCollectQueryDataWithTime(
            PdhQueryHandle phQuery,
            out long timestamp);

        [DllImport("pdh.dll", CharSet = CharSet.Unicode)]
        public static extern PdhStatus PdhGetFormattedCounterValue(
            PdhCounterHandle phCounter,
            PdhFormat dwFormat,
            ref uint lpdwType,
            ref PDH_FMT_COUNTERVALUE pValue);

        [DllImport("pdh.dll", CharSet = CharSet.Unicode)]
        public static extern PdhStatus PdhGetDataSourceTimeRange(
            string szDataSource,
            ref uint pdwNumEntries,
            ref PDH_TIME_INFO pInfo,
            ref uint dwBufferSize
            );

        [DllImport("pdh.dll", CharSet = CharSet.Unicode)]
        public static extern PdhStatus PdhGetFormattedCounterArray(
            PdhCounterHandle phCounter,
            PdhFormat dwFormat,
            ref UInt32 dwBufferSize,
            out UInt32 dwBufferCount,
            IntPtr itemBuffer);

        [DllImport("pdh.dll", CharSet = CharSet.Unicode)]
        public static extern PdhStatus PdhGetRawCounterValue(
            PdhCounterHandle phCounter,
            out uint lpdwType,
            out PDH_RAW_COUNTER pValue);

        [DllImport("pdh.dll", CharSet = CharSet.Unicode)]
        public static extern PdhStatus PdhGetCounterTimeBase(
            PdhCounterHandle phCounter,
            out UInt64 pTimeBase);

        [DllImport("pdh.dll", CharSet = CharSet.Unicode)]
        public static extern PdhStatus PdhEnumMachines(
            string szDataSource,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] char[] mszMachineNameList,
            ref uint pcchBufferLength
            );

        [DllImport("pdh.dll", CharSet = CharSet.Unicode)]
        public static extern PdhStatus PdhEnumObjects(
            string szDataSource,
            string szMachineName,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] char[] mszObjectList,
            ref uint pcchBufferLength,
            PdhDetailLevel dwDetailLevel,
            int bRefresh
            );

        [DllImport("pdh.dll", CharSet = CharSet.Unicode)]
        public static extern PdhStatus PdhEnumObjectItems(
            string szDataSource,
            string szMachineName,
            string szObjectName,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)] char[] mszCounterList,
            ref uint pcchCounterListLength,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 7)] char[] mszInstanceList,
            ref uint pcchInstanceListLength,
            PdhDetailLevel dwDetailLevel,
            uint dwFlags
            );

        [DllImport("pdh.dll", CharSet = CharSet.Unicode)]
        public static extern PdhStatus PdhGetCounterInfo(
            PdhCounterHandle phCounter,
            bool bRetrieveExplainText,
            ref UInt32 pdwBufferSize,
            IntPtr lpBuffer);
    }
}