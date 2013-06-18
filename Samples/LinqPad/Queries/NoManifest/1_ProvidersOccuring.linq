<Query Kind="Statements">
  <Connection>
    <ID>68b1608c-1854-48ff-ae3b-f0238e61f537</ID>
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

// Let's see what providers are we dealing with

var statObservable = from e in playback.GetObservable<SystemEvent>() // this is asking for all events
group e by e.Header.ProviderId into g
   from c in g.Count() 
   select new {
       ProviderId = g.Key, 
       Count = c,
   };
   
var statEnumerable = playback.BufferOutput(statObservable);  // subscribe a list to the Rx query defined so far
playback.Run(); 											 // after the file is read, the above enumerable has the result

var sorted = from s in statEnumerable						 // this is LINQ to Objects
			 orderby s.ProviderId
			 select s;
			 
sorted.Dump();