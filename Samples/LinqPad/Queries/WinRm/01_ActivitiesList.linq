<Query Kind="Statements">
  <Connection>
    <ID>ceda4c03-6947-41d1-a094-c8f70c6efa7f</ID>
    <Driver Assembly="TxLinqPadDriver" PublicKeyToken="3d3a4b0768c9178e">TxLinqPadDriver.TxDataContextDriver</Driver>
    <DriverData>
      <ContextName>WinRm</ContextName>
      <Files>C:\TxSamples\LinqPad\Traces\WsRm01.etl;</Files>
      <MetadataFiles>C:\TxSamples\LinqPad\Manifests\Web-Services-for-Management-Core.man;</MetadataFiles>
      <IsRealTime>false</IsRealTime>
      <IsUsingDirectoryLookup>false</IsUsingDirectoryLookup>
    </DriverData>
  </Connection>
</Query>

var statistics = from e in playback.GetObservable<SystemEvent>()
				 group e by e.Header.ActivityId
					 into g
					 from c in g.Count()
					 select new
					 {
						 ActivityId = g.Key,
						 Count = c,
					 };

// The output is IEnumerable, so from this point on we can use Linq-to-Objects
var ordered = from e in playback.BufferOutput(statistics)
			  orderby e.Count descending
			  select e;

playback.Run();

ordered.Dump();