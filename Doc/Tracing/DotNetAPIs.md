# Tracing From .Net

With Tx, we are betting on a design pattern that can easily represent any kind of log/trace as input to LINQ. This assumes that the code producing traces already exists and we are not going to change it. 

Adding new way of tracing simply means specific parser as in the Tx [Extensibility sample](../../Samples/Introduction/UlsLogs/Readme.md)

Still, in retrospective it is worth looking at the different approaches to tracing:

![ApproachesComparison.JPG](ApproachesComparison.JPG)

## printf

There are numerous reincarnations of tracing/logging to plain text, that elaborate on the  "printf" paradigm in standard C. 

In C# the most notable API-s are:

* [System.Diagnostics.TraceSource](http://msdn.microsoft.com/en-us/library/system.diagnostics.tracesource(v=vs.110).aspx)
* [Log4Net](http://logging.apache.org/log4net/)
* [Patterns & Practices "Looging and Instrumentation Application Block"](https://msdn.microsoft.com/en-us/library/ff648417.aspx)
 
Regardless of API fashions:

* concatenating strings is wasting CPU and memory 
* implementing one's own writing to disk is usually worse than [TextWriterTraceListener](http://msdn.microsoft.com/en-us/library/system.diagnostics.textwritertracelistener(v=vs.110).aspx)
* files are bloated by repeating format strings
* there is no way to tell ahead of time what kinds of events may occur 

Pieces from Tx code relevant for plain text are: 

* The [extensibility sample](../../Samples/Introduction/UlsLogs/Readme.md)
* Support for [W3C text logs](../../Source/Tx.Windows/IIS/W3CEnumerable.cs) from IIS 

## Text in ETW
One simple solution to fixing the run-time performance is to redirect the traces [using EventProviderTraceListener](http://blogs.msdn.com/b/peaceofmind/archive/2008/04/16/using-eventprovidertracelistener.aspx), which shipped in .Net 2.0.

ETW implements asynchronous buffering and disk-space pre-allocation, so compared to self-made trace-listener the run-time performance degradation can be reduced from 40 times to negligible (below 1%, within the boundaries of experimental error)

Unfortunately, this does not fix the other problems. Files are even bigger because each file has header block and each event has ETW header. 

Relevant Tx samples are:

* [Causality Navigation](../../Samples/TimeAndOrder/CausalityNavigation/Readme.md) for how to trace and then read it
* [IE_IIS](../../Samples/LinqPad/Queries/IE_IIS/Readme.md) for parsing sub-strings from text in ETW events

## Windows Trace Pre-Processor (WPP)

WPP originated as a way to instrument native code so that:

* The API is set of macros as easy as printf
* The formatting strings are not traced, but put in the pdb instead
* Tools like [tracefmt.exe](https://msdn.microsoft.com/en-us/library/windows/hardware/ff552974(v=vs.85).aspx) and [traceview.exe](https://technet.microsoft.com/en-us/subscriptions/index/ff553892) reconstruct the traces for viewing

[NTrace](http://ntrace.codeplex.com/) looks like open source .Net implementation. 

Note WPP traces are not currently supported in Tx.

## ETW with manually written manifests

In this approach, manifests are manually written and then compiled to ETW proxies. Here are the:

* [Original paper](https://msdn.microsoft.com/en-us/magazine/cc163437.aspx) describing the approach
* [Writing Manifests](https://msdn.microsoft.com/en-us/library/windows/desktop/dd996930(v=vs.85).aspx) on MSDN

Although this produces excellent traces to use LINQ on, it requires significant amount of work to write the manifest and complex build environment.

One other problem is the fragility as Manifests are distributed separately from the ETL files and can easily mismatch.

## Event Source

Finally the best approach is using the [EventSource API](http://blogs.msdn.com/b/vancem/archive/2012/08/13/windows-high-speed-logging-etw-in-c-net-using-system-diagnostics-tracing-eventsource.aspx) which shipped in .Net 4.5.
Additional benefits of this approach are:

* Manifests are auto-generated
	* this reduces the complexity a lot
	* it results in unique, friendly names in the symbol attribute (showing as class names in Tx)
* Manifests are included in the etl files
	* This eliminates UI steps in (no need to browse for Manifests)
	* It makes sure the right manifest is used







