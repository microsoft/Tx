<Query Kind="Statements">
  <Connection>
    <ID>bceb0ed6-52bc-45f7-b629-a9ea8ae98bbb</ID>
    <Driver Assembly="TxLinqPadDriver" PublicKeyToken="3d3a4b0768c9178e">TxLinqPadDriver.TxDataContextDriver</Driver>
    <DriverData>
      <ContextName>http</ContextName>
      <Files>c:\TxSamples\LINQPad\Traces\HTTP_Server.etl;</Files>
      <MetadataFiles>c:\TxSamples\LINQPad\Manifests\HTTP_Server.man;</MetadataFiles>
      <IsRealTime>false</IsRealTime>
      <IsUsingDirectoryLookup>false</IsUsingDirectoryLookup>
    </DriverData>
  </Connection>
  <Namespace>Microsoft.Etw.Microsoft_Windows_HttpService</Namespace>
</Query>

var begin = playback.GetObservable<Parse>();
var end = playback.GetObservable<FastSend>();

var requests = from b in begin 
			   from e in end.Where(e=>e.Header.ActivityId == b.Header.ActivityId).Take(1)
			   select new
			   {
					b.Header.ActivityId,
					b.Url,
					e.HttpStatus,
					Duration = e.Header.Timestamp - b.Header.Timestamp
				};
			
var statistics = from r in requests
				group r by new
				{
					Milliseconds = Math.Ceiling(r.Duration.TotalMilliseconds * 10) / 10,
					Url = r.Url
				} into groups
				from c in groups.Count()
				select new
				{
					groups.Key.Url,
					groups.Key.Milliseconds,
					Count = c
				};
				
var ordered = from s in playback.BufferOutput(statistics)
			  orderby s.Milliseconds, s.Url
			  select s;

playback.Run(); // this does the sequence-compute, and fills up the above collection

ordered.Dump();