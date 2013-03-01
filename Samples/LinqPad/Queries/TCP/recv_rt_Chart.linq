<Query Kind="Statements">
  <Connection>
    <ID>e92f3bbf-e115-4274-914d-a19ca0c64259</ID>
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
  <Reference Relative="..\..\..\..\References\Linq2Charts\LINQ2Charts.dll">C:\git\tx\References\Linq2Charts\LINQ2Charts.dll</Reference>
  <GACReference>System.Windows.Forms.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35</GACReference>
  <Namespace>System.Linq.Charting</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Runtime.CompilerServices</Namespace>
  <Namespace>System.Security.Principal</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
  <Namespace>Tx.Windows.Microsoft_Windows_Kernel_Network</Namespace>
  <Namespace>Tx.Windows.Microsoft_Windows_Kernel_Network</Namespace>
</Query>

// Start the real time session
var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
	throw new Exception("To use ETW real-time session, please start LINQPad as administrator");

Process logman = Process.Start(
	"logman.exe",
	"create trace TCP -rt -nb 2 2 -bs 1024 -p {7dd42a49-5329-4832-8dfd-43d979153a88} 0xffffffffffffffff -ets");
logman.WaitForExit();

// create chart
var columns = new Column{ Points = {}, LegendText = "IP Addresses" };
var chart = new Chart
{ ChartAreas = { new ChartArea { Series = { columns }} }
, Dock = DockStyle.Fill,
};
chart.Dump("IP Traffic");

// This is the actual query
var received = playback.GetObservable<KNetEvt_RecvIPV4>();

var x = from window in received.Window(TimeSpan.FromSeconds(3))
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
				
// And here, we just draw
var y = x.Publish();

y.ObserveOn(chart).Subscribe(v =>
{
	chart.BeginInit();
	
		columns.BasePoints.Clear();
		foreach(var point in v.Points) columns.Add(point.Address, point.Count);
		
	chart.EndInit();
});

y.Connect();
playback.Start();
y.Dump();