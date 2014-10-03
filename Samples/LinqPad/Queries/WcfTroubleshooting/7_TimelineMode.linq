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

var timelineDetails = from all in playback.GetObservable<SystemEvent>()
						where all.Header.Level < 4
						select new
						{
							all.OccurenceTime,
							Type = all.GetType().Name,
							Details = all.ToString()
						};
						
timelineDetails.Dump();