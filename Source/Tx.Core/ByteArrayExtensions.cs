// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Text;

namespace System.Reactive
{
    public static class ByteArrayExtensions
    {
        /// <summary>
        /// Displays the byte array as hex-dump, like within a debugger
        /// </summary>
        /// <param name="bytes">array to display as hex</param>
        /// <returns></returns>
        public static string ToHexDump(this byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            int lineOffset = 0;

            while (lineOffset < bytes.Length)
            {
                // output line offset from the start of the buffer
                sb.Append(lineOffset.ToString("x4"));
                sb.Append(": ");

                // output hex dump
                int endOffset = Math.Min(lineOffset + 16, bytes.Length);
                int index = 0;
                for (int byteOffset = lineOffset; byteOffset < endOffset; byteOffset++)
                {
                    if (index == 8)
                        sb.Append(' '); 
                    
                    index++;
                    sb.Append(bytes[byteOffset].ToString("x2"));
                    sb.Append(' ');
                }

                // fill in the blanks if we cut off without completing entire line
                int lineLength = endOffset - lineOffset;
                if (lineLength < 16)
                {
                    for (int i = lineLength; i < 16; i++ )
                    {
                        if (index == 8)
                            sb.Append(' ');

                        index++;
                        
                        sb.Append("   ");
                    }
                }

                sb.Append("  ");

                // output character dump
                index = 0;
                for (int byteOffset = lineOffset; byteOffset < endOffset; byteOffset++)
                {
                    index++;
                    byte b = bytes[byteOffset];
                    if (b > 32)
                        sb.Append((char)b);
                    else
                        sb.Append('.');

                    if (index == 8)
                        sb.Append(' ');
                }

                sb.AppendLine();
                lineOffset += 16;
            }

            return sb.ToString();
        }
    }
}
