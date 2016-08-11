set bin="c:\Bin"

if not exist "%bin%" mkdir %bin% || EXIT /B 1

pushd %~dp0

msbuild /p:Configuration=Release40 || EXIT /B 1
msbuild /p:Configuration=Debug40 || EXIT /B 1
msbuild /p:Configuration=Release45 || EXIT /B 1
msbuild /p:Configuration=Debug45 || EXIT /B 1

popd

copy ..\tools\NuGet.exe %bin%\ || EXIT /B 1
copy ..\tools\zip.exe %bin%\ || EXIT /B 1
copy ..\tools\PushPackages.cmd %bin%\ || EXIT /B 1

pushd  ..\Samples\LinqPad\Queries || EXIT /B 1
call create_samples_package.cmd || EXIT /B 1
popd

pushd

cd /d %bin%\Debug || EXIT /B 1
call :setVersion || EXIT /B 1
call :packAll || EXIT /B 1

cd /d %bin%\Release || EXIT /B 1
call :setVersion || EXIT /B 1
call :packAll || EXIT /B 1

cd /d %bin%\Release\Net40 || EXIT /B 1
..\..\zip.exe ..\..\Tx.LinqPad.lpx header.xml System.Reactive.Interfaces.dll System.Reactive.Core.dll System.Reactive.Linq.dll System.Reactive.PlatformServices.dll System.Reactive.Windows.Forms.dll Tx.Core.dll Tx.Windows.dll Tx.Windows.TypeGeneration.dll Tx.SqlServer.dll msvcr100.dll xe.dll Microsoft.SqlServer.XE.Core.dll Microsoft.SqlServer.XEvent.Configuration.dll Microsoft.SqlServer.XEvent.dll Microsoft.SqlServer.XEvent.Linq.dll Microsoft.SqlServer.XEvent.Targets.dll Tx.LinqPad.dll HTTP_Server.man HTTP_Server.etl BasicPerfCounters.blg CrossMachineHTTP.etl CrossMachineIE.etl IE_Client.man sqltrace.xel Microsoft.Windows.ApplicationServer.Applications.man SampleWcfTrace.etl || EXIT /B 1


popd
goto end

:setVersion

pushd Net40\Properties || EXIT /B 1
..\SetVersion.exe || EXIT /B 1
popd

pushd Net45\Properties || EXIT /B 1
..\SetVersion.exe || EXIT /B 1
popd

exit /b 0

:packAll
call :pack Tx.Core || EXIT /B 1
call :pack Tx.Windows || EXIT /B 1
call :pack Tx.Bond || EXIT /B 1
call :pack Tx.Network || EXIT /B 1
call :pack Tx.Windows.TypeGeneration || EXIT /B 1
call :pack Tx.SqlServer || EXIT /B 1
call :pack Tx.All || EXIT /B 1

exit /b 0

:pack %1
call Net40\Properties\%1.Layout.cmd || EXIT /B 1
cd /d %1 || EXIT /B 1
copy ..\Net40\Properties\%1.nuspec || EXIT /B 1
..\..\NuGet pack %1.nuspec || EXIT /B 1
move *.nupkg ..\ || EXIT /B 1
cd ..
rd /s/q %1 || EXIT /B 1
exit /b 0

:end
popd
exit /b 0
