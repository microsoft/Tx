using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Tx.Windows
{
    /// <summary>
    /// EtwNativeEvent represents event from ETW as native structure
    /// 
    /// Note that the structure is not a C# object - it is not on the heap or on the stack
    /// instead, it is in ETW's dedicated buffer
    /// </summary>
    public unsafe struct EtwNativeEvent
    {
        internal EVENT_RECORD* record;

#region struct EVENT_HEADER
        public UInt16 Size { get { return record->EventHeader.Size; } }
        public UInt16 HeaderType { get { return record->EventHeader.HeaderType; } }
        public UInt16 Flags { get { return record->EventHeader.Flags; } }
        public UInt16 EventProperty { get { return record->EventHeader.EventProperty; } }
        public UInt32 ThreadId { get { return record->EventHeader.ThreadId; } }
        public UInt32 ProcessId { get { return record->EventHeader.ProcessId; } }
        public DateTimeOffset TimeStamp { get { return TimeUtil.DateTimeOffsetFromFileTime(record->EventHeader.TimeStamp); } }
        public Int64 TimeStampRaw { get { return record->EventHeader.TimeStamp; } }
        public Guid ProviderId { get { return record->EventHeader.ProviderId; } }
#region struct EVENT_DESCRIPTOR
        public UInt16 Id { get { return record->EventHeader.EventDescriptor.Id; } }
        public byte Version { get { return record->EventHeader.EventDescriptor.Version; } }
        public byte Channel { get { return record->EventHeader.EventDescriptor.Channel; } }
        public byte Level { get { return record->EventHeader.EventDescriptor.Level; } }
        public byte Opcode { get { return record->EventHeader.EventDescriptor.Opcode; } }
        public UInt16 Task { get { return record->EventHeader.EventDescriptor.Task; } }
        public UInt64 Keyword { get { return record->EventHeader.EventDescriptor.Keyword; } }
#endregion
        public UInt64 ProcessorTime { get { return record->EventHeader.ProcessorTime; } }
        public Guid ActivityId { get { return record->EventHeader.ActivityId; } }
#endregion
        public UInt16 ExtendedDataCount { get { return record->ExtendedDataCount; } }
        public UInt16 UserDataLength { get { return record->UserDataLength; } }
        public IntPtr ExtendedData { get { return record->ExtendedData; } }
        public IntPtr UserData { get { return record->UserData; } }
        public IntPtr UserContext { get { return record->UserContext; } }
        
        internal byte* _data;
        internal byte* _end;
        internal uint _length; // used to remember the last UInt32, and as length to read the following win:Binary

        public char ReadAnsiChar()
        {
            byte value = *((byte*)_data);
            _data += sizeof(byte);
            return (char)value;
        }

        public char ReadChar()
        {
            char value = *((char*)_data);
            _data += sizeof(char);
            return value;
        }

        public double ReadDouble(int length)
        {
            double value = *((double*)_data);
            _data += sizeof(double);
            return value;
        }

        public sbyte ReadInt8()
        {
            sbyte value = *((sbyte*)_data);
            _data += sizeof(sbyte);
            return value;
        }

        public byte ReadUInt8()
        {
            byte value = *((byte*)_data);
            _data +=sizeof(byte);
            return value;
        }

        public short ReadInt16()
        {
            short value = *((short*)_data);
            _data += sizeof(short);
           return value;
        }

        public ushort ReadUInt16()
        {
            ushort value = *((ushort*)_data);
            _data += sizeof(ushort);
            return value;
        }

        public int ReadInt32()
        {
            int value = *((int*)_data);
            _data +=sizeof(int);
            return value;
        }

        public uint ReadUInt32()
        {
            uint value = *((uint*)_data);
            _data +=sizeof(uint);
            _length = value;
            return value;
        }

        public long ReadInt64()
        {
            long value = *((long*)_data);
            _data +=sizeof(long);
            return value;
        }

        public ulong ReadUInt64()
        {
            ulong value = *((ulong*)_data);
            _data +=sizeof(ulong);
            return value;
        }

        public ulong ReadPointer()
        {
            if ((Flags & EtwNativeMethods.EVENT_HEADER_FLAG_32_BIT_HEADER) != 0)
            {
                return ReadUInt32();
            }
            else
            {
                return ReadUInt64();
            }
        }

        public bool ReadBoolean()
        {
            bool value = *((int*)_data) != 0;
            _data += 4; // 32 bit value. See: http://msdn.microsoft.com/en-us/library/aa382774(v=vs.85) 
            return value;
        }

        public float ReadFloat()
        {
            float value = *((float*)_data);
            _data +=sizeof(float);
            return value;
        }

        public double ReadDouble()
        {
            double value = *((double*)_data);
            _data +=sizeof(double);
            return value;
        }
        
        public DateTime ReadFileTime()
        {
            long value = *((long*)_data);
            _data +=sizeof(long);
            return DateTime.FromFileTimeUtc(value);
        }

        public DateTime ReadSystemTime()
        {
            int year = ReadInt16();
            int month = ReadInt16();
            int dayOfWeek = ReadInt16();
            int day = ReadInt16();
            int hour = ReadInt16();
            int minute = ReadInt16();
            int second = ReadInt16();
            int milliseconds = ReadInt16();
            return new DateTime(year, month, day, hour, minute, second, milliseconds);
        }

        public Guid ReadGuid()
        {
            Guid value = new Guid(
                *((int*)_data),
                *((short*)((byte*)_data + 4)),
                *((short*)((byte*)_data + 6)),
                *((byte*)_data + 8),
                *((byte*)_data + 9),
                *((byte*)_data + 10),
                *((byte*)_data + 11),
                *((byte*)_data + 12),
                *((byte*)_data + 13),
                *((byte*)_data + 14),
                *((byte*)_data + 15));

            _data +=sizeof(Guid);
            return value;
        }

        public byte[] ReadBytes()
        {
            byte[] value = new byte[_length];
            fixed (byte* pb = value)
            {
                TypeServiceUtil.MemCopy((byte*)_data, pb, (int)_length);
            }
            _data += _length;
            return value;
        }

        public string ReadAnsiString(int length)
        {
            string str = Marshal.PtrToStringAnsi((IntPtr)_data, length);
            _data += length;
            return str;
        }
        
        public string ReadAnsiString()
        {
            byte* end;
            for (end = _data; end<_end; end++)
            {
                if (0 == *end)
                    break;
            };

            string str = Marshal.PtrToStringAnsi((IntPtr)_data, (int)(end-_data));
            int length = (str.Length + 1);
            _data += length;
            return str;
        }

        public string ReadUnicodeString()
        {
            string str = new string((char*)_data);

            // special case for missing 0 before the end of the buffer
            int maxLength = (int)(_end - _data) >> 1;
            if (str.Length > maxLength)
                str = str.Substring(0, maxLength);

            _data += (str.Length + 1)<<1;
            return str;
        }

        public string ReadUnicodeString(int fixedLength)
        {
            char* end;
            for (end = (char*)_data; end < (char*)_end; end ++)
            { 
                if (0 == *end)
                    break;
            };

            string str = Marshal.PtrToStringUni((IntPtr)_data, (int)(((byte*)end) - _data)>>1);
            int length = (str.Length + 1) * sizeof(char);
            if (fixedLength > 0)
            {
                _data += fixedLength * sizeof(char); 
            }
            else
            {
                _data += (length);
            }
            return str;
        }

        public string ReadAnsiStringPrefixLen()
        {
            int length = ReadInt16();
            string str = Marshal.PtrToStringAnsi((IntPtr)_data, length);
            _data += length;
            return str;
        }
        
        public string ReadUnicodeStringPrefixLen()
        {
            int length = ReadInt16() / 2;
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = (char)ReadInt16();
            }
            string str = new string(chars);
            return str;
        }

        public string ReadSid()
        {
            SecurityIdentifier sid = new SecurityIdentifier((IntPtr)_data);
            _data += sid.BinaryLength;
            return sid.Value;
        }

        public void ResetReader()
        {
            _data = (byte*)UserData.ToPointer();
            _end = _data + UserDataLength;
        }
    }
}
