// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reactive;
using System.Reflection;

namespace Tx.Windows
{
    class PerfCounterPartitionTypeMap : PerfCounterTypeMap, IPartitionableTypeMap<PerformanceSample, PerfCounterPartitionKey>
    {
        PerfCounterPartitionKey.Comparer _comparer = new PerfCounterPartitionKey.Comparer();
        PerfCounterPartitionKey _key = new PerfCounterPartitionKey("", "");

        public IEqualityComparer<PerfCounterPartitionKey> Comparer
        {
            get { return _comparer; }
        }

        public PerfCounterPartitionKey GetTypeKey(Type outputType)
        {
            var attribute = outputType.GetAttribute<PerformanceCounterAttribute>();
            if (attribute == null)
                return null;

            return new PerfCounterPartitionKey(attribute.CounterSet, attribute.CounterName);
        }

        public PerfCounterPartitionKey GetInputKey(PerformanceSample evt)
        {
            return new PerfCounterPartitionKey(evt.CounterSet, evt.CounterName);
        }
    }

    class PerfCounterTypeMap : ITypeMap<PerformanceSample>
    {
        public Func<PerformanceSample, DateTimeOffset> TimeFunction
        {
            get { return e => e.Timestamp; }
        }

        public Func<PerformanceSample, object> GetTransform(Type outputType)
        {
            Expression<Func<PerformanceSample, PerformanceSample>> template = e =>
                new PerformanceSample(e);

            if (outputType == typeof(PerformanceSample))
                return template.Compile();

            LambdaExpression ex = (LambdaExpression)template;
            ConstructorInfo constructor = outputType.GetConstructor(new Type[] { typeof(PerformanceSample) });
            var n = Expression.New(constructor, ex.Parameters);
            var cast = Expression.Convert(n, typeof(object));
            var exp = Expression.Lambda<Func<PerformanceSample, object>>(cast, ex.Parameters);
            return exp.Compile();
        }
    }

    public class PerfCounterPartitionKey
    {
        string _counterSet;
        string _counterName;
        int _hashCode;

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
