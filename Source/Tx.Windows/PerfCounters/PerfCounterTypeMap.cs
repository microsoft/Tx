// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reactive;
using System.Reflection;

namespace Tx.Windows
{
    internal class PerfCounterPartitionTypeMap : PerfCounterTypeMap,
                                                 IPartitionableTypeMap<PerformanceSample, PerfCounterPartitionKey>
    {
        private readonly PerfCounterPartitionKey.Comparer _comparer = new PerfCounterPartitionKey.Comparer();

        public IEqualityComparer<PerfCounterPartitionKey> Comparer
        {
            get { return _comparer; }
        }

        public PerfCounterPartitionKey GetTypeKey(Type outputType)
        {
            var attribute = outputType.GetTypeInfo().GetCustomAttribute<PerformanceCounterAttribute>();
            if (attribute == null)
                return null;

            return new PerfCounterPartitionKey(attribute.CounterSet, attribute.CounterName);
        }

        public PerfCounterPartitionKey GetInputKey(PerformanceSample evt)
        {
            return new PerfCounterPartitionKey(evt.CounterSet, evt.CounterName);
        }
    }

    internal class PerfCounterTypeMap : ITypeMap<PerformanceSample>
    {
        public Func<PerformanceSample, DateTimeOffset> TimeFunction
        {
            get { return e => e.Timestamp; }
        }

        public Func<PerformanceSample, object> GetTransform(Type outputType)
        {
            Expression<Func<PerformanceSample, PerformanceSample>> template = e =>
                                                                              new PerformanceSample(e);

            if (outputType == typeof (PerformanceSample))
                return template.Compile();

            LambdaExpression ex = template;
            ConstructorInfo constructor = outputType.GetConstructor(new[] {typeof (PerformanceSample)});
            Debug.Assert(constructor != null, "constructor != null");
            NewExpression n = Expression.New(constructor, ex.Parameters);
            UnaryExpression cast = Expression.Convert(n, typeof (object));
            Expression<Func<PerformanceSample, object>> exp = Expression.Lambda<Func<PerformanceSample, object>>(cast,
                                                                                                                 ex
                                                                                                                     .Parameters);
            return exp.Compile();
        }
    }

    public class PerfCounterPartitionKey
    {
        private readonly string _counterName;
        private readonly string _counterSet;
        private readonly int _hashCode;

        public PerfCounterPartitionKey(string counterSet, string counterName)
        {
            _counterSet = counterSet;
            _counterName = counterName;
            _hashCode = (_counterSet + '\\' + _counterName).GetHashCode();
        }

        public class Comparer : IEqualityComparer<PerfCounterPartitionKey>
        {
            public bool Equals(PerfCounterPartitionKey x, PerfCounterPartitionKey y)
            {
                return (x._counterSet == y._counterSet) &&
                       (x._counterName == y._counterName);
            }

            public int GetHashCode(PerfCounterPartitionKey obj)
            {
                return obj._hashCode;
            }
        }
    }
}