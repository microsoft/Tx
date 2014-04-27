<Query Kind="Statements">
  <Connection>
    <ID>88a04bb7-535c-43a6-99c6-385c238126df</ID>
    <Driver Assembly="Tx.LinqPad" PublicKeyToken="3d3a4b0768c9178e">Tx.LinqPad.TxDataContextDriver</Driver>
    <DriverData>
      <ContextName>http</ContextName>
      <Files>($SampleTraces)HTTP_Server.etl;</Files>
      <MetadataFiles>($SampleTraces)HTTP_Server.man;</MetadataFiles>
      <IsRealTime>false</IsRealTime>
      <IsUsingDirectoryLookup>false</IsUsingDirectoryLookup>
    </DriverData>
  </Connection>
  <Namespace>Tx.Windows.Microsoft_Windows_HttpService</Namespace>
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

// up to here it was all Rx query, now knowing the result is small we can buffer it
// as IEnumerable collection and use LINQ to Objects to sort it 
				
var ordered = from s in playback.BufferOutput(statistics)
			  orderby s.Milliseconds, s.Url
			  select s;

playback.Run(); // Run is explicit way to start the processing

ordered.Dump();