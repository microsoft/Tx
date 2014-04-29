<Query Kind="Statements">
  <Connection>
    <ID>68b1608c-1854-48ff-ae3b-f0238e61f537</ID>
    <Driver Assembly="Tx.LinqPad" PublicKeyToken="3d3a4b0768c9178e">Tx.LinqPad.TxDataContextDriver</Driver>
    <DriverData>
      <ContextName>http_noManifest</ContextName>
      <Files>C:\demo\HTTP_Server.etl;</Files>
      <MetadataFiles></MetadataFiles>
      <IsRealTime>false</IsRealTime>
      <IsUsingDirectoryLookup>false</IsUsingDirectoryLookup>
    </DriverData>
  </Connection>
</Query>

var raw = EtwObservable.FromFiles(@"c:\git\tx\Traces\HTTP_Server.etl");

var hex = raw.Take(10)
			.Select(e=> new { e.Id, Content = e.ReadBytes(e.UserDataLength).ToHexDump() });
hex.Dump();