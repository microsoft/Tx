<Query Kind="Expression">
  <Connection>
    <ID>240ff299-fb64-4e88-9780-c07a6308c7ab</ID>
    <Persist>true</Persist>
    <Driver Assembly="TxLinqPadDriver" PublicKeyToken="3d3a4b0768c9178e">TxLinqPadDriver.TxDataContextDriver</Driver>
    <DriverData>
      <ContextName>rdp</ContextName>
      <Files>C:\TxSamples\LinqPad\Traces\rdpperf.etl;</Files>
      <MetadataFiles>C:\TxSamples\LinqPad\Manifests\Microsoft-RDP-PerformanceCounters.man;</MetadataFiles>
      <IsRealTime>false</IsRealTime>
      <IsUsingDirectoryLookup>false</IsUsingDirectoryLookup>
    </DriverData>
  </Connection>
  <Namespace>Microsoft.Etw.Microsoft_RDV_RDP_PerfCounters_Encoder</Namespace>
</Query>

(from e in playback.GetObservable<RDV_RDP_ENCODER_FRAME_ENCODING_END>()
select new 
{
	e.sessionId,
	e.stackId, 
	e.moduleId,
	e.uniqueCounterId,
	e.Value,
}).Take(5)