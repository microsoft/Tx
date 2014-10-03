ECHO starting client

C:

set workingDir=C:\Program Files (x86)\Microsoft Visual Studio 11.0\VC\
set vs=vcvarsall.bat
 
cd %workingDir%
call %vs%

E:

cd E:\Troubleshooting WCF with LINQPad\Troubleshooting WCF with LINQPad - documentation\Code samples\WCFSample\WCFClient

svcutil.exe /language:cs /out:generatedProxy.cs /config:app.config http://localhost:8080/Calculator

cd bin/debug

for /l %%i in (1, 1, 10) do (

WCFClient.exe -add %%i %%i

WCFClient.exe -subtract 10 9

WCFClient.exe -multiply 9 7

WCFClient.exe -divide 7 6

WCFClient.exe -add 1 1

WCFClient.exe -subtract 4 9

WCFClient.exe -multiply 6 0

WCFClient.exe -divide 8 0

WCFClient.exe -divide 8 8

WCFClient.exe -divide 9 0

WCFClient.exe -divide 100 0

WCFClient.exe -divide 98 0
)

ECHO The correct base address is http://localhost:8080/Calculator
ECHO Let’s include a wrong base address in the command : http://localhost:80009000/Calculator

cd../../../WCFClient

set workingDir=C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\
set vs=vcvarsall.bat
 
cd %workingDir%
call %vs%

svcutil.exe /language:cs /out:generatedProxy.cs /config:app.config http://localhost:8000/Calculator

svcutil.exe /language:cs /out:generatedProxy.cs /config:app.config http://localhost:9000/Calculator

logman stop WCFETWTracing -ets