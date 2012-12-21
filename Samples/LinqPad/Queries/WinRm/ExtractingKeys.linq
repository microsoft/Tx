<Query Kind="Program">
  <Connection>
    <ID>ceda4c03-6947-41d1-a094-c8f70c6efa7f</ID>
    <Persist>true</Persist>
    <Driver Assembly="TxLinqPadDriver" PublicKeyToken="3d3a4b0768c9178e">TxLinqPadDriver.TxDataContextDriver</Driver>
    <DriverData>
      <ContextName>WinRm</ContextName>
      <Files>C:\TxSamples\LinqPad\Traces\WsRm01.etl;</Files>
      <MetadataFiles>C:\TxSamples\LinqPad\Manifests\Web-Services-for-Management-Core.man;</MetadataFiles>
      <IsRealTime>false</IsRealTime>
      <IsUsingDirectoryLookup>false</IsUsingDirectoryLookup>
    </DriverData>
  </Connection>
  <Namespace>Microsoft.Etw.Microsoft_Windows_WinRM</Namespace>
</Query>

void Main()
{
	var ids = from e in playback.GetObservable<Microsoft.Etw.Microsoft_Windows_WinRM.LOG_WSMAN_AN_SOAP_LISTENER_RECEIVING>()
	where e.SoapDocument.StartsWith("<s:Envelope")
	select new
	{
		Id = GetSubstring(e.SoapDocument, "<a:MessageID>uuid:","</a:MessageID>")
	};
	
	ids.Dump();
}

public static string GetSubstring(string source, string startMarker, string endMarker)
{
	int startIndex = source.IndexOf(startMarker);
	int begin = startIndex + startMarker.Length;

	string result;
	if (endMarker == null)
	{
		result = source.Substring(begin);
	}
	else
	{
		int end = source.IndexOf(endMarker, begin);
		result = source.Substring(begin, end - begin);
	}

	return result;
}