using System;
namespace Tx.Bond.Extensions
{
    /// <summary>
    /// Stats class that defines the properties of the events.
    /// </summary>
    public sealed class EventStatistics
    {
        /// <summary>
        /// Gets or sets the event count.
        /// </summary>
        public long EventCount { get; set; }

        /// <summary>
        /// Gets or sets the byte size.
        /// </summary>
        public long ByteSize { get; set; }

        /// <summary>
        /// Gets or sets the events per second.
        /// </summary>
        public double EventsPerSecond { get; set; }

        /// <summary>
        /// Gets or sets the average byte size.
        /// </summary>
        public double AverageByteSize { get; set; }

        public static EventStatistics operator + (EventStatistics x, EventStatistics y)
        {
            if (x == null || y == null)
            {
                throw new ArgumentNullException("x and/or y");
            }

            return new EventStatistics
            {
                // To avoid inaccuracy, don't calculating average of averages.
                AverageByteSize = (x.ByteSize + y.ByteSize) / (x.EventCount + y.EventCount),

                ByteSize = x.ByteSize + y.ByteSize,
                EventCount = x.EventCount + y.EventCount,

                // change this if G and S, agree to add last/first event timestamps.
                EventsPerSecond = (x.EventsPerSecond + y.EventsPerSecond) / 2,
            };
        }
    }
}