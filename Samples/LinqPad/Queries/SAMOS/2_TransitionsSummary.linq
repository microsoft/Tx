<Query Kind="Statements">
  <Connection>
    <ID>6944919a-6157-468e-ad23-2020618a3677</ID>
    <Driver Assembly="TxLinqPadDriver" PublicKeyToken="3d3a4b0768c9178e">TxLinqPadDriver.TxDataContextDriver</Driver>
    <DriverData>
      <ContextName>http</ContextName>
      <Files>C:\TraceInsight\SampleTraces\HTTP_Server.etl;</Files>
      <HideEventsThatDontOccur>true</HideEventsThatDontOccur>
      <IsRealTime>false</IsRealTime>
      <DataCollectorDefinition></DataCollectorDefinition>
    </DriverData>
    <DriverData>
      <ContextName>http</ContextName>
      <Files>C:\TraceInsight\SampleTraces\HTTP_Server.etl;</Files>
      <HideEventsThatDontOccur>true</HideEventsThatDontOccur>
      <IsRealTime>false</IsRealTime>
      <DataCollectorDefinition></DataCollectorDefinition>
    </DriverData>
  </Connection>
</Query>

var all = playback.GetObservable<SystemEvent>();

var transitions = all.Publish(input=>
{
	return from first in input 
		  from second in input.Take(1)
		  select new
			{ 
				FromState = first.Header.EventId,
				ToState = second.Header.EventId
			};
});

var transitionSummary = from t in transitions
	group t by t into sameType
	from c in sameType.Count()
	select new 
	{
		sameType.Key.FromState,
		sameType.Key.ToState,		
		Count = c
	};

// the query below is Linq-to-objects (Pull)
var ordered = from t in playback.BufferOutput(transitionSummary)
	orderby t.FromState, t.ToState
	select t;

playback.Run();

ordered.Dump();