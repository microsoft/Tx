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

playback.KnownTypes = typeof(Parse).Assembly.GetTypes();

var all = playback.GetObservable<SystemEvent>();
var statistics = from e in all
				 group e by new { e.Header.EventId, TypeName = e.GetType().Name, e.Header.Version }
					 into g
					 from c in g.Count()
					 select new
					 {
					 	 g.Key.EventId,
						 g.Key.TypeName,
						 g.Key.Version,
						 Count = c,
					 };

// The output is IEnumerable, so from this point on we can use Linq-to-Objects
var ordered = from e in playback.BufferOutput(statistics)
			  orderby e.EventId, e.Version
			  select e;

playback.Run();

ordered.Dump();