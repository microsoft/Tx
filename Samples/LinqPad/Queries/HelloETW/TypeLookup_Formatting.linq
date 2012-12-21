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

var fmt = from e in playback.GetObservable<SystemEvent>()
select new
{
	e.Header.Timestamp,
	e.Header.RelatedActivityId,
	EventType = e.GetType().Name,
	Message = e.ToString()
};

fmt.Dump();