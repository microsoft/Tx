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

// Here GetSubstring is an extension method with signature:  		
//    public static string GetSubstring(this string source, string startMarker, string endMarker)
//
// You can implement your own extension methods and add reference to the assembly/namespace by clicking Query->Query Properties menu.

var clientSend = from e in playback.GetObservable<WININET_REQUEST_HEADER_Info_210>()
		select new 
		{
			Timestamp = e.OccurenceTime,
			ActivityId = e.Header.ActivityId,
			File = e.Headers.GetSubstring("/","?"),
			Token = e.Headers.GetSubstring("?"," HTTP/1.1")
		};
		
var clientReceive = from e in playback.GetObservable<WININET_HTTP_RESPONSE_Stop_203>()
		select new
		{
			Timestamp = e.OccurenceTime,
			ActivityId = e.Header.ActivityId,
		};
		
var clientActivities= from s in clientSend   // joining events with timestamps from the same clock is safe
		              from r in clientReceive.Where(r=>r.ActivityId == s.ActivityId).Take(1)
					  select new {
						File = s.File,
						Token = s.Token,
						ReqestSent = s.Timestamp,
						ResponseReceived = r.Timestamp
					};
					
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

var serverSendSlow = from e in playback.GetObservable<SendComplete>() // some alternative path in HTTP.sys?
		select new
		{
			Timestamp = e.OccurenceTime,
			ActivityId = e.Header.ActivityId,
		};
		
var serverSend = serverSendFast.Merge(serverSendSlow);

var serverActivities = from r in serverRecv // joining events with timestamps from the same clock is safe
		from s in serverSend.Where(s=> r.ActivityId == s.ActivityId)
		select new	{
				Token = r.Token,
				RequestReceived = r.Timestamp,
				ResponseSent =s.Timestamp
			};
			
var durations = from c in clientActivities
		from s in serverActivities.Where(s=> c.Token == s.Token).Take(1)
		select new {
				c.File,
				c.Token,
				ClientDuration = (c.ResponseReceived - c.ReqestSent).TotalMilliseconds, // subtracting timestamps from the same machine is safe
				ServerDuration = (s.ResponseSent - s.RequestReceived).TotalMilliseconds, 
			};

			
var totalTime = from d in durations
		group d by d.File into groups
		from a in groups.Aggregate(
			new { ClientDuration = 0.0, ServerDuration = 0.0, Count = 0 },
			(a,e) => new { 
				ClientDuration = a.ClientDuration + e.ClientDuration, 
				ServerDuration = a.ServerDuration + e.ServerDuration,
				Count = a.Count + 1})
		select new
		{
			File = groups.Key,
			a.ClientDuration,
			a.ServerDuration,
			a.Count
		};
			
var averages = from t in totalTime
	select new 
		{
			File = t.File,
			AverageClientDurationMs = t.ClientDuration / t.Count,
			AverageServerDurationMs = t.ServerDuration / t.Count,
			AverageNetworkDuration = (t.ClientDuration - t.ServerDuration) / t.Count // subtracting is safe because the rate is the same
		};
		
var sorted  = from e in playback.BufferOutput(averages)
			orderby e.File, e.AverageClientDurationMs select e;
						
playback.Run();

sorted.Dump();