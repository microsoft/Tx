<Query Kind="Statements">
  <Connection>
    <ID>7e542fef-390d-4333-bc5b-3bd121802be5</ID>
    <Driver Assembly="Tx.LinqPad" PublicKeyToken="3d3a4b0768c9178e">Tx.LinqPad.TxDataContextDriver</Driver>
    <DriverData>
      <ContextName>IE_IIS</ContextName>
      <Files>($SampleTraces)CrossMachineHTTP.etl;($SampleTraces)CrossMachineIE.etl;</Files>
      <MetadataFiles>($SampleTraces)HTTP_Server.man;($SampleTraces)IE_Client.man;</MetadataFiles>
      <IsRealTime>false</IsRealTime>
      <IsUsingDirectoryLookup>false</IsUsingDirectoryLookup>
    </DriverData>
  </Connection>
  <Namespace>Tx.Windows.Microsoft_Windows_HttpService</Namespace>
  <Namespace>Tx.Windows.Microsoft_Windows_WinINet</Namespace>
</Query>

// Here we also extract the correlation token from the string content

var serverRecv = from e in playback.GetObservable<Parse>()
		select new
		{
			Timestamp = e.OccurenceTime,
			ActivityId = e.Header.ActivityId,
			Token = e.Url.GetSubstring("?", null)
		};
		
var serverSendFast = from e in playback.GetObservable<FastSend>()
		select new
		{
			Timestamp = e.OccurenceTime,
			ActivityId = e.Header.ActivityId,
		};

var serverSendSlow = from e in playback.GetObservable<SendComplete>() // this looks like some alternative path in HTTP.sys
		select new
		{
			Timestamp = e.OccurenceTime,
			ActivityId = e.Header.ActivityId,
		};
		
var serverSend = serverSendFast.Merge(serverSendSlow); // we want all requests, regrardless of the code path

var serverActivities = from r in serverRecv 
		from s in serverSend.Where(s=> r.ActivityId == s.ActivityId)
		select new	{
				Token = r.Token,
				RequestReceived = r.Timestamp,
				ResponseSent =s.Timestamp,
				Duration = r.Timestamp - s.Timestamp
			};
			
serverActivities.Dump();