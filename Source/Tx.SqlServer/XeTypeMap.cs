// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reactive;
using System.Reflection;
using Microsoft.SqlServer.XEvent;
using Microsoft.SqlServer.XEvent.Linq;

namespace Tx.SqlServer
{
    internal class XeTypeMap : IPartitionableTypeMap<PublishedEvent, Guid>
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

            return (Guid) eventAttribute.EventGuid;
        }

        public Func<PublishedEvent, DateTimeOffset> TimeFunction
        {
            get { return evt => evt.Timestamp; }
        }

        public Func<PublishedEvent, object> GetTransform(Type outpuType)
        {
            ConstructorInfo constrtorInfo = outpuType.GetConstructor(new Type[] {});
            if (constrtorInfo == null)
                throw new Exception("Type " + outpuType.FullName +
                                    " does not implement public constructor with no arguments.");

            ParameterExpression inputEvent = Expression.Parameter(typeof (PublishedEvent), "e");
            var bindings = new List<MemberBinding>();

            int index = 0;
            foreach (PropertyInfo property in outpuType.GetProperties())
            {
                Expression readExpression;
                if (property.GetAttribute<NonPublishedAttribute>() != null)
                    continue;

                MemberExpression propertyValue = Expression.Property(
                    Expression.Call(
                        Expression.Property(inputEvent, typeof (PublishedEvent).GetProperty("Fields")),
                        typeof (PublishedEvent.FieldList).GetMethod("get_Item", new[] {typeof (int)}),
                        Expression.Constant(index++)),
                    "Value");

                if (property.PropertyType.IsSubclassOf(typeof (Enum)))
                {
                    readExpression = Expression.Convert(
                        Expression.Property(
                            Expression.Convert(propertyValue, typeof (MapValue)),
                            typeof (MapValue).GetProperty("Key")),
                        property.PropertyType);
                }
                else
                {
                    readExpression = Expression.Convert(propertyValue, property.PropertyType);
                }

                bindings.Add(Expression.Bind(property, readExpression));
            }

            NewExpression n = Expression.New(constrtorInfo);
            MemberInitExpression m = Expression.MemberInit(n, bindings.ToArray());
            UnaryExpression cast = Expression.Convert(m, typeof (object));
            Expression<Func<PublishedEvent, object>> exp = Expression.Lambda<Func<PublishedEvent, object>>(cast,
                                                                                                           inputEvent);
            return exp.Compile();
        }

        private class GuidComparer : IEqualityComparer<Guid>
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