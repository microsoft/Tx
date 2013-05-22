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
  <Namespace>Tx.Windows.Microsoft_Windows_WinINet</Namespace>
</Query>

// Here the correlation token is in the string content of the event
// So, we use the extension method GetSubString("startToken", "endToken") to extract it as a field

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
		
var clientActivities= from s in clientSend   
		              from r in clientReceive.Where(r=>r.ActivityId == s.ActivityId).Take(1)
					  select new {
						File = s.File,
						Token = s.Token,
						ReqestSent = s.Timestamp,            // both timestamps are from the clock on the client machine
						ResponseReceived = r.Timestamp,      // thus, it is safe to subtract them
						Duration = r.Timestamp - s.Timestamp // the output is the client's perceived duration				
					};

clientActivities.Dump();