<Query Kind="Expression">
  <Connection>
    <ID>81682494-18d6-444e-bb8f-2538ed87da22</ID>
    <Persist>true</Persist>
    <Driver Assembly="Tx.LinqPad" PublicKeyToken="3d3a4b0768c9178e">Tx.LinqPad.TxDataContextDriver</Driver>
    <DriverData>
      <ContextName>BasicCounters</ContextName>
      <Files>($SampleTraces)BasicPerfCounters.blg;</Files>
      <MetadataFiles></MetadataFiles>
      <IsRealTime>false</IsRealTime>
      <IsUsingDirectoryLookup>false</IsUsingDirectoryLookup>
    </DriverData>
  </Connection>
  <Namespace>Tx.Windows.Counters.Processor</Namespace>
</Query>

from ps in playback.GetObservable<Percent_Processor_Time>()
	where ps.CounterSet == "Processor" && ps.CounterName == "% Processor Time" && ps.Instance == "_Total"
	select new { ps.Timestamp, ps.Value }