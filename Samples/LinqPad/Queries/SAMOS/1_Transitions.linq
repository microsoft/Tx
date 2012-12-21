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
  </Connection>
</Query>

var all = from e in playback.GetObservable<SystemEvent>()
//          where e.Header.ActivityId == new Guid("80000146-0000-fe00-b63f-84710c7967bb")
		  select e;

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

transitions.Dump();