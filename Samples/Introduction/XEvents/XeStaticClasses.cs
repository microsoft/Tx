// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Tx.SqlServer;

namespace Microsoft.SqlServer.XEvent.Static
{
    [XEvent("sql_statement_starting", "{40bfa6df-b111-41cd-a1e6-12209e618b8d}", Channel.Debug, "SQL RDBMS statement starting")]
    public class sql_statement_starting : BaseXEvent<sql_statement_starting>
    {
        public MapValue state { get; set; } 
        public Int32 line_number { get; set; }
        public Int32 offset { get; set; }
        public Int32 offset_end { get; set; }
        public String statement { get; set; }
    }

    [XEvent("sql_statement_completed", "{cdfd84f9-184e-49a4-bb71-1614a9d30416}", Channel.Debug, "SQL RDBMS statement completed")]
    public class sql_statement_completed : BaseXEvent<sql_statement_completed>
    {
        public Int64 duration { get; set; }
        public UInt64 cpu_time { get; set; }
        public UInt64 physical_reads { get; set; }
        public UInt64 logical_reads { get; set; }
        public UInt64 writes { get; set; }
        public UInt64 row_count { get; set; }
        public UInt64 last_row_count { get; set; }
        public Int32 line_number { get; set; }
        public Int32 offset { get; set; }
        public Int32 offset_end { get; set; }
        public String statement { get; set; }
        public Byte[] parameterized_plan_handle { get; set; }
    }

    [XEvent("login_timing", "{c747a483-f2d8-46c2-9e06-01b7bbca3838}", Channel.Debug, "SQL Azure Gateway login timing")]
    public class login_timing : BaseXEvent<login_timing>
    {
        public Int32 Version { get; set; }
        public Int32 LoginState { get; set; }
        public DateTimeOffset ClientConnectTime { get; set; }
        public DateTimeOffset ClientPreLoginTime { get; set; }
        public DateTimeOffset ClientLoginTime { get; set; }
        public DateTimeOffset FirewallCheckTime { get; set; }
        public DateTimeOffset AuthenticationTime { get; set; }
        public DateTimeOffset LookupDatabaseTime { get; set; }
        public DateTimeOffset LookupServerTime { get; set; }
        public DateTimeOffset ServerConnectTime { get; set; }
        public DateTimeOffset ServerPreLoginTime { get; set; }
        public DateTimeOffset ServerLoginTime { get; set; }
        public Double LoginDurationMs { get; set; }
        public Guid ActivityId { get; set; }
        public Guid SubscriptionId { get; set; }
        public String Server { get; set; }
        public String Database { get; set; }
    }
}
