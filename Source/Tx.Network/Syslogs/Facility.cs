namespace Tx.Network.Syslogs
{
    public enum Facility : byte
    {
        Kernel = 0,

        UserLevel,

        MailSystem,

        SystemDaemons,

        Authorization,

        Syslog,

        Printer,

        News,

        Uucp,

        Clock,

        SecurityAuth,

        Ftp,

        Ntp,

        LogAudit,

        LogAlert,

        ClockDaemon,

        Local0,

        Local1,

        Local2,

        Local3,

        Local4,

        Local5,

        Local6,

        Local7
    }
}