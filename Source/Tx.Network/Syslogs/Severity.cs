namespace Tx.Network.Syslogs
{
    public enum Severity : byte
    {
        Emergency = 0,
        Alert,
        Critical,
        Error,
        Warning,
        Notice,
        Informational,
        Debug
    }
}