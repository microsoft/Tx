set sourceFolder=%~dp0
set dropFolder=%~dp0

(
set /p versionParam=
)<%sourceFolder%version.txt

echo %versionParam%

rem set msbuildPath="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\msbuild.exe"
set msbuildPath="msbuild"

pushd
cd /d %sourceFolder%SetVersion || goto failFast
dotnet restore || goto failFast
dotnet msbuild /p:Configuration=Release || goto failFast
popd

pushd  %sourceFolder%..\Samples\LinqPad\Queries || goto failFast
call create_samples_package.cmd %dropFolder%samples.zip
popd

call :pack Tx.Core || goto failFast
call :pack Tx.Windows || goto failFast
call :pack Tx.Bond || goto failFast
call :pack Tx.Network || goto failFast
call :pack Tx.SqlServer || goto failFast

pushd
%sourceFolder%SetVersion\bin\Release\SetVersion.exe %versionParam% %sourceFolder%Tx.Windows.TypeGeneration\Tx.Windows.TypeGeneration.csproj || goto failFast
cd /d %sourceFolder%Tx.Windows.TypeGeneration || goto failFast
dotnet restore || goto failFast
dotnet build -c=Release || goto failFast
%sourceFolder%SetVersion\bin\Release\SetVersion.exe %versionParam% %sourceFolder%EtwEventTypeGen\EtwEventTypeGen.csproj || goto failFast
%sourceFolder%SetVersion\bin\Release\SetVersion.exe %versionParam% %sourceFolder%EtwEventTypeGen\Properties\Tx.Windows.TypeGeneration.nuspec || goto failFast
cd /d %sourceFolder%EtwEventTypeGen || goto failFast
dotnet restore || goto failFast
dotnet build -c=Release || goto failFast
copy %sourceFolder%EtwEventTypeGen\Properties\Tx.Windows.TypeGeneration.nuspec %sourceFolder%EtwEventTypeGen\bin\Release\net45\ || goto failFast
cd /d %sourceFolder%EtwEventTypeGen\bin\Release\net45 || goto failFast
%sourceFolder%..\tools\NuGet pack Tx.Windows.TypeGeneration.nuspec || goto failFast
move %sourceFolder%EtwEventTypeGen\bin\Release\net45\Tx.Windows.TypeGeneration.%versionParam%*.nupkg %dropFolder% || goto failFast
popd

pushd
cd /d %sourceFolder%Tx.Linqpad || goto failFast
dotnet restore || goto failFast
%msbuildPath% /p:Configuration=Release || goto failFast
cd /d %sourceFolder%Tx.Linqpad\bin\Release\net45 || goto failFast
%sourceFolder%..\tools\zip.exe %dropFolder%Tx.LinqPad.lpx header.xml System.Reactive.Interfaces.dll System.Reactive.Core.dll System.Reactive.Linq.dll System.Reactive.PlatformServices.dll System.Reactive.Windows.Forms.dll Tx.Core.dll Tx.Windows.dll Tx.Windows.TypeGeneration.dll Tx.SqlServer.dll %sourceFolder%..\References\XEvent\msvcr100.dll %sourceFolder%..\References\XEvent\xe.dll Microsoft.SqlServer.XE.Core.dll Microsoft.SqlServer.XEvent.Configuration.dll Microsoft.SqlServer.XEvent.dll Microsoft.SqlServer.XEvent.Linq.dll Microsoft.SqlServer.XEvent.Targets.dll Tx.LinqPad.dll HTTP_Server.man HTTP_Server.etl BasicPerfCounters.blg CrossMachineHTTP.etl CrossMachineIE.etl IE_Client.man sqltrace.xel Microsoft.Windows.ApplicationServer.Applications.man SampleWcfTrace.etl || goto failFast
popd

pushd
%sourceFolder%SetVersion\bin\Release\SetVersion.exe %versionParam% %sourceFolder%Tx.All\Tx.All.nuspec || goto failFast
cd /d %sourceFolder%Tx.All || goto failFast
%sourceFolder%..\tools\NuGet pack Tx.All.nuspec || goto failFast
move %sourceFolder%Tx.All\Tx.All.*.nupkg %dropFolder%\ || goto failFast
popd

goto end

:pack %1

%sourceFolder%SetVersion\bin\Release\SetVersion.exe %versionParam% %sourceFolder%%1\%1.csproj || goto failFast

pushd
cd /d %sourceFolder%%1 || goto failFast
dotnet restore || goto failFast
dotnet build -c=Release || goto failFast
move %sourceFolder%%1\bin\Release\%1.*.nupkg %dropFolder% || goto failFast
popd

:end
cd %~dp0
exit /b 0

:failFast
cd %~dp0
exit /b 1