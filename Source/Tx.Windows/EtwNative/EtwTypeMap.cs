using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reactive;
using System.Reflection;

namespace Tx.Windows
{
    public class EtwTypeMap : IRootTypeMap<EtwNativeEvent, SystemEvent>
    {
        public Func<EtwNativeEvent, DateTimeOffset> TimeFunction
        {
            get { return e => e.TimeStamp; }
        }

        public Func<EtwNativeEvent, object> GetTransform(Type outputType)
        {
            Expression<Func<EtwNativeEvent, SystemEvent>> template = e =>
                new SystemEvent
                {
                    Header = new SystemHeader
                    {
                        Timestamp = e.TimeStamp.DateTime,
                        ActivityId = e.ActivityId,
                        RelatedActivityId = GetRelatedActivityId(e.ExtendedDataCount, e.ExtendedData, e.Flags),
                        ProviderId = e.ProviderId,
                        EventId = e.Id,
                        Opcode = e.Opcode,
                        Version = e.Version,
                        ProcessId = e.ProcessId,
                        ThreadId = e.ThreadId,
                        Level = e.Level,
                        Channel = e.Channel,
                        Task = e.Task,
                        Keywords = e.Keyword,
                    }
                };

            if (outputType == typeof(SystemEvent))
                return template.Compile();

            LambdaExpression ex = (LambdaExpression)template;
            MemberInitExpression mi = (MemberInitExpression)ex.Body;
            List<MemberBinding> bindings = new List<MemberBinding>(mi.Bindings);
            ParameterExpression reader = ex.Parameters[0];

            PropertyInfo[] properties = outputType.GetProperties();
            foreach (PropertyInfo p in properties)
            {
                EventFieldAttribute attribute = p.GetAttribute<EventFieldAttribute>();
                if (attribute == null) continue;

                Expression readExpression = null;


                switch (attribute.OriginalType)
                {
                    case "win:Boolean":
                        readExpression = MakeExpression(r => r.ReadBoolean(), reader);
                        break;

                    case "win:Pointer":
                        readExpression = MakeExpression(r => r.ReadPointer(), reader);
                        break;

                    case "win:Int8":
                        readExpression = MakeExpression(r => r.ReadInt8(), reader);
                        break;

                    case "win:UInt8":
                        readExpression = MakeExpression(r => r.ReadUInt8(), reader);
                        break;

                    case "win:Int16":
                        readExpression = MakeExpression(r => r.ReadInt16(), reader);
                        break;

                    case "win:UInt16":
                        readExpression = MakeExpression(r => r.ReadUInt16(), reader);
                        break;

                    case "win:Int32":
                        readExpression = MakeExpression(r => r.ReadInt32(), reader);
                        break;

                    case "win:HexInt32":
                    case "win:UInt32":
                        readExpression = MakeExpression(r => r.ReadUInt32(), reader);
                        break;

                    case "win:Int64":
                        readExpression = MakeExpression(r => r.ReadInt64(), reader);
                        break;

                    case "win:HexInt64":
                    case "win:UInt64":
                        readExpression = MakeExpression(r => r.ReadUInt64(), reader);
                        break;

                    case "win:Double":
                        readExpression = MakeExpression(r => r.ReadDouble(), reader);
                        break;

                    case "win:Float":
                        readExpression = MakeExpression(r => r.ReadFloat(), reader);
                        break;

                    case "win:UnicodeString":
                        int len = 0;
                        if (int.TryParse(attribute.Length, out len))
                        {
                            readExpression = MakeExpression(r => r.ReadUnicodeString(len), reader);
                        }
                        else
                        {
                            readExpression = MakeExpression(r => r.ReadUnicodeString(), reader);
                        }
                        break;

                    case "win:AnsiString":
                        readExpression = MakeExpression(r => r.ReadAnsiString(), reader);
                        break;

                    case "win:FILETIME":
                        readExpression = MakeExpression(r => r.ReadFileTime(), reader);
                        break;

                    case "win:SYSTEMTIME":
                        readExpression = MakeExpression(r => r.ReadSystemTime(), reader);
                        break;

                    case "win:SID":
                        readExpression = MakeExpression(r => r.ReadSid(), reader);
                        break;
                                       
                    case "win:Binary":
                        // HACK: this is not handling the length
                        readExpression = MakeExpression(r => r.ReadBytes(), reader);
                        break;

                    case "win:GUID":
                        readExpression = MakeExpression(r => r.ReadGuid(), reader);
                        break;

                    default:
                        throw new NotImplementedException("Unknown primitive type " + attribute.OriginalType);
                }
                MemberBinding b = Expression.Bind(p, readExpression);
                bindings.Add(b);
            }

            var n = Expression.New(outputType);
            var m = Expression.MemberInit(n, bindings.ToArray());
            var cast = Expression.Convert(m, typeof(object));
            var exp = Expression.Lambda<Func<EtwNativeEvent, object>>(cast, ex.Parameters);
            return exp.Compile();
        }

        static Expression MakeExpression<TProperty>(
            Expression<Func<EtwNativeEvent, TProperty>> expression,
            ParameterExpression readerParameter)
        {
            MethodCallExpression call = (MethodCallExpression)expression.Body;
            MethodCallExpression callFixed = Expression.Call(readerParameter, call.Method, call.Arguments);
            return callFixed;
        }

        static Guid GetRelatedActivityId(UInt16 extendedDataCount, IntPtr extendedData, UInt16 flags)
        {
            Guid relatedActivityId = Guid.Empty;
            for (int ext = 0; ext < extendedDataCount; ext++)
            {
                unsafe
                {
                    EventHeaderExtendedDataItem extendedDataItem = *((EventHeaderExtendedDataItem*)extendedData.ToPointer());
                    if (extendedDataItem.ExtType != EventHeaderExtType.RelatedActivityId) 
                        continue;


                    byte[] value = new byte[16];
                    fixed (byte* pb = value)
                    {
                        TypeServiceUtil.MemCopy((byte*)extendedDataItem.DataPtr.ToPointer(), pb, 16);
                    }
                    relatedActivityId = new Guid(value);
                    break;
                }
            }

            return relatedActivityId;
        }
    }
}
