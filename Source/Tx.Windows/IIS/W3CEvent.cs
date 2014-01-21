// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Tx.Windows
{
    public class W3CEvent
    {
        public DateTime dateTime { get; set; }
        public string c_ip { get; set; }
        public string cs_bytes { get; set; }
        public string cs_Cookie { get; set; }
        public string cs_host { get; set; }
        public string cs_method { get; set; }
        public string cs_Referer { get; set; }
        public string cs_uri_query { get; set; }
        public string cs_uri_stem { get; set; }
        public string cs_User_Agent { get; set; }
        public string cs_username { get; set; }
        public string cs_version { get; set; }
        public string s_computername { get; set; }
        public string s_ip { get; set; }
        public string s_port { get; set; }
        public string s_sitename { get; set; }
        public string sc_bytes { get; set; }
        public string sc_status { get; set; }
        public string sc_substatus { get; set; }
        public string sc_win32_status { get; set; }
        public string time_taken { get; set; }
    }
}
