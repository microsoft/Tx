set bin="c:\Bin"

msbuild /p:Configuration=Release40
msbuild /p:Configuration=Debug40
msbuild /p:Configuration=Release45
msbuild /p:Configuration=Debug45

copy ..\tools\NuGet.exe %bin%\
copy ..\tools\zip.exe %bin%\

pushd

cd %bin%\Debug
call :packAll

cd %bin%\Release
call :packAll

cd %bin%\Release\Net40
..\..\zip.exe ..\..\Tx.LinqPad.lpx header.xml System.Reactive.Interfaces.dll System.Reactive.Core.dll System.Reactive.Linq.dll System.Reactive.PlatformServices.dll Tx.Core.dll Tx.Windows.dll Tx.Windows.TypeGeneration.dll Tx.LinqPad.dll


popd
goto end

:packAll
call :pack Tx.Core
call :pack Tx.Windows
call :pack Tx.Windows.TypeGeneration
call :pack Tx.SqlServer
call :pack Tx.All

exit /b 0

:pack %1
call Net40\Properties\%1.Layout.cmd
cd %1
copy ..\Net40\Properties\%1.nuspec
..\..\NuGet pack %1.nuspec
move *.nupkg ..\
cd ..
rd /s/q %1
exit /b 0

:end
popd
exit /b 0
