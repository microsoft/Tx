// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace Tx.Windows
{
    /// <summary>
    /// Class representing metadata about particular {provider, eventId}
    /// </summary>
    unsafe class EtwTdhEventInfo
    {
        EtwPropertyInfo[] _properties;
        Dictionary<string, object> _template;
        string _formatString;

        public EtwTdhEventInfo(ref EtwNativeEvent e)
        {
            IntPtr buffer = IntPtr.Zero;
            try
            {
                buffer = ReadTdhMetadata(ref e);
                CopyMetadata(buffer, ref e);
            }
            finally
            {
                if (buffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
        }

        /// <summary>
        /// Method to read ETW event as per the metadata stored in this class
        /// </summary>
        /// <param name="e">The ETW native event helper class for one-time read</param>
        /// <returns></returns>
        public IDictionary<string, object> Deserialize(ref EtwNativeEvent e)
        {
            Dictionary<string, object> instance = new Dictionary<string, object>(_template)
            {
                { "EventId", e.Id },
                { "Version", e.Version },
                { "TimeCreated", e.TimeStamp.UtcDateTime },

                { "ProcessId", e.ProcessId },
                { "ThreadId", e.ThreadId },
                { "ActivityId", e.ActivityId }
            };

            Dictionary<string, object> eventData = new Dictionary<string, object>();
            List<object> values = new List<object>();
            foreach (var p in _properties)
            {
                uint len = p.Length;
                if (p.LengthPropertyName != null)
                {
                    try
                    {
                        string num = Convert.ToString(eventData[p.LengthPropertyName]);
                        if (num.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                        {
                            num = num.Substring(2);
                            len = uint.Parse(num, System.Globalization.NumberStyles.HexNumber);
                        }
                        else
                        {
                            len = Convert.ToUInt32(num);
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.Data["ProviderGuid"] = e.ProviderId;
                        throw;
                    }
                }

                try
                {
                    object value = GetValue(p.Type, len, ref e);
                    value = FormatValue(p, value);
                    value = EtwTdhPostFormat.ApplyFormatting(e.ProviderId, e.Id, p.Name, value);
                    eventData.Add(p.Name, value);
                    values.Add(value);
                }
                catch (Exception)
                {
                    eventData.Add(p.Name, "Exception on retrieving value");
                }
            }

            instance.Add("EventData", eventData);

            if (_formatString == null)
            {
                instance.Add("Message", null);
            }
            else
            {
                string message = string.Format(_formatString, values.ToArray());
                instance.Add("Message", message);
            }

            return instance;
        }

        /// <summary>
        /// This function reads the event metadata from TDH into globally allocated buffer
        /// It is caller's responsibility to free the memory by calling Marshal.FreeHGlobal
        /// </summary>
        /// <param name="e">ETW native event interop wrapper structure</param>
        /// <returns>Pointer to newly allocated TRACE_EVENT_INFO structure</returns>
        IntPtr ReadTdhMetadata(ref EtwNativeEvent e)
        {
            int bufferSize = 0;
            int status = EtwNativeMethods.TdhGetEventInformation(ref *e.record, 0, IntPtr.Zero, IntPtr.Zero, ref bufferSize);
            if (122 != status) // ERROR_INSUFFICIENT_BUFFER
            {
                Exception ex = new Exception("Unexpected TDH status " + status);
                ex.Data["ProviderGuid"] = e.ProviderId;
                throw ex;
            }

            var mybuffer = Marshal.AllocHGlobal(bufferSize);
            status = EtwNativeMethods.TdhGetEventInformation(ref *e.record, 0, IntPtr.Zero, mybuffer, ref bufferSize);

            if (status != 0)
            {
                throw new Exception("TDH status " + status);
            }

            return mybuffer;
        }

        /// <summary>
        /// This function copies the information from the native TRACE_EVENT_INFO structure
        /// to property information in this oject 
        /// </summary>
        /// <param name="buffer">IntPtr to TRACE_EVENT_INFO structure</param>
        void CopyMetadata(IntPtr buffer, ref EtwNativeEvent e)
        {
            TRACE_EVENT_INFO* info = (TRACE_EVENT_INFO*)buffer;
            byte* start = (byte*)info;
            byte* end = start + sizeof(TRACE_EVENT_INFO);

            _template = new Dictionary<string, object>();
            _template.Add("Provider", CopyString(start, info->ProviderNameOffset));
            _template.Add("Level", CopyString(start, info->LevelNameOffset));
            _template.Add("Task", CopyString(start, info->TaskNameOffset));
            _template.Add("Opcode", CopyString(start, info->OpcodeNameOffset));
            _template.Add("Channel", CopyString(start, info->ChannelNameOffset));
            _formatString = TranslateFormatString(CopyString(start, info->EventMessageOffset));

            EVENT_PROPERTY_INFO* prop = (EVENT_PROPERTY_INFO*)end;
            var propList = new List<EtwPropertyInfo>();


            for (int i = 0; i < info->TopLevelPropertyCount; i++)
            {
                var propInfo = prop + i;
                var name = CopyString(start, propInfo->NameOffset);
                var type = (TdhInType)(*propInfo).NonStructTypeValue.InType;
                var outType = (TdhOutType)(*propInfo).NonStructTypeValue.OutType;

                EtwPropertyInfo property = null;
                if (propInfo->Flags == PROPERTY_FLAGS.PropertyParamLength)
                {
                    string lenPropertyName = propList[(int)(propInfo->LengthPropertyIndex)].Name;
                    property = new EtwPropertyInfo { Name = name, Type = type, OutType = outType, LengthPropertyName = lenPropertyName };
                }
                else
                {
                    ushort len = (*propInfo).LengthPropertyIndex;
                    property = new EtwPropertyInfo { Name = name, Length = len, Type = type, OutType = outType };
                }

                if (propInfo->NonStructTypeValue.MapNameOffset > 0)
                {
                    string mapName = CopyString(start, propInfo->NonStructTypeValue.MapNameOffset);
                    property.ValueMap = ReadTdhMap(mapName, ref e);
                }

                propList.Add(property);
            }

            _properties = propList.ToArray();
        }

        Dictionary<uint, string> ReadTdhMap(string mapName, ref EtwNativeEvent e)
        {
            IntPtr pMapName = Marshal.StringToBSTR(mapName);

            int bufferSize = 0;
            int status = EtwNativeMethods.TdhGetEventMapInformation(
                ref *e.record,
                pMapName,
                IntPtr.Zero, ref bufferSize);

            if (122 != status) // ERROR_INSUFFICIENT_BUFFER
            {
                throw new Exception("Unexpected TDH status " + status);
            }

            var mybuffer = Marshal.AllocHGlobal(bufferSize);
            status = EtwNativeMethods.TdhGetEventMapInformation(
                ref *e.record,
                pMapName,
                mybuffer, ref bufferSize);

            if (status != 0)
            {
                throw new Exception("TDH status " + status);
            }

            EVENT_MAP_INFO* mapInfo = (EVENT_MAP_INFO*)mybuffer;
            byte* startMap = (byte*)mapInfo;
            var name1 = CopyString(startMap, mapInfo->NameOffset);
            byte* endMap = startMap + sizeof(EVENT_MAP_INFO);

            var map = new Dictionary<uint, string>();
            for (int i = 0; i < mapInfo->EntryCount; i++)
            {
                EVENT_MAP_ENTRY* mapEntry = (EVENT_MAP_ENTRY*)endMap + i;
                uint value = mapEntry->Value;
                string name = CopyString(startMap, mapEntry->OutputOffset);
                map.Add(value, name);
            }

            return map;
        }

        string CopyString(byte* start, uint offset)
        {
            if (offset == 0)
                return null;

            byte* pName = start + offset;
            var name = Marshal.PtrToStringUni((IntPtr)(pName));
            return name;
        }

        static object GetValue(TdhInType type, uint len, ref EtwNativeEvent evt)
        {
            // please keep the code below in the same order is the reader methods in EtwNativeEvent
            switch (type)
            {
                case TdhInType.AnsiChar:
                    return evt.ReadAnsiChar();

                case TdhInType.UnicodeChar:
                    return evt.ReadChar();

                case TdhInType.Int8:
                    return evt.ReadByte();

                case TdhInType.UInt8:
                    return evt.ReadUInt8();

                case TdhInType.Int16:
                    return evt.ReadInt16();

                case TdhInType.UInt16:
                    return evt.ReadUInt16();

                case TdhInType.Int32:
                    return evt.ReadInt32();

                case TdhInType.UInt32:
                case TdhInType.HexInt32:
                    return evt.ReadUInt32();

                case TdhInType.Int64:
                    return evt.ReadInt64();

                case TdhInType.UInt64:
                case TdhInType.HexInt64:
                    return evt.ReadUInt64();

                case TdhInType.Pointer:
                    return evt.ReadPointer();

                case TdhInType.Boolean:
                    return evt.ReadBoolean();

                case TdhInType.Float:
                    return evt.ReadFloat();

                case TdhInType.Double:
                    return evt.ReadDouble();

                case TdhInType.FileTime:
                    return evt.ReadFileTime();

                case TdhInType.SystemTime:
                    return evt.ReadSystemTime();

                case TdhInType.Guid:
                    return evt.ReadGuid();

                case TdhInType.Binary:
                    return evt.ReadBytes(len);

                case TdhInType.UnicodeString:
                case TdhInType.SID:
                    if (len > 0)
                    {
                        return evt.ReadUnicodeString((int)len);
                    }
                    else
                    {
                        return evt.ReadUnicodeString();
                    }

                case TdhInType.AnsiString:
                    return evt.ReadAnsiString();

                default:
                    var ex = new Exception("Unknown type " + type.ToString());
                    ex.Data ["ProviderGuid"] = evt.ProviderId;
                    throw ex;
            }
        }

        static object FormatValue(EtwPropertyInfo propertyInfo, object value)
        {
            if (propertyInfo.ValueMap != null)
            {
                uint key = Convert.ToUInt32(value);
                if (!propertyInfo.ValueMap.TryGetValue(key, out string name))
                {
                    name = value.ToString();
                }

                return name;
            }

            switch (propertyInfo.OutType)
            {
                case TdhOutType.SocketAddress:
                    return FormatAsIpAddress(value);

                case TdhOutType.HexInt8:
                    return "0x" + ((byte)value).ToString("X");

                case TdhOutType.HexInt16:
                    return "0x" + ((ushort)value).ToString("X");

                case TdhOutType.HexInt32:
                    return "0x" + ((uint)value).ToString("X");

                case TdhOutType.HexInt64:
                    return "0x" + ((ulong)value).ToString("X");

                default:
                    return value;
            }
        }

        static string FormatAsIpAddress(object value)
        {
            byte[] buffer = (byte[])value;

            if (buffer.Length == 0)
            {
                return IPAddress.None.ToString();
            }

            if (buffer.Length == 0x10)
            {
                // Interpret the data as IPv4 address:port, represented as sockaddr_in structure
                int port = buffer[2] << 8 | buffer[3];

                byte[] addr = new byte[4];
                Array.Copy(buffer, 4, addr, 0, 4);
                var ipv4 = new IPAddress(addr);

                string s = ipv4.ToString().ToUpper() + ":" + port.ToString();
                return s;
            }
            else
            {
                // Interpret the data as IPv4 address:port, represented as sockaddr_in6 structure
                int port = buffer[2] << 8 | buffer[3];

                byte[] addr = new byte[16];
                Array.Copy(buffer, 8, addr, 0, 16);
                var ipv6 = new IPAddress(addr);

                string s = ipv6.ToString().ToUpper() + ":" + port.ToString();
                return s;
            }
        }

        string TranslateFormatString(string messageFormat)
        {
            if (messageFormat == null)
            {
                return null;
            }

            string format = messageFormat.Replace("%n", "\n");
            format = format.Replace("%t", "    ");

            StringBuilder sb = new StringBuilder();
            int startIndex = 0;
            while (startIndex < format.Length - 1)
            {
                int percentIndex = format.IndexOf('%', startIndex); // no more arguments

                SkipEscapedPercent:

                if (percentIndex < 0)
                {
                    string last = format.Substring(startIndex);
                    sb.Append(last);
                    break;
                }
                if (format[percentIndex + 1] == '%') // special case %% means % escaped
                {
                    percentIndex = format.IndexOf('%', percentIndex + 2);
                    goto SkipEscapedPercent;
                }

                string prefix = format.Substring(startIndex, percentIndex - startIndex);
                sb.Append(prefix);

                int beginNumberIndex = percentIndex + 1;
                int endNumberIndex = beginNumberIndex;
                while (endNumberIndex < format.Length)
                {
                    if (format[endNumberIndex] < '0' || format[endNumberIndex] > '9')
                    {
                        break;
                    }

                    endNumberIndex++;
                }

                string s = format.Substring(beginNumberIndex, endNumberIndex - beginNumberIndex);
                int index = int.Parse(s) - 1; // The C# convention is to start from 0 and teh % notation in the manifests starts from 1
                sb.Append('{');
                sb.Append(index);
                sb.Append('}');

                startIndex = endNumberIndex;
            }

            return sb.ToString();
        }

        class EtwPropertyInfo
        {
            public string Name;
            public TdhInType Type;
            public TdhOutType OutType;
            public ushort Length; // used when the length is explicit
            public string LengthPropertyName; // used when the length is specified as previous property value
            public Dictionary<uint, string> ValueMap;

            public override string ToString()
            {
                if (Type == TdhInType.Binary)
                {
                    return Type + "[" + Length + "] " + Name;
                }
                else
                {
                    return Type + " " + Name;
                }
            }
        }
    }
}