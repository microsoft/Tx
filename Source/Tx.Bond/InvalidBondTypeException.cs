// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Tx.Bond
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class InvalidBondTypeException : Exception
    {
        public InvalidBondTypeException(string message)
            : base(message)
        {
        }


        public InvalidBondTypeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected InvalidBondTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
