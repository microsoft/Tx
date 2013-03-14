// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Tx.Windows;

namespace PerformanceBaseline.Code
{
    [PerformanceTestSuite("Cluster Shared Volume", "Code")]
    class CodeCsv : CallbackTestSuite
    {
        long _eventCount;
        public long _expectedCount;
        Dictionary<EventType, EventStatistics> _baselineStatistics = new Dictionary<EventType, EventStatistics>();

        public CodeCsv()
            : base(@"CSV.etl")
        {
        }

        [PerformanceTestCase("EventCount")]
        public void EventCount()
        {
            EtwCallback = EventCountCallback;
            _expectedCount = 30027;
        }

        public void EventCountCallback(EtwNativeEvent evt)
        {
            _eventCount++;
        }

        [PerformanceTestCase("Event Type Statistics")]
        public void EventTypeStatistics()
        {
            EtwCallback = EventTypeStatisticsCallback;
        }

        public void EventTypeStatisticsCallback(EtwNativeEvent evt)
        {
            EventType evtType = new EventType(evt);

            EventStatistics statistics;
            if (_baselineStatistics.TryGetValue(evtType, out statistics))
            {
                statistics.count++;
            }
            else
            {
                statistics = new EventStatistics
                {
                    type = evtType,
                    count = 1,
                };

                _baselineStatistics.Add(statistics.type, statistics);
            };
        }

        struct EventType : IComparable<EventType>, IEquatable<EventType>
        {
            public readonly Guid ProviderId;
            public readonly ushort EventId;
            public readonly byte Opcode;
            public readonly byte Version;

            internal EventType(EtwNativeEvent evt)
            {
                ProviderId = evt.ProviderId;
                EventId = evt.Id;
                Opcode = evt.Opcode;
                Version = evt.Version;
            }

            public int CompareTo(EventType other)
            {
                int p = ProviderId.CompareTo(other.ProviderId);
                if (p != 0)
                    return p;

                int e = EventId.CompareTo(other.EventId);
                if (e != 0)
                    return e;

                int o = Opcode.CompareTo(other.Opcode);
                if (o != 0)
                    return o;

                return Version.CompareTo(other.Version);
            }

            public bool Equals(EventType other)
            {
                return CompareTo(other) == 0;
            }

            public override int GetHashCode()
            {
                return (int)EventId;
            }

            public override bool Equals(object obj)
            {
                return obj is EventType && Equals((EventType)obj);
            }
        }

        class EventStatistics
        {
            public EventType type;
            public long count;
        }

    }
}
