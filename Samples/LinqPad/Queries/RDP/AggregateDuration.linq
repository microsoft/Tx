<Query Kind="Statements">
  <Connection>
    <ID>240ff299-fb64-4e88-9780-c07a6308c7ab</ID>
    <Driver Assembly="TxLinqPadDriver" PublicKeyToken="3d3a4b0768c9178e">TxLinqPadDriver.TxDataContextDriver</Driver>
    <DriverData>
      <ContextName>rdp</ContextName>
      <Files>C:\TxSamples\LinqPad\Traces\rdpperf.etl;</Files>
      <MetadataFiles>C:\TxSamples\LinqPad\Manifests\Microsoft-RDP-PerformanceCounters.man;</MetadataFiles>
      <IsRealTime>false</IsRealTime>
      <IsUsingDirectoryLookup>false</IsUsingDirectoryLookup>
    </DriverData>
  </Connection>
  <Namespace>Microsoft.Etw.Microsoft_RDV_RDP_PerfCounters_Encoder</Namespace>
</Query>

var start = playback.GetObservable<RDV_RDP_ENCODER_FRAME_ENCODING_START>();
var end = playback.GetObservable<RDV_RDP_ENCODER_FRAME_ENCODING_END>();

var encoding = from s in start
			   from e in end.Where(e => e.Value == s.Value).Take(1)
			   select new
			   {
			   	    s.OccurenceTime,
					s.Value, 
					Duration = e.OccurenceTime - s.OccurenceTime
				};
				
var statistics = from r in encoding
				group r by new
				{
					Milliseconds = Math.Ceiling(r.Duration.TotalMilliseconds / 10) * 10,
				} into groups
				from c in groups.Count()
				select new
				{
					groups.Key.Milliseconds,
					Count = c
				};
				
var ordered = from s in playback.BufferOutput(statistics)
			  orderby s.Milliseconds descending
			  select s;

playback.Run(); // this does the sequence-compute, and fills up the above collection

ordered.Dump();