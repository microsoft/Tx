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
  <Reference>&lt;RuntimeDirectory&gt;\Accessibility.dll</Reference>
  <Reference Relative="..\..\..\..\References\Linq2Charts\LINQ2Charts.dll">C:\git\tx\References\Linq2Charts\LINQ2Charts.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Deployment.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Serialization.Formatters.Soap.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Security.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Windows.Forms.DataVisualization.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Windows.Forms.dll</Reference>
  <Namespace>System.Linq.Charting</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Runtime.CompilerServices</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
  <Namespace>Tx.Windows.Microsoft_Windows_Kernel_Network</Namespace>
</Query>

var columns = new Column{ Points = {}, LegendText = "IP Addresses" };
var chart = new Chart
{ ChartAreas = { new ChartArea { Series = { columns }} }
, Dock = DockStyle.Fill,
};
chart.Dump("IP Traffic");

var received = playback.GetObservable<KNetEvt_RecvIPV4>();

var x = from window in received.Window(TimeSpan.FromSeconds(10), playback.Scheduler)
              from stats in
                  (   // calculate statistics within one window
                      from packet in window
                      group packet by packet.saddr into g
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

x.ObserveOn(chart).Subscribe(v =>
{
	chart.BeginInit();
	
		columns.BasePoints.Clear();
		foreach(var point in v.Points) columns.Add(point.Address, point.Count);
		
	chart.EndInit();
});

playback.Run();