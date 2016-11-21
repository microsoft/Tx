rd /s/q Tx.SqlServer

md Tx.SqlServer\lib\Net45

call :copy Tx.SqlServer.dll
call :copy Tx.SqlServer.xml
call :copy Tx.SqlServer.pdb
call :copy Microsoft.SqlServer.XEvent.dll
call :copy Microsoft.SqlServer.XE.Core.dll
call :copy Microsoft.SqlServer.XEvent.Configuration.dll
call :copy Microsoft.SqlServer.XEvent.Linq.dll
call :copy Microsoft.SqlServer.XEvent.Targets.dll

md Tx.SqlServer\content
copy Net45\xe.dll Tx.SqlServer\content\
copy Net45\msvcr100.dll Tx.SqlServer\content\

goto end

:copy
copy Net45\%1 Tx.SqlServer\lib\Net45\
exit /b 0

:end
exit /b 0

