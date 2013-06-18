<Query Kind="Statements">
  <Connection>
    <ID>68b1608c-1854-48ff-ae3b-f0238e61f537</ID>
    <Persist>true</Persist>
    <Driver Assembly="Tx.LinqPad" PublicKeyToken="3d3a4b0768c9178e">Tx.LinqPad.TxDataContextDriver</Driver>
    <DriverData>
      <ContextName>http_noManifest</ContextName>
      <Files>($SampleTraces)HTTP_Server.etl;</Files>
      <MetadataFiles></MetadataFiles>
      <IsRealTime>false</IsRealTime>
      <IsUsingDirectoryLookup>false</IsUsingDirectoryLookup>
    </DriverData>
  </Connection>
</Query>

// This set of samples illustrates what to do if there is no manifest available
// We can't see the tree on the left, but we can still do event statistics

var statObservable = from e in playback.GetObservable<SystemEvent>()
group e by new { e.Header.ProviderId, e.Header.EventId }
   into g
   from c in g.Count() 
   select new {
       g.Key.ProviderId, 
	   g.Key.EventId, 
       Count = c,
   };
   
var statEnumerable = playback.BufferOutput(statObservable);  // subscribe a list to the Rx query defined so far
playback.Run(); 											 // after the file is read, the above enumerable has the result

var sorted = from s in statEnumerable						 // this is LINQ to Objects
			 orderby s.ProviderId, s.EventId
			 select s;
			 
sorted.Dump(); 