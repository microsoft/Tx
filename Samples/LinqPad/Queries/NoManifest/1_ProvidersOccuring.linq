<Query Kind="Statements">
  <Connection>
    <ID>56b28019-287b-4d3a-85c8-5ccaccf4fb1e</ID>
    <Persist>true</Persist>
    <Driver Assembly="Tx.LinqPad" PublicKeyToken="3d3a4b0768c9178e">Tx.LinqPad.TxDataContextDriver</Driver>
    <DriverData>
      <ContextName>docdbserver1</ContextName>
      <Files>C:\Repro\DocDB\Test (full run)\docdbserver1.etl;</Files>
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