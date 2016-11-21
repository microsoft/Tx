namespace Tests.Tx.Network
{
    using System;
    using System.Collections.Generic;

    using global::Tx.Network.Syslogs;

    public class TxSyslogTestSettings
    {
        public static string SourceIP = "127.0.0.1";
        public static string TargetIP = "127.0.0.1";
        public static int TargetPort = 514;
        public static TimeSpan Delay = TimeSpan.FromMilliseconds(10);
        public static TimeSpan Duration = TimeSpan.FromSeconds(10);
        public static List<string> MessageList = new List<string>()
        {
            "<140> Dec 19 10:42:53 Router1A %DAEMON-4-RPD_MPLS_LSP_CHANGE: MPLS LSP TU.Router1A.Router4b.01 change on primary(standard_path) Route  10.6.32.244(flag=0x21) 10.4.82.244(flag=1 Label=373824) 10.6.32.101(flag=0x21) 10.4.80.28(flag=1 Label=489839) 10.6.32.163(flag=0x21) 10.2.140.117(flag=1 Label=787179) 10.6.32.166(flag=0x21) 10.4.83.239(flag=1 Label=424677) 10.6.32.229(flag=0x21) 10.4.80.106(flag=1 Label=381322) 10.6.32.227(flag=0x21) 10.4.82.162(flag=1 Label=303724) 10.6.32.136(flag=0x21) 10.4.82.233(flag=1 Label=301840) 10.6.32.137(flag=0x21) 10.2.141.205(flag=1 Label=651624) 10.6.32.13(flag=0x21) 10.4.81.254(flag=1 Label=438961) 10.9..144.13(flag=0x21) 10.6.38.210(flag=1 Label=477580) 10.9..144.8(flag=0x21) 10.6.42.147(flag=1 Label=746841) 10.9.144.22(flag=0x20) 10.6.42.13(Label=0) lsp bandwidth 31478736 bps",
            "<140> Dec 19 10:42:58 Router2B %DAEMON-4-RPD_MPLS_LSP_CHANGE: MPLS LSP TU.Router2B.Router2A.01 change on primary(standard_path) Route  10.6.32.137(flag=0x21) 10.4.81.255(flag=1 Label=651672) 10.6.32.136(flag=0x21) 10.2.141.204(flag=1 Label=301888) 10.6.32.227(flag=0x21) 10.4.82.234(flag=1 Label=303772) 10.6.32.229(flag=0x21) 10.4.82.107(flag=1 Label=381370) 10.6.32.244(flag=0x21) 10.6.45.53(flag=1 Label=373888) 10.6.32.88(flag=0x20) 10.4.82.236(Label=0) lsp bandwidth 859795072 bps",
            "<140> Dec 19 10:43:00 Router3 %DAEMON-4-RPD_MPLS_LSP_CHANGE: MPLS LSP TU.Router3.Router4A.01 change on primary(standard_path) Route  10.6.32.245(flag=0x21) 10.6.45.216(flag=1 Label=335930) 10.6.32.244(flag=0x21) 10.4.83.5(flag=1 Label=373936) 10.6.32.101(flag=0x21) 10.4.80.28(flag=1 Label=490111) 10.6.32.163(flag=0x21) 10.2.140.117(flag=1 Label=787275) 10.6.32.166(flag=0x21) 10.4.80.27(flag=1 Label=424757) 10.6.32.229(flag=0x21) 10.4.80.106(flag=1 Label=381418) 10.6.32.174(flag=0x20) 10.4.82.125(Label=0) lsp bandwidth 26163330 bps",
            "<140> Dec 19 10:43:03 Router2B %DAEMON-4-RPD_MPLS_LSP_CHANGE: MPLS LSP TU.Router2B.Router2C.01 change on primary(standard_path) Route  10.6.32.137(flag=0x21) 10.4.82.3(flag=1 Label=651688) 10.6.32.136(flag=0x21) 10.2.141.204(flag=1 Label=301920) 10.6.32.227(flag=0x21) 10.4.82.232(flag=1 Label=303820) 10.6.32.229(flag=0x21) 10.4.82.163(flag=1 Label=381450) 10.6.32.244(flag=0x21) 10.6.45.61(flag=1 Label=373952) 10.6.32.89(flag=0x20) 10.4.82.239(Label=0) lsp bandwidth 902207488 bps",
            "<140> Dec 19 10:43:04 Router2B %DAEMON-4-BGP_PREFIX_THRESH_EXCEEDED: FACE:1057::1:7b8::1 (External AS 65432): Configured maximum prefix-limit threshold(450) exceeded for inet6-unicast nlri: 457 (instance master)",
            "<140> Dec 19 10:43:12 Router2B %DAEMON-4: /usr/sbin/sshd[70132]: exited, status 255"
        };
    }
    public class SimpleTxSyslog
    {
        public string Message;
        public Severity Sev;
        public Facility Fac;
        public DateTimeOffset RecTime;
    }
    
}