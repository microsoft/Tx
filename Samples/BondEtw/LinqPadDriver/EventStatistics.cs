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
    }
}