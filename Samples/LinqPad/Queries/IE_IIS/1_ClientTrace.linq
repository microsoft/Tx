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

// A pre-requisite to approaching any distributed problem is to have an understanding of the physical topology. 
// In this example:
//
// +---------------+          +-----------------+
// |    georgis3   |  GET     |     georgis2    |
// |    (client)   | -------> |     (server)    |
// |               | <- - - - |                 |
// | IE_Client.etl |          | HTTP_Server.etl |
// +---------------+          +-----------------+

// Let's first look at the client trace.
// The goal is to identify events containing useful data.

playback.KnownTypes = typeof(Wininet_Connect_Start_1045).Assembly.GetTypes();

var formatted = (from e in playback.GetObservable<SystemEvent>()
                where e.GetType().Namespace == "Tx.Windows.Microsoft_Windows_WinINet"
				select new {
				Type = e.GetType().Name, 
				Message = e.ToString() }).Take(1000);
				
formatted.Dump();
				

// Afrer executing the query, we notice the following two events seem useful: 
// 		WININET_REQUEST_HEADER_Info_210  
//		WININET_HTTP_RESPONSE_Stop_203