// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Microsoft.SqlServer.XEvent.Linq;
using System.IO;

namespace Tx.SqlServer
{
    public class XeObservable
    {
        public static IObservable<PublishedEvent> FromFiles(params string[] xelFiles)
        {
            if (xelFiles == null)
                throw new ArgumentNullException("xelFiles");

            // Looks like XEvent has bug handling relative paths
            string[] fullPaths = (from f in xelFiles select Path.GetFullPath(f)).ToArray();

            var enumerable = new QueryableXEventData(xelFiles);

            return enumerable.ToObservable(ThreadPoolScheduler.Instance);
        }

        public static IObservable<PublishedEvent> FromTarget<TTarget>() where TTarget : XeSubject, new()
        {
            return new TTarget();
        }
    }
}