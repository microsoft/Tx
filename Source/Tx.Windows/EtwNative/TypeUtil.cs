// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Tx.Windows
{
    internal static class TypeServiceUtil
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport("Kernel32.dll")]
        internal static extern void RtlZeroMemory(IntPtr destPtr, int length);

        /// <summary>
        ///     Do a byte by byte comparison of two byte arrays
        /// </summary>
        /// <param name="leftByte">pointer to the memory that holds left side of comparison value</param>
        /// <param name="leftLen">the valid length for pLeft</param>
        /// <param name="rightByte">pointer to the memory that holds right side of comparison value</param>
        /// <param name="rightLen">the valid lenghth for pRight</param>
        /// <returns>
        ///     negatve value if pLeft is less than pRight, positive value if pLeft
        ///     is greater than pRight and 0 if equals
        /// </returns>
        public static unsafe int CompareBytes(byte* leftByte, int leftLen, byte* rightByte, int rightLen)
        {
            int length = Math.Min(leftLen, rightLen);
            for (int i = 0; i < length; i++)
            {
                if (*(leftByte + i) < *(rightByte + i))
                {
                    return -1;
                }
                if (*(leftByte + i) > *(rightByte + i))
                {
                    return 1;
                }
            }

            if (leftLen == rightLen)
            {
                return 0;
            }
            if (leftLen < rightLen)
            {
                return -1;
            }
            return 1;
        }

        /// <summary>
        ///     Copy memory in native bytes
        /// </summary>
        /// <param name="srcPtr"></param>
        /// <param name="destPtr"></param>
        /// <param name="bytesToCopy"></param>
        internal static unsafe void MemCopy(byte* srcPtr, byte* destPtr, int bytesToCopy)
        {
            // AMD64 implementation uses longs instead of ints where possible
            if (bytesToCopy >= 16)
            {
                do
                {
                    ((int*) destPtr)[0] = ((int*) srcPtr)[0];
                    ((int*) destPtr)[1] = ((int*) srcPtr)[1];
                    ((int*) destPtr)[2] = ((int*) srcPtr)[2];
                    ((int*) destPtr)[3] = ((int*) srcPtr)[3];

                    destPtr += 16;
                    srcPtr += 16;
                } while ((bytesToCopy -= 16) >= 16);
            }

            if (bytesToCopy > 0) // protection against negative len and optimization for len==16*N
            {
                if ((bytesToCopy & 8) != 0)
                {
                    ((int*) destPtr)[0] = ((int*) srcPtr)[0];
                    ((int*) destPtr)[1] = ((int*) srcPtr)[1];

                    destPtr += 8;
                    srcPtr += 8;
                }

                if ((bytesToCopy & 4) != 0)
                {
                    ((int*) destPtr)[0] = ((int*) srcPtr)[0];
                    destPtr += 4;
                    srcPtr += 4;
                }

                if ((bytesToCopy & 2) != 0)
                {
                    ((short*) destPtr)[0] = ((short*) srcPtr)[0];
                    destPtr += 2;
                    srcPtr += 2;
                }

                if ((bytesToCopy & 1) != 0)
                {
                    *destPtr = *srcPtr;
                }
            }
        }
    }
}