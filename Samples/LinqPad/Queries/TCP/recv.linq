<Query Kind="Expression">
  <Connection>
    <ID>492b0438-a4c3-47cd-ba72-6c20ed8e8465</ID>
    <Persist>true</Persist>
    <Driver Assembly="Tx.LinqPad" PublicKeyToken="3d3a4b0768c9178e">Tx.LinqPad.TxDataContextDriver</Driver>
    <DriverData>
      <ContextName>tcp</ContextName>
      <Files>C:\TxSamples\LinqPad\Traces\KernelNetwork.etl;</Files>
      <MetadataFiles>C:\TxSamples\LinqPad\Manifests\SystemEvents.man;</MetadataFiles>
      <IsRealTime>false</IsRealTime>
      <IsUsingDirectoryLookup>false</IsUsingDirectoryLookup>
    </DriverData>
  </Connection>
  <Namespace>Tx.Windows.Microsoft_Windows_Kernel_Network</Namespace>
</Query>

from recv in playback.GetObservable<KNetEvt_RecvIPV4>()
select new
{
	recv.daddr,
	recv.dport,
	recv.size
}