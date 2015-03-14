# Playback Features

Here is description of the featuures that Playback adds, different than everything that already exists in Rx.

## Multiplexed sequences
In Rx, sequences are represented as IObservable**&lt;T&gt;**. 

This is a simple view, in which all events are of the same type **T**. This allows leveraging many C#  features like IntelliSense, compile-time validation, etc. Assuming all events are of one type also works on some logs like W3C and .csv files.

In most real logs/traces however, there are many types of events multiplexed in order of occurence:

![Multiplexed Sequence](MultiplexedSequence.JPG)


Here circles, triangles, and squares represent types of events

* On the left we see the original (multiplexed) sequence - **all events** in order of occurence
* On the right, the events are de-multiplexed into typed sequences represented as IObservable&lt;T&gt; 

De-multipplexing (ignoring performance improvements) is semantically equivalent to:

* var c = m.GetObservable&lt;Circle&gt;
* var t = m.GetObservable&lt;Triangle&gt;
* var sq = ...

Tx provides two implementations of De-Multiplexing

* [Demultiplexor](../Source/Tx.Core/Demultiplexor.cs) is a component exposing IObservable&lt;**Object**&gt; in left and GetObservable**&lt;T&gt;** on right
* Playback acts the same as Demultiplexor as far as types are concerned

For visual intuition, see the [Playback Conceptual model](PlaybackConcepts.md)

## Structured and Time-line mode
Playback supports two modes of query:

* **Structured mode**, which is analogous to Database (Rx on the typed sequences)
* **Time-line** mode, in which events of multiple types show up in order of occurrence on the same IObservable&lt;BaseClass&gt;. This is similar to classic event-viewing tools such as EventVwr

For more details see [Playback Samples](./Samples/Playback/Readme.md)

## Hiding the heterogeneity of the event sources

It would have been nice if there was one single format for logs & traces... But in reality:

* There is proliferation of formats (e.g. Logs vs. ETW in Windows)
* Often, the interesting questions are accross formats

For example, typical suport issue  can include XEvents from SQL Server as well as Logs and ETW from Windows. Adding IIS or Sharepoint brings their formats too (W3C and ULS).

Playback separates the responsibility of parsing specific format from building queries. 

* Users of Playback can build queries without need to understand file-format details. For them Playback just represents the stream of [ALL events as C# instances](PlaybackConcepts.md)
* Parsing is about implementing IEnumerable or IObservable. 
	* Sometimes this as easy as a wrapper of some old API that existed before LINQ. 
	* In other cases it requires learning a whole ecosystem like the ETW versions of metadata (classic vs. Manifest). 


See also:

* [Playback Internals](PlaybackInternals.md)
* [Extensibility sample](../Samples/Introduction/UlsLogs/Readme.md) about adding semi-structured text format traces (SharePoint Unified Logging Service (ULS) format)

## Same API for real-time and past history

Historically, the concept of "query" evolved for stored data. With Rx however, queries work the same way on real-time events and on stored events. This is why many people describe Rx simply as "LINQ to Events".

For example, WPF UI applications don't store mouse-move events. But, if events are stored (say, for testing purposes) the same Rx query can be used without changes, simply by getting events in push mode again (e.g. using IEnumerable.ToObservable())

Playback preserves this API symmetry, by converting [all observables to Hot](HotObservables.md)

This allows for very simple procedure to build real-time queries:

* Capture events in a file, and build a query using virtual time
* Instead of files, use real-time feeds

Exmaple of query that was build like this is creating [synthetic counters](SyntheticCounters.md) from ETW events.

Because Playback is simply merging the incomming Observable-s, it is also possible to mix past and real-time sources. In this case all the past events are delivered in a burst followed by the real-time events when they occur.

## Single-Read to answer multiple queries from file(s)

The typical pattern of access to logs (=traces) it that:
 
* Events are written all the time
* Noone ever wants to look at them...unless there is a problem. And when problem arises, people want to answer ad-hoc questions ranging from state-less queries (e.g. "grep") to stateful queries like statistics about durations between begin and end events.

Tx takes a different aproch than "storage & query" engines (Databases, Hadoop, etc.). Instead of  uploading events you keep them in the log file. Then one or more queries are executed on **single-pass read** over the file(s).

Imagine that we have one huge log file, and:

* We want to count all events 
* We also want to match "Begin" and "End" events, and calculate average duration

Similarly you can build any number of queries. When calling .Subscribe(...) the in-memory graphs of IObserver callbacks are fully constructed but no events flow through. The reading of the file(s) is initiated when you call Run() or Start():

* Run() blocks the current thread until the file(s) are read
* Start() returns immediately (you must keep the IDisposable-s for real-time operation)

To cancel the processing you can call:
* Dispose on some query to cancel it without impacting other queries
* Dispose on the Playback to cancel the file-read (thus all queries)

Relevant samples are:

* Get2Observables in the [Playback samples](../Samples/Playback/Program.cs) 
* Single Pass in the [HTTP trace samples](../Samples/LinqPad/Queries/HTTP.sys/Readme.md)

## Occurence Time Scheduler

Imagine for example counting events in 5 sec window of time. 

The subtlety in this statement is that it makes sense only in "occurrence time" - i.e. some time-stamps of the events. 

The default Time in Rx is system time. This is great for events like mouse-move, that occur at the same machine and are pushed to the callbacks with negligible latency. Here using .Window(5 sec) to count mouse-move events will work flawlesly.
 
This is not the case when the events are stored in a file. If we used the default behavior of Window, it will aggregate as per time it takes to read events. In typical ETW reading speed (above 200,000 events/sec) any normal trace will be processed for less than a second... and the result will be wrong: one total count.

Playback builds on the concept of **virtual time** in Rx to provide a Scheduler that represents the occurence time of events.

This can be passed as argument to primitives like Window, to produce deterministic results based only on event content (incl. timestamps). This way on machine with twice faster disk the output of the query will be exactly the same - only it will show up twice faser.

For more details see:

* [TimeSource](TimeSource.md), which is the mechanism of presenting "occurence time" as virtual time
* [Playback Internals](PlaybackInternals.md)


