<Query Kind="Statements">
  <Connection>
    <ID>81682494-18d6-444e-bb8f-2538ed87da22</ID>
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

// This query calculates the average CPU, the standard deviation, minuimum and maximum of CPU
// The approach with "Power Sums" is described here: http://en.wikipedia.org/wiki/Standard_deviation 
// Look for “Rapid Calculation Methods” in this page.

var cpu = from ps in playback.GetObservable<Percent_Processor_Time>()
	where ps.CounterSet == "Processor" && ps.CounterName == "% Processor Time" && ps.Instance == "_Total"
	select ps;

var powerSumBases = from ps in cpu
	select new	{
		s0_base = 1,
		s1_base = ps.Value,
		s2_base = ps.Value * ps.Value
	};

var powerSums = from window in powerSumBases.Window(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), playback.Scheduler)
   from a in window.Aggregate(
   new { s0 = 0.0, s1 = 0.0, s2 = 0.1},
   (acc, point) => new { 
       s0 = acc.s0 + point.s0_base, 
       s1 = acc.s1 + point.s1_base,
       s2 = acc.s2 + point.s2_base })
       select a;

var avgAndDeviation = from ps in powerSums
          select new {
              Average = ps.s1 / ps.s0,
              Deviation = Math.Sqrt((ps.s0 * ps.s2 - ps.s1 * ps.s1) / (ps.s0 * ps.s0 - 1)),
          };


avgAndDeviation.Dump();

playback.Run();