netsh advfirewall firewall add rule name="TCP 9000" dir=in protocol=tcp localport=9000 action=allow

logman start CausalityNavigation -nb 64 512 -bs 1024 -p {8400115e-3a7a-4fb0-95ca-6121397f7c4a} 0xff -o LocalTrace.etl -ets

start BounceMessages.exe
BounceMessages.exe

logman stop CausalityNavigation -ets

CausalityNavigation.exe