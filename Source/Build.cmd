REM Switch to the folder where this script resides
pushd "%~dp0" 

REM Set developer environment for VS 2017
call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\Tools\VsDevCmd.bat" -arch=amd64 -host_arch=amd64 -winsdk=10.0.16299.0

REM Restore dependencies; there is some nuance here which we will get to later
REM dotnet build /p:Configuration=Release /p:Platform=x64 "%~dp0TxKql.sln"
msbuild "%~dp0Tx.sln"

if "%ERRORLEVEL%" neq "0" (
   echo "Failed to build solution."
   popd
   exit /B -1
)

popd
exit /B 0
