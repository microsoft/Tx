<Query Kind="Statements">
  <Connection>
    <ID>492b0438-a4c3-47cd-ba72-6c20ed8e8465</ID>
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
  <Namespace>System.Net</Namespace>
</Query>

var received = playback.GetObservable<KNetEvt_RecvIPV4>();

var x = from window in received.Window(TimeSpan.FromSeconds(5), playback.Scheduler)
     from stats in
         (   // calculate statistics within one window
             from packet in window
             group packet by packet.daddr into g
             from aggregate in g.Aggregate(
                 new { count=0.0, size = 0.0},
                 (ac, p) => new { count = ac.count+1, size = ac.size + p.size })
             select new
             {
                 Address = new IPAddress(g.Key).ToString(),
                 Average = aggregate.size / aggregate.count
             })
             .ToList()
     select stats;
					
var all = playback.BufferOutput(x);

playback.Run();

all.Dump();