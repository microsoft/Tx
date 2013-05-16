<Query Kind="Expression">
  <Connection>
    <ID>68b1608c-1854-48ff-ae3b-f0238e61f537</ID>
    <Persist>true</Persist>
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

(from e in playback.GetObservable<SystemEvent>()
select e.Header).Take(5)
