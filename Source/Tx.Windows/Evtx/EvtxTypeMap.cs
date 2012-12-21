using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq.Expressions;
using System.Reactive;
using System.Reflection;

namespace Tx.Windows
{
    public class EvtxTypeMap : IRootTypeMap<EventRecord, SystemEvent>
    {
        ManifestEventPartitionKey.Comparer _comparer = new ManifestEventPartitionKey.Comparer();

        public Func<EventRecord, DateTimeOffset> TimeFunction
        {
            get { return evt => evt.TimeCreated.Value.ToUniversalTime(); }
        }

        public Func<EventRecord, object> GetTransform(Type outputType) 
        {
            Expression<Func<EventRecord, SystemEvent>> template = e => new SystemEvent
            {
                Header = new SystemHeader
                {
                    ActivityId = e.ActivityId.HasValue ? e.ActivityId.Value : Guid.Empty,
                    Channel = 0,
                    Context = e.LogName,
                    Level = e.Level.Value,
                    EventId = (ushort)e.Id,
                    Keywords = e.Keywords.HasValue ? (ulong)e.Keywords.Value : (ulong)0,
                    Opcode = e.Opcode.HasValue ? (byte)e.Opcode.Value : (byte)0,
                    ProcessId = (uint)e.ProcessId.Value,
                    ProviderId = e.ProviderId.HasValue ? e.ProviderId.Value : Guid.Empty,
                    RelatedActivityId = e.RelatedActivityId.HasValue ? e.RelatedActivityId.Value : Guid.Empty,
                    Task = e.Task.HasValue ? (ushort)e.Task.Value : (ushort)0,
                    ThreadId = e.ThreadId.HasValue ? (uint)e.ThreadId.Value : (uint)0,
                    Timestamp = e.TimeCreated.HasValue ? e.TimeCreated.Value : DateTime.MinValue,
                    Version = e.Version.HasValue ? e.Version.Value : (byte)0
                }
            };

            if (outputType == typeof(SystemEvent))
                return template.Compile();

            LambdaExpression ex = (LambdaExpression)template;
            MemberInitExpression mi = (MemberInitExpression)ex.Body;
            List<MemberBinding> bindings = new List<MemberBinding>(mi.Bindings);
            ParameterExpression record = ex.Parameters[0];

            PropertyInfo[] properties = outputType.GetProperties();
            int index = 0;
            foreach (PropertyInfo p in properties)
            {
                EventFieldAttribute attribute = p.GetAttribute<EventFieldAttribute>();
                if (attribute == null) continue;

                var b = Expression.Bind(p,
                            Expression.Convert(
                                Expression.Property(
                                        Expression.Call(
                                            Expression.Property(record, typeof(EventRecord).GetProperty("Properties")),
                                            typeof(IList<EventProperty>).GetMethod("get_Item"),
                                            Expression.Constant(index++)),
                                        typeof(EventProperty).GetProperty("Value")),
                                GetSimpleType(attribute.OriginalType)));

                bindings.Add(b);
            }

            var n = Expression.New(outputType);
            var m = Expression.MemberInit(n, bindings.ToArray());
            var cast = Expression.Convert(m, typeof(object));
            var exp = Expression.Lambda<Func<EventRecord, object>>(cast, ex.Parameters);

            return exp.Compile();
        }

        protected Type GetSimpleType(string winType)
        {
            return _typeMap[winType];
        }

        static Dictionary<string, Type> _typeMap = new Dictionary<string, Type>()
        {       { "win:Int8", typeof(sbyte) },
                { "win:UInt8", typeof(byte)},
                { "win:Int16", typeof(short)},
                { "win:UInt16", typeof(ushort)}, 
                { "win:Int32", typeof(int)},
                { "win:UInt32", typeof(uint)},
                { "win:Int64", typeof(long)},
                { "win:UInt64", typeof(ulong)},
                { "win:Pointer", typeof(ulong)},
                { "win:UnicodeString", typeof(string)},
                { "win:AnsiString", typeof(string)}
         };
    }
}
