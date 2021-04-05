// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Tx.Windows
{
    public sealed class TimeUtil
    {
        // DateTimeKind option needed for overriding DateTimeKind if parsing 
        // performance counter blg from different timezone than creation time zone
        public static DateTimeKind DateTimeKind {get; set;} = DateTimeKind.Local;
        private const Int64 TicksPerMillisecond = 10000;
        private const Int64 TicksPerSecond = TicksPerMillisecond * 1000;
        private const Int64 TicksPerMinute = TicksPerSecond * 60;
        private const Int64 TicksPerHour = TicksPerMinute * 60;
        private const Int64 TicksPerDay = TicksPerHour * 24;

        // Number of days in a non-leap year
        private const int DaysPerYear = 365;
        // Number of days in 4 years
        const int DaysPer4Years = DaysPerYear * 4 + 1;
        // Number of days in 100 years
        private const int DaysPer100Years = DaysPer4Years * 25 - 1;
        // Number of days in 400 years
        private const int DaysPer400Years = DaysPer100Years * 4 + 1;

        // Number of days from 1/1/0001 to 12/31/1600
        private const int DaysTo1601 = DaysPer400Years * 4;

        private const Int64 FileTimeOffset = DaysTo1601 * TicksPerDay;

        public static DateTimeOffset DateTimeOffsetFromFileTime(Int64 fileTime)
        {
            return new DateTimeOffset(FileTimeOffset + fileTime, TimeSpan.Zero);
        }

        public static DateTime FromFileTime(Int64 fileTime)
        {
            return new DateTime(FileTimeOffset + fileTime, DateTimeKind);
        }
    }
}