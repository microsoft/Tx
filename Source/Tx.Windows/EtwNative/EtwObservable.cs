using System;
using System.Reactive.Linq;

namespace Tx.Windows
{
    /// <summary>
    /// Factory for creating raw ETW (Event Tracing for Windows) observable sources
    /// </summary>
    public static class EtwObservable 
    {
        /// <summary>
        /// Creates a reader for one or more .etl (Event Trace Log) files
        /// </summary>
        /// <param name="etlFiles">up to 63 files to read</param>
        /// <returns>sequence of events ordered by timestamp</returns>
        public static IObservable<EtwNativeEvent> FromFiles(params string[] etlFiles)
        {
            if (etlFiles == null)
                throw new ArgumentNullException("etlFiles");

            if (etlFiles.Length == 0 || etlFiles.Length > 63)
                throw new ArgumentException("the supported count of files is from 1 to 63");

            return Observable.Create<EtwNativeEvent>(o=>new EtwFileReader(o, etlFiles));
        }

        /// <summary>
        /// Creates a listener to ETW real-time session
        /// </summary>
        /// <param name="sessionName">session name</param>
        /// <returns>events received from the session</returns>
        public static IObservable<EtwNativeEvent> FromSession(string sessionName)
        {
            if (sessionName == null)
                throw new ArgumentNullException("sessionName");

            return Observable.Create<EtwNativeEvent>(o=>new EtwListener(o, sessionName));
        }
    }
}
