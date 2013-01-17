// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using Tx.Windows;

namespace TxSamples.EvtxRaw
{
    class Program
    {
        static void Main()
        {
            IEnumerable<EventRecord> evtx = EvtxEnumerable.FromFiles(@"HTTP_Server.evtx");
            Console.WriteLine(evtx.Count());

            Console.ReadLine();
        }
    }
}
