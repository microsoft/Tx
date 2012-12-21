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
	  
var last = from e in all 
			group e by e.Header.ActivityId into activities
			from l in activities.TakeLast(1) 
			select new
			{
				l.Header.ActivityId,
				l.Header.EventId
			};
	
last.Dump();