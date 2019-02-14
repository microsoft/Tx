// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Reactive.Linq;
using System.Collections.Generic;

namespace Tx.Windows
{
    /// <summary>
    /// Class that produces decoded ETW events as dictionary { name, value }
    /// Internally, this uses the Trace Data Helper (TDH) API to obtain the metadata
    /// from the OS registry of manifests that were installed on the machine
    /// </summary>
    public static class EtwTdhObservable
    {
        /// <summary>
        /// Creates a reader for one or more .etl (Event Trace Log) files
        /// </summary>
        /// <param name="etlFiles">up to 63 files to read</param>
        /// <returns>sequence of events ordered by timestamp</returns>
        public static IObservable<IDictionary<string, object>> FromFiles(params string[] etlFiles)
        {
            EtwTdhDeserializer d = new EtwTdhDeserializer();
            var file = EtwObservable.FromFiles(etlFiles);
            return file.Select(e => new EtwTdhEvent(d, e));
        }

        /// <summary>
        /// Hooks to real-time ETW session. The session must be created ahead of time
        /// </summary>
        /// <param name="sessionName">the real-time session name</param>
        /// <returns></returns>
        public static IObservable<IDictionary<string, object>> FromSession(string sessionName)
        {
            EtwTdhDeserializer d = new EtwTdhDeserializer();
            var session = EtwObservable.FromSession(sessionName);
            return session.Select(e => new EtwTdhEvent(d, e));
        }
    }
}
