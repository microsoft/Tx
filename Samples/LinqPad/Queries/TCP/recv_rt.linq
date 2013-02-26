<Query Kind="Statements">
  <Connection>
    <ID>e92f3bbf-e115-4274-914d-a19ca0c64259</ID>
    <Persist>true</Persist>
    <Driver Assembly="Tx.LinqPad" PublicKeyToken="3d3a4b0768c9178e">Tx.LinqPad.TxDataContextDriver</Driver>
    <DriverData>
      <ContextName>tcp_realTime</ContextName>
      <SessionName>tcp</SessionName>
      <Files></Files>
      <MetadataFiles>C:\TxSamples\LinqPad\Manifests\SystemEvents.man;</MetadataFiles>
      <IsRealTime>true</IsRealTime>
      <IsUsingDirectoryLookup>false</IsUsingDirectoryLookup>
    </DriverData>
  </Connection>
  <GACReference>System.Windows.Forms.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35</GACReference>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Security.Principal</Namespace>
  <Namespace>Tx.Windows.Microsoft_Windows_Kernel_Network</Namespace>
  <Namespace>System.Windows.Forms.DataVisualization.Charting</Namespace>
</Query>

var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
	throw new Exception("To use ETW real-time session, please start LINQPad as administrator");

Process logman = Process.Start(
	"logman.exe",
	"create trace TCP -rt -nb 2 2 -bs 1024 -p {7dd42a49-5329-4832-8dfd-43d979153a88} 0xffffffffffffffff -ets");
logman.WaitForExit();

var received = playback.GetObservable<KNetEvt_RecvIPV4>();

var x = from window in received.Window(TimeSpan.FromSeconds(1))
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
              select new { stats.Count, Points=stats.OrderBy(s=> s.Count )};

x.DumpLive();

playback.Start();

