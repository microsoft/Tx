IF EXISTS(SELECT * FROM sys.server_event_sessions WHERE name='PlaybackDemo')
   DROP EVENT SESSION [PlaybackDemo] ON SERVER;
CREATE EVENT SESSION [PlaybackDemo]
ON SERVER
ADD EVENT sqlserver.sql_statement_starting,
ADD EVENT sqlserver.sql_statement_completed
ADD TARGET package0.asynchronous_file_target
   (SET filename='c:\test\PlaybackDemo.xel', metadatafile='c:\test\PlaybackDemo.xem');
GO

ALTER EVENT SESSION [PlaybackDemo]
ON SERVER
STATE=START
GO

select * from sysobjects
select * from sys.dm_xe_packages
GO

ALTER EVENT SESSION [PlaybackDemo]
ON SERVER
STATE=STOP
GO
