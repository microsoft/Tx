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
</Query>

// Now let look at the server trace:

playback.KnownTypes = typeof(Parse).Assembly.GetTypes();

var formatted = (from e in playback.GetObservable<SystemEvent>()
                where e.GetType().Namespace == "Tx.Windows.Microsoft_Windows_HttpService"
				select new {
				Type = e.GetType().Name, 
				Message = e.ToString() }).Take(1000);
				
formatted.Dump();


// Afrer executing the query, we notice the following two events seem useful: 
// 		Parse  
//		FastSend