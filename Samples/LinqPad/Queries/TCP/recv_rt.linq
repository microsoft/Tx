<Query Kind="Statements">
  <Connection>
    <ID>b8da68f2-b9e3-4067-80c3-279d0f12fc7e</ID>
    <Driver Assembly="TxLinqPadDriver" PublicKeyToken="3d3a4b0768c9178e">TxLinqPadDriver.TxDataContextDriver</Driver>
    <DriverData>
      <ContextName>tcp real-time</ContextName>
      <Files>c:\TxSamples\LINQPad\Traces\KernelNetwork.etl;</Files>
      <MetadataFiles>c:\TxSamples\LINQPad\Manifests\SystemEvents.man;</MetadataFiles>
      <IsRealTime>true</IsRealTime>
      <IsUsingDirectoryLookup>false</IsUsingDirectoryLookup>
      <SessionName>tcp</SessionName>
    </DriverData>
  </Connection>
  <Namespace>System.Net</Namespace>
  <Namespace>Microsoft.Etw.Microsoft_Windows_Kernel_Network</Namespace>
  <Namespace>System.Security.Principal</Namespace>
</Query>

var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
	throw new Exception("To use ETW real-time session, please start LINQPad as administrator");

Process logman = Process.Start(
	"logman.exe",
	"create trace TCP -rt -nb 2 2 -bs 1024 -p {7dd42a49-5329-4832-8dfd-43d979153a88} 0xffffffffffffffff -ets");
logman.WaitForExit();

var o = from e in playback.GetObservable<KNetEvt_RecvIPV4>()
select new 
{
	address = new IPAddress(e.daddr).ToString(),
	e.dport,
	e.size,
};

o.DumpLive();

playback.Start();