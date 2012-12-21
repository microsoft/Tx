using System.Net;
using System.Text;
using System;
using System.Runtime.InteropServices;

namespace Tcp
{
    public static class FormattingExtensions
    {

        public static IPAddress FormatAsIPAddress(this byte[] data)
        {
            if (data.Length == 0)
                return IPAddress.None;

            if (data[0] != 2)
                return IPAddress.None;

            // interpret as struct sockaddr_in: http://msdn.microsoft.com/en-us/library/aa921121.aspx
            if (data[4] == 0)
                return IPAddress.None;

            IPAddress ipAddress = new IPAddress(new byte[]{ data[4], data[5], data[6], data[7] });
            return ipAddress;
        }
    }
}
