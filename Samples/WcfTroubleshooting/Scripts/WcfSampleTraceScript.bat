logman  start WCFETWTracing -p "Microsoft-Windows-Application Server-Applications"  -nb 256 1024 -bs 512 -ets -ct perf -f bincirc -max 500 -o "../../../Traces/SampleWcfTrace.etl"

ECHO Started tracing to etl file

cd ../WCFHost/bin/debug

start .\WCFHost.exe

ECHO Starting Client session.. 
cd ../../../Scripts/
start .\InvokeWcf.bat
pause