set bin="c:\Bin"

if not exist "%bin%" mkdir %bin% || goto failFast

if "%1"=="NoBuild" goto noBuild

pushd %~dp0

msbuild /p:Configuration=Release40 || goto failFast
msbuild /p:Configuration=Debug40 || goto failFast
msbuild /p:Configuration=Release45 || goto failFast
msbuild /p:Configuration=Debug45 || goto failFast

popd

:noBuild

copy ..\tools\NuGet.exe %bin%\ || goto failFast
copy ..\tools\zip.exe %bin%\ || goto failFast
copy ..\tools\PushPackages.cmd %bin%\ || goto failFast

pushd  ..\Samples\LinqPad\Queries || goto failFast
call create_samples_package.cmd
popd

pushd

cd /d %bin%\Debug || goto failFast
call :setVersion || goto failFast
call :packAll || goto failFast

cd /d %bin%\Release || goto failFast
call :setVersion || goto failFast
call :packAll || goto failFast

cd /d %bin%\Release\Net40 || goto failFast
..\..\zip.exe ..\..\Tx.LinqPad.lpx header.xml System.Reactive.Interfaces.dll System.Reactive.Core.dll System.Reactive.Linq.dll System.Reactive.PlatformServices.dll System.Reactive.Windows.Forms.dll Tx.Core.dll Tx.Windows.dll Tx.Windows.TypeGeneration.dll Tx.SqlServer.dll msvcr100.dll xe.dll Microsoft.SqlServer.XE.Core.dll Microsoft.SqlServer.XEvent.Configuration.dll Microsoft.SqlServer.XEvent.dll Microsoft.SqlServer.XEvent.Linq.dll Microsoft.SqlServer.XEvent.Targets.dll Tx.LinqPad.dll HTTP_Server.man HTTP_Server.etl BasicPerfCounters.blg CrossMachineHTTP.etl CrossMachineIE.etl IE_Client.man sqltrace.xel Microsoft.Windows.ApplicationServer.Applications.man SampleWcfTrace.etl || goto failFast

popd
goto end

:setVersion

pushd Net40\Properties || goto failFast
..\SetVersion.exe || goto failFast
popd

pushd Net45\Properties || goto failFast
..\SetVersion.exe || goto failFast
popd

exit /b 0 

:packAll
call :pack Tx.Core || goto failFast
call :pack Tx.Windows || goto failFast
call :pack Tx.Bond || goto failFast
call :pack Tx.Network || goto failFast
call :pack Tx.Windows.TypeGeneration || goto failFast
call :pack Tx.SqlServer || goto failFast
call :pack Tx.All || goto failFast

exit /b 0 

:pack %1
call Net40\Properties\%1.Layout.cmd || goto failFast
cd /d %1 || goto failFast
copy ..\Net40\Properties\%1.nuspec || goto failFast
..\..\NuGet pack %1.nuspec || goto failFast
move *.nupkg ..\ || goto failFast
cd ..
rd /s/q %1 || goto failFast
exit /b 0

:end
cd %~dp0
exit /b 0

:failFast
cd %~dp0
exit /b 1