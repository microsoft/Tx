<Query Kind="Expression">
  <Connection>
    <ID>7e173971-53a4-4a45-b6e9-128a2518ec8e</ID>
    <Persist>true</Persist>
    <Driver Assembly="TxLinqPadDriver" PublicKeyToken="3d3a4b0768c9178e">TxLinqPadDriver.TxDataContextDriver</Driver>
    <DriverData>
      <ContextName>http evtx</ContextName>
      <Files>c:\TxSamples\LINQPad\Traces\HTTP_Server.evtx;</Files>
      <MetadataFiles>c:\TxSamples\LINQPad\Manifests\HTTP_Server.man;</MetadataFiles>
      <IsRealTime>false</IsRealTime>
      <IsUsingDirectoryLookup>false</IsUsingDirectoryLookup>
    </DriverData>
  </Connection>
  <Namespace>Microsoft.Etw.Microsoft_Windows_HttpService</Namespace>
</Query>

from fr in playback.GetObservable<FastResp>()
select new
{
	fr.Header.ActivityId,
	fr.StatusCode
}