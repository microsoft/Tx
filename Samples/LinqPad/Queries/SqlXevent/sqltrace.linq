<Query Kind="Expression">
  <Connection>
    <ID>c133d683-ab9e-4616-ad16-9e8319a50ce7</ID>
    <Persist>true</Persist>
    <Driver Assembly="Tx.LinqPad" PublicKeyToken="3d3a4b0768c9178e">Tx.LinqPad.TxDataContextDriver</Driver>
    <DriverData>
      <ContextName>sqltrace</ContextName>
      <Files>($SampleTraces)sqltrace.xel;</Files>
      <MetadataFiles></MetadataFiles>
      <IsRealTime>false</IsRealTime>
      <IsUsingDirectoryLookup>false</IsUsingDirectoryLookup>
    </DriverData>
  </Connection>
</Query>

from c in playback.GetObservable<Tx.SqlServer.sqlserver.sql_statement_completed>()
select new { c.statement, c.row_count }