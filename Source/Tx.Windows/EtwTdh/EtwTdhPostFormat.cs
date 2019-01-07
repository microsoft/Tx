using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Tx.Windows
{
    /// <summary>
    /// This is a placeholder for all the cases in which the formatting is not enough
    /// Example are IP addresses that are reprenseted as integers 
    /// For now it is hard-coded until we understnad in what cases we need to post-process
    /// values that were formatted accoring to TDH
    /// </summary>
    class EtwTdhPostFormat
    {
        static readonly Guid Microsoft_Windows_Kernel_Network = new Guid("7dd42a49-5329-4832-8dfd-43d979153a88");
        public static object ApplyFormatting(Guid providerId, uint eventId, string name, object value)
        {
            if (providerId == Microsoft_Windows_Kernel_Network)
            {
                if (name.Contains("addr"))
                {
                    if (value.GetType() == typeof(byte[]))
                        return new IPAddress((byte[])value).ToString();
                    else
                        return new IPAddress((uint)value).ToString();

                }
            }

            return value;
        }

    }
}
