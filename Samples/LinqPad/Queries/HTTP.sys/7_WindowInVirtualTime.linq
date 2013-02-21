<Query Kind="Statements">
  <Connection>
    <ID>88a04bb7-535c-43a6-99c6-385c238126df</ID>
    <Driver Assembly="Tx.LinqPad" PublicKeyToken="3d3a4b0768c9178e">Tx.LinqPad.TxDataContextDriver</Driver>
    <DriverData>
      <ContextName>http</ContextName>
      <Files>($SampleTraces)HTTP_Server.etl;</Files>
      <MetadataFiles>($SampleTraces)HTTP_Server.man;</MetadataFiles>
      <IsRealTime>false</IsRealTime>
      <IsUsingDirectoryLookup>false</IsUsingDirectoryLookup>
    </DriverData>
  </Connection>
  <Namespace>Tx.Windows.Microsoft_Windows_HttpService</Namespace>
</Query>

// This is doing temporal query - aggregation in 5 sec windows
// where the 5 sec is in virtual time as per the event timestamps

var begin = playback.GetObservable<Parse>();
var end = playback.GetObservable<FastSend>();

var requests = from b in begin
            from e in end
                 .Where(e => b.Header.ActivityId == e.Header.ActivityId)
                 .Take(TimeSpan.FromSeconds(1), playback.Scheduler) // <-- Playback virtual time!
                 .Take(1)
            select new
            {
                b.Url,
                e.HttpStatus,
                Duration = e.Header.Timestamp - b.Header.Timestamp
            };

var statistics = from window in requests.Window(TimeSpan.FromSeconds(5), playback.Scheduler)
              from stats in
                  (   // calculate statistics within one window
                      from request in window
                      group request by new
                          {
                              Milliseconds = Math.Ceiling(request.Duration.TotalMilliseconds * 10) / 10,
                              request.Url
                          } into g
                      from Count in g.Count()
                      select new
                      {
                          g.Key.Url,
                          g.Key.Milliseconds,
                          Count
                      })
                      .ToList()
              select stats.OrderBy(s=> s.Milliseconds );

statistics.Dump();

playback.Run(); 
