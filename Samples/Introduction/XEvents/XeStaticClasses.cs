using System;
using Tx.SqlServer;

namespace Microsoft.SqlServer.XEvent.Static
{
    // the intention with the code belo is to reuse the classes C# developers define
    // to produce XEvents - e.g. Mugunthan's GpmAccessEvent

    [XEvent("sql_statement_starting", "{40bfa6df-b111-41cd-a1e6-12209e618b8d}", Channel.Debug, "SQL RDBMS statement starting")]
    public class sql_statement_starting : BaseXEvent<sql_statement_starting>
    {
        public MapValue state;
        public Int32 line_number;
        public Int32 offset;
        public Int32 offset_end;
        public String statement;
    }

    [XEvent("sql_statement_completed", "{cdfd84f9-184e-49a4-bb71-1614a9d30416}", Channel.Debug, "SQL RDBMS statement completed")]
    public class sql_statement_completed : BaseXEvent<sql_statement_completed>
    {
        public Int64 duration;
        public UInt64 cpu_time;
        public UInt64 physical_reads;
        public UInt64 logical_reads;
        public UInt64 writes;
        public UInt64 row_count;
        public UInt64 last_row_count;
        public Int32 line_number;
        public Int32 offset;
        public Int32 offset_end;
        public String statement;
        public Byte[] parameterized_plan_handle;
    }

    [XEvent("login_timing", "{c747a483-f2d8-46c2-9e06-01b7bbca3838}", Channel.Debug, "SQL Azure Gateway login timing")]
    public class login_timing : BaseXEvent<login_timing>
    {
        public Int32 Version;
        public Int32 LoginState;
        public DateTimeOffset ClientConnectTime;
        public DateTimeOffset ClientPreLoginTime;
        public DateTimeOffset ClientLoginTime;
        public DateTimeOffset FirewallCheckTime;
        public DateTimeOffset AuthenticationTime;
        public DateTimeOffset LookupDatabaseTime;
        public DateTimeOffset LookupServerTime;
        public DateTimeOffset ServerConnectTime;
        public DateTimeOffset ServerPreLoginTime;
        public DateTimeOffset ServerLoginTime;
        public Double LoginDurationMs;
        public Guid ActivityId;
        public Guid SubscriptionId;
        public String Server;
        public String Database;
    }
}
