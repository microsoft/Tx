<Query Kind="Statements">
  <Connection>
    <ID>3a12b3b9-0c7d-4ea6-9f20-3c805780fb9b</ID>
    <Driver Assembly="Tx.LinqPad" PublicKeyToken="3d3a4b0768c9178e">Tx.LinqPad.TxDataContextDriver</Driver>
    <DriverData>
      <ContextName>WcfSample</ContextName>
      <Files>($SampleTraces)SampleWcfTrace.etl;</Files>
      <MetadataFiles>($SampleTraces)Microsoft.Windows.ApplicationServer.Applications.man;</MetadataFiles>
      <IsRealTime>false</IsRealTime>
      <IsUsingDirectoryLookup>false</IsUsingDirectoryLookup>
    </DriverData>
  </Connection>
  <Namespace>Tx.Windows.Microsoft_Windows_Application_Server_Applications</Namespace>
</Query>

playback.KnownTypes = typeof(HandledExceptionError).Assembly.GetTypes();

var errors = from all in playback.GetObservable<SystemEvent>()
				where all.Header.Level < 4
				select new 
				{
					Type = all.GetType().Name,
					Details = all.ToString(),
					Duration = all.OccurenceTime					
				};
				
var grouped = from error in errors
				group error by error.Type into groupedError
				from count in groupedError.Count()
				select new
				{
					groupedError.Key,
					count
				};
				
grouped.Dump("Count of errors in trace");

var details = from error in errors
				group error by new {Type = error.Type, Details = error.Details} into groupedError
				from count in groupedError.Count()
				select new
				{
					Type = groupedError.Key.Type,
					Details = groupedError.Key.Details,
					Occurences = count					
				};
				
details.Dump("Grouped details");