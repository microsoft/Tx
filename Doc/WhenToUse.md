# When to use Tx?

Common sense when writing code is to take minimum dependencies. Tx is built on technologies that existed before, and in many cases it is easier to take dependency only on these and not on Tx. 

## When NOT to use Tx

Use **only** LINQ-to-Objects when:

* There are no **real-time** feeds involved
* The data is already in memory (e.g. List &lt;T&gt;), or you are dealing with single file that is easy to parse - e.g. a text file, each line representing event of the same type. 

Example how to parse text files using LINQ-to-Objects is [W3CEnumerable.cs](http://tx.codeplex.com/SourceControl/latest#Source/Tx.Windows/IIS/W3CEnumerable.cs). This parses the W3C logs from IIS

Use **only** [Reactive Extensions (Rx)](http://rx.codeplex.com) when:

* There are **real-time** feeds (e.g. mouse moves)
* You need the same query to work on past history (e.g. file(s)) and real-time
* Each feed/file contains a **single type** of events, like T for IObservable&lt;T&gt;


## When to use Tx


Tx.Core adds the following new features to Rx: 

* Support for **Multiplexed** sequences (single sequence containing events of different types in order of occurence). The simplest example of turning multiplexd sequence into type-specific Obseravable-s is the [Demultiplexor](http://tx.codeplex.com/SourceControl/latest#Source/Tx.Core/Demultiplexor.cs)
* Merging **multiple input files** in order of occurence - e.g. two log files
* **Hereogeneous** Inputs - e.g. queries across some log files (.evtx) and some traces (.etl)
* **Single-pass-read** to answer multiple queries on file(s)
* **Scale in # of queries**. This is side effect of the above, which applies to both real-time and single-read of past history
* Providing **virtual time** as per event timestamps. See [TimeSource](http://tx.codeplex.com/wikipage?title=TimeSource)
* Support for both "Structured" (like database, or Rx) and "Timeline" mode (like most event-viewing tools)

Building on the above, various Tx.**&lt;product&gt;** parsers provide out-of-box support for API that is missing in .Net:

Tx.Windows

* [Event Tracing for Windows (ETW)](http://msdn.microsoft.com/en-us/library/windows/desktop/aa363668(v=vs.85).aspx)
* Performance Counters - interop with the "Performance Data Helper" (PDH) [native API](http://msdn.microsoft.com/en-us/library/windows/desktop/aa373214(v=vs.85).aspx)
* Small fix to present Event Logs (.evtx) as IEnumerable
* W3C text logs from IIS

Tx.SqlServer

* SQL Server Extended Events ([XEvent](http://blogs.msdn.com/b/extended_events/)) is the tracing technology used by the SQL-Server engine.

Extensibility

Here is an [Extensibility Sample](http://tx.codeplex.com/wikipage?title=ULS%20Sample). 

This shows multiplexed stream of semi-structured text (Sharepoint's "Unified Logging Service" (ULS) format)











 



