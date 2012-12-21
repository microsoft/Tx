using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reactive;
using System.Linq.Expressions;
using System.Reflection;

namespace UlsLogs
{
    class UlsTypeMap : IPartitionableTypeMap<UlsRecord, string>
    {
        Dictionary<Type, Regex> _expressions = new Dictionary<Type, Regex>();

        public IEqualityComparer<string> Comparer
        {
            get { return StringComparer.Ordinal; }
        }

        public string GetInputKey(UlsRecord evt)
        {
            return evt.EventId;
        }

        public string GetTypeKey(Type outpuType)
        {
            var eventAttribute = outpuType.GetAttribute<UlsEventAttribute>();
            if (eventAttribute == null)
                return null;

            return eventAttribute.EventId;
        }

        public Func<UlsRecord, DateTimeOffset> TimeFunction
        {
            get { return evt => evt.Time; }
        }

        public Func<UlsRecord, object> GetTransform(Type outputType)
        {
            var eventAttribute = outputType.GetAttribute<UlsEventAttribute>();
            Regex parse = new Regex(eventAttribute.RegEx);
            _expressions.Add(outputType, parse);

            //Func<UlsRecord, object> example = e =>
            //    Transform(e, m=>
            //        new LeavingMonitoredScope
            //        {
            //            Scope = m.Groups[1].Value,
            //            ExecutionTime = double.Parse(m.Groups[2].Value)
            //        });

            //Expression<Func<UlsRecord, object>> example = e =>
            //    Transform(e, m =>
            //        new EnteringMonitoredScope
            //        {
            //            Scope = m.Groups[1].Value,
            //        });

            var inputEvent = Expression.Parameter(typeof(UlsRecord), "e");
            var match = Expression.Parameter(typeof(Match), "m");
            List<MemberBinding> bindings = new List<MemberBinding>();
            int index = 1;
            foreach (FieldInfo field in outputType.GetFields())
            {
                var value = Expression.Property(
                                Expression.Call(
                                        Expression.Property(match, typeof(Match).GetProperty("Groups")),
                                            typeof(GroupCollection).GetMethod("get_Item", new Type[]{ typeof(int) }),
                                            Expression.Constant(index++)),
                                    "Value");

                if (field.FieldType == typeof(string))
                {
                    var b = Expression.Bind(field, value);
                    bindings.Add(b);
                }
                else
                {
                    var p = Expression.Call(field.FieldType.GetMethod("Parse", new Type[]{ typeof(string)}), value);
                    bindings.Add(Expression.Bind(field, p));
                }
            }

            var n = Expression.New(outputType);
            var m = Expression.MemberInit(n, bindings.ToArray());
            var lambda2 = Expression.Lambda(m,match);

            var transform = this.GetType().GetMethod("Transform", BindingFlags.NonPublic | BindingFlags.Instance);
            var trOutput = transform.MakeGenericMethod(outputType);
            var call = Expression.Call(Expression.Constant(this), trOutput, inputEvent, lambda2);
            var exp = Expression.Lambda<Func<UlsRecord, object>>(call, inputEvent);
            return exp.Compile();
        }

        object Transform<TOutput>(UlsRecord input, Func<Match, TOutput> create)
        {
            Regex r = _expressions[typeof(TOutput)];
            Match m = r.Match(input.Message);

            TOutput o = create(m);
            return o;
        }
    }
}
