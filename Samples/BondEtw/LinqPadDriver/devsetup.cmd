@rem This is post build step that sets locally the LINQPad data context driver for Tx

@echo off

if not "%1" == "" cd %1

set DRIVER_DIR=c:\ProgramData\LINQPad\Drivers\DataContext\4.0\BondEtwDriver (3d3a4b0768c9178e)
if not exist "%DRIVER_DIR%" md "%DRIVER_DIR%"

copy header.xml "%DRIVER_DIR%"\
copy gbc.exe "%DRIVER_DIR%"\
copy Bond.Attributes.* "%DRIVER_DIR%"\
copy Bond.* "%DRIVER_DIR%"\
copy BondEtwDriver.* "%DRIVER_DIR%"\
copy Microsoft.Diagnostics.Tracing.EventSource.* "%DRIVER_DIR%"\
copy System.Reactive.* "%DRIVER_DIR%"\
copy Tx.Bond.* "%DRIVER_DIR%"\
copy Tx.Core.* "%DRIVER_DIR%"\
copy Tx.Windows.* "%DRIVER_DIR%"\

del BondEtwDriver.lpx 
zip.exe BondEtwDriver.lpx header.xml gbc.exe Bond.Attributes.* Bond.* BondEtwDriver.* Microsoft.Diagnostics.Tracing.EventSource.* System.Reactive.* Tx.Bond.* Tx.Core.* Tx.Windows.*

