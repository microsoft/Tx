using System;

namespace Tx.Windows
{
    public class TimeUtil
    {
        const Int64 TicksPerMillisecond = 10000;
        const Int64 TicksPerSecond = TicksPerMillisecond * 1000;
        const Int64 TicksPerMinute = TicksPerSecond * 60;
        const Int64 TicksPerHour = TicksPerMinute * 60;
        const Int64 TicksPerDay = TicksPerHour * 24;

        // Number of days in a non-leap year
        const int DaysPerYear = 365;
        // Number of days in 4 years
        const int DaysPer4Years = DaysPerYear * 4 + 1;
        // Number of days in 100 years
        const int DaysPer100Years = DaysPer4Years * 25 - 1;
        // Number of days in 400 years
        const int DaysPer400Years = DaysPer100Years * 4 + 1;

        // Number of days from 1/1/0001 to 12/31/1600
        const int DaysTo1601 = DaysPer400Years * 4;

        const Int64 FileTimeOffset = DaysTo1601 * TicksPerDay;

        static TimeSpan utcOffset = TimeZoneInfo.Local.BaseUtcOffset;

        public static DateTimeOffset DateTimeOffsetFromFileTime(Int64 fileTime)
        {
            //TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(FromFileTime(fileTime));
            //return new DateTimeOffset(FileTimeOffset + fileTime, utcOffset);
            //return new DateTime(FileTimeOffset + fileTime, DateTimeKind.Utc);
            return new DateTimeOffset(FileTimeOffset + fileTime, TimeSpan.Zero);
        }
                        
        public static DateTime FromFileTime(Int64 fileTime)
        {
            return new DateTime(FileTimeOffset + fileTime, DateTimeKind.Utc);
        }
    }
}
