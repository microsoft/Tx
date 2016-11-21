namespace Tx.Bond
{
    [global::Bond.Schema]
    internal partial class BondEnvelope
    {
        [global::Bond.Id(1)]
        public long ReceivedFileTimeUtc { get; set; }
        [global::Bond.Id(2)]
        public long OccurrenceFileTimeUtc { get; set; }
        [global::Bond.Id(3)]
        public string TypeId { get; set; }
        [global::Bond.Id(4)]
        public string Source { get; set; }
        [global::Bond.Id(5)]
        public string Protocol { get; set; }
        [global::Bond.Id(6)]
        public byte[] Payload { get; set; }
    }
}
