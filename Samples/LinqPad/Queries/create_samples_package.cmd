if exist %1 del %1 || goto failFast 
..\..\..\Tools\zip -r %1 header.xml NoManifest HTTP.sys IE_IIS "Performance Counters" SqlXevent WcfTroubleshooting