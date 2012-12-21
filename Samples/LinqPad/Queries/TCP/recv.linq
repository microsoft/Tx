<Query Kind="Expression">
  <Connection>
    <ID>06a460fa-6aa5-4796-a541-aaefa1bbc618</ID>
    <Persist>true</Persist>
    <Driver Assembly="TxLinqPadDriver" PublicKeyToken="3d3a4b0768c9178e">TxLinqPadDriver.TxDataContextDriver</Driver>
    <DriverData>
      <ContextName>tcp</ContextName>
      <Files>c:\TxSamples\LINQPad\Traces\KernelNetwork.etl;</Files>
      <MetadataFiles>c:\TxSamples\LINQPad\Manifests\SystemEvents.man;</MetadataFiles>
      <IsRealTime>false</IsRealTime>
      <IsUsingDirectoryLookup>false</IsUsingDirectoryLookup>
    </DriverData>
  </Connection>
  <Namespace>Microsoft.Etw.Microsoft_Windows_Kernel_Network</Namespace>
</Query>

from recv in playback.GetObservable<KNetEvt_RecvIPV4>()
select new
{
	recv.daddr,
	recv.dport,
	recv.size
}
