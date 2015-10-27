namespace Tx.Bond.Extensions
{
    /// <summary>
    /// Stats class that defines the properties of the events.
    /// </summary>
    public class Stats
    {
        public long EventCount { get; set; }
        public long ByteSize { get; set; }
        public string ManifestId { get; set; }
        public long MinTime { get; set; }
        public long MaxTime { get; set; }
    }
}