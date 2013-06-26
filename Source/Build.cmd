set bin="c:\Bin"

msbuild /p:Configuration=Release40
msbuild /p:Configuration=Debug40
msbuild /p:Configuration=Release45
msbuild /p:Configuration=Debug45

copy ..\tools\NuGet.exe %bin%\
copy ..\tools\zip.exe %bin%\
copy ..\tools\PushPackages.cmd %bin%\

pushd  ..\Samples\LinqPad\Queries
call create_samples_package.cmd
popd

pushd

cd /d %bin%\Debug
call :setVersion
call :packAll

cd /d %bin%\Release
call :setVersion
call :packAll

cd /d %bin%\Release\Net40
..\..\zip.exe ..\..\Tx.LinqPad.lpx header.xml System.Reactive.Interfaces.dll System.Reactive.Core.dll System.Reactive.Linq.dll System.Reactive.PlatformServices.dll System.Reactive.Windows.Forms.dll Tx.Core.dll Tx.Windows.dll Tx.Windows.TypeGeneration.dll Tx.SqlServer.dll xe.dll Microsoft.SqlServer.XE.Core.dll Microsoft.SqlServer.XEvent.Configuration.dll Microsoft.SqlServer.XEvent.dll Microsoft.SqlServer.XEvent.Linq.dll Microsoft.SqlServer.XEvent.Targets.dll Tx.LinqPad.dll HTTP_Server.man HTTP_Server.etl BasicPerfCounters.blg CrossMachineHTTP.etl CrossMachineIE.etl IE_Client.man sqltrace.xel


popd
goto end

:setVersion

pushd Net40\Properties
..\SetVersion.exe
popd

pushd Net45\Properties
..\SetVersion.exe
popd

exit /b 0

:packAll
call :pack Tx.Core
call :pack Tx.Windows
call :pack Tx.Windows.TypeGeneration
call :pack Tx.SqlServer
call :pack Tx.All

exit /b 0

:pack %1
call Net40\Properties\%1.Layout.cmd
cd /d %1
copy ..\Net40\Properties\%1.nuspec
..\..\NuGet pack %1.nuspec
move *.nupkg ..\
cd ..
rd /s/q %1
exit /b 0

:end
popd
exit /b 0
