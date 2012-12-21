using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reactive;
using System.Reflection;
using Microsoft.SqlServer.XEvent;
using Microsoft.SqlServer.XEvent.Linq;

namespace Tx.SqlServer
{
    class XeTypeMap : IPartitionableTypeMap<PublishedEvent, Guid>
    {
        public IEqualityComparer<Guid> Comparer
        {
            get { return new GuidComparer(); }
        }

        public Guid GetInputKey(PublishedEvent evt)
        {
            return evt.UUID;
        }

        public Guid GetTypeKey(Type outpuType)
        {
            var eventAttribute = outpuType.GetAttribute<XEventAttribute>();
            if (eventAttribute == null)
                return Guid.Empty;

            return (Guid)eventAttribute.EventGuid;
        }

        public Func<PublishedEvent, DateTimeOffset> TimeFunction
        {
            get { return evt => evt.Timestamp; }
        }

        public Func<PublishedEvent, object> GetTransform(Type outpuType)
        {
            ConstructorInfo constrtorInfo = outpuType.GetConstructor(new Type[] { });
            if (constrtorInfo == null)
                throw new Exception("Type " + outpuType.FullName + " does not implement public constructor with no arguments.");
            
            var inputEvent = Expression.Parameter(typeof(PublishedEvent), "e");
            List<MemberBinding> bindings = new List<MemberBinding>();

            int index = 0;
            List<Expression> list = new List<Expression>();
            foreach (FieldInfo field in outpuType.GetFields())
            {
                Expression readExpression = null;
                if (field.GetAttribute<NonPublishedAttribute>() != null)
                    continue;

                var propertyValue = Expression.Property(
                                            Expression.Call(
                                                Expression.Property(inputEvent, typeof(PublishedEvent).GetProperty("Fields")),
                                                typeof(PublishedEvent.FieldList).GetMethod("get_Item", new Type[] { typeof(int) }),
                                                Expression.Constant(index++)),
                                            "Value");

                if (field.FieldType.IsSubclassOf(typeof(Enum)))
                {
                    readExpression = Expression.Convert(
                                Expression.Property(
                                        Expression.Convert(propertyValue, typeof(MapValue)),
                                        typeof(MapValue).GetProperty("Key")),
                                field.FieldType);
                }
                else
                {
                    readExpression = Expression.Convert(propertyValue, field.FieldType);
                }

                bindings.Add(Expression.Bind(field, readExpression));
            }

            var n = Expression.New(constrtorInfo);
            var m = Expression.MemberInit(n, bindings.ToArray());
            var cast = Expression.Convert(m, typeof(object));
            var exp = Expression.Lambda<Func<PublishedEvent, object>>(cast, inputEvent);
            return exp.Compile();
        }

        static T GetNext<T>(IEnumerator<PublishedEventField> enumerator)
        {
            if (!enumerator.MoveNext())
                throw new Exception("unexpected end of enumeration");

            return (T)enumerator.Current.Value;
        }

        class GuidComparer : IEqualityComparer<Guid>
        {
            public bool Equals(Guid x, Guid y)
            {
                return x.Equals(y);
            }

            public int GetHashCode(Guid obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
