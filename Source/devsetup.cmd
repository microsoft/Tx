@rem This is post build step that sets locally the LINQPad data context driver for Tx

@echo off

if not "%1" == "" cd %1

set DRIVER_DIR=c:\ProgramData\LINQPad\Drivers\DataContext\4.0\Tx.LinqPad (3d3a4b0768c9178e)
if not exist "%DRIVER_DIR%" md "%DRIVER_DIR%"

set TYPE_DIR=%DRIVER_DIR%\EventTypes
if not exist "%TYPE_DIR%" md "%TYPE_DIR%"
 
call :copy_dll Tx.Core
call :copy_dll Tx.Windows
call :copy_dll Tx.Windows.TypeGeneration
call :copy_dll Tx.LinqPad

echo Reactive Binaries
copy ..\References\DESKTOPCLR40\System.Reactive.Interfaces.dll "%DRIVER_DIR%"\
copy ..\References\DESKTOPCLR40\System.Reactive.Core.dll "%DRIVER_DIR%"\
copy ..\References\DESKTOPCLR40\System.Reactive.Linq.dll "%DRIVER_DIR%"\
copy ..\References\DESKTOPCLR40\System.Reactive.PlatformServices.dll "%DRIVER_DIR%"\

echo header.xml
copy Tx.LinqPad\header.xml "%DRIVER_DIR%"\

goto end

:copy_dll
echo %1.dll
copy c:\bin\Debug\Net40\%1.dll "%DRIVER_DIR%"\
copy c:\bin\Debug\Net40\%1.pdb "%DRIVER_DIR%"\
exit /b 0

:copy_exe
echo %1.exe
copy c:\bin\Debug\Net40\%1.exe "%DRIVER_DIR%"\
copy c:\bin\Debug\Net40\%1.pdb "%DRIVER_DIR%"\

exit /b 0

:end
exit /b 0