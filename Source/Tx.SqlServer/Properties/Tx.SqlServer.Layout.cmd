rd /s/q Tx.SqlServer

md Tx.SqlServer\lib\Net40

call :copy Tx.SqlServer.dll
call :copy Tx.SqlServer.xml
call :copy Microsoft.XEvent.dll
call :copy Microsoft.XEvent.xml
call :copy Microsoft.SqlServer.XE.Core.dll
call :copy Microsoft.SqlServer.XEvent.Configuration.dll
call :copy Microsoft.SqlServer.XEvent.Linq.dll
call :copy Microsoft.SqlServer.XEvent.Targets.dll
call :copy xe.dll

goto end

:copy
copy Net40\%1 Tx.SqlServer\lib\Net40\
exit /b 0

:end
exit /b 0

