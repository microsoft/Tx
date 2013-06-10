// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Reactive;

namespace Tx.Windows
{
    internal class W3CTypeMap : ITypeMap<W3CEvent>
    {
        public Func<W3CEvent, DateTimeOffset> TimeFunction
        {
            get { return e => e.dateTime; }
        }

        public Func<W3CEvent, object> GetTransform(Type outputType)
        {
            return e => (object)e;
        }
    }
}
