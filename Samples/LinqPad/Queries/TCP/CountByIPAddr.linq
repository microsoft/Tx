<Query Kind="Statements">
  <Connection>
    <ID>492b0438-a4c3-47cd-ba72-6c20ed8e8465</ID>
    <Driver Assembly="Tx.LinqPad" PublicKeyToken="3d3a4b0768c9178e">Tx.LinqPad.TxDataContextDriver</Driver>
    <DriverData>
      <ContextName>tcp</ContextName>
      <Files>C:\git\tx\Traces\tcp.etl;</Files>
      <MetadataFiles>C:\TxSamples\LinqPad\Manifests\SystemEvents.man;</MetadataFiles>
      <IsRealTime>false</IsRealTime>
      <IsUsingDirectoryLookup>false</IsUsingDirectoryLookup>
    </DriverData>
  </Connection>
  <Namespace>Tx.Windows.Microsoft_Windows_Kernel_Network</Namespace>
  <Namespace>System.Net</Namespace>
</Query>

var received = playback.GetObservable<KNetEvt_RecvIPV4>();

var x = from window in received.Window(TimeSpan.FromSeconds(10), playback.Scheduler)
              from stats in
                  (   // calculate statistics within one window
                      from packet in window
                      group packet by packet.daddr into g
                      from Count in g.Count()
                      select new
                      {
                          g.Key,
                          Count
                      })
                      .ToList()
              select new { 
			  		stats.Count, 
					Points=from s in stats orderby s.Count descending 
					       select new { s.Count, Address = new IPAddress(s.Key).ToString() }
				};

var y = playback.BufferOutput(x);

playback.Run();

y.Dump();