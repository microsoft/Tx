## Using Tx from LINQPad

[LINQPad](http://www.linqpad.net/) is the best way to learn the cutting edge features of C#

In the LINQPad experience of Tx is **as if all the events were in a Database**
Except, no database is involved - the the query happens directly on raw logs/traces or real-time sessions 

- [Getting started](../Source/Tx.LinqPad/Readme.md) with the Tx LINQPad Driver
- [Queries on trace from HTTP.sys](../Samples/LinqPad/Queries/HTTP.sys/Readme.md)  - the kernel driver used by IIS (Internet Information Services)
- [Troubleshooting WCF](../Samples/LinqPad/Queries/WcfTroubleshooting/Readme.md)  - using ETW traces (new in .Net 4.5) to understand errors in WCF client-server application 
- [Average and Deviation](../Samples/LinqPad/Queries/Performance%20Counters/Readme.md). This query is based on .blg file (the default output of PerfMon for counters)
- [Cross /provider & cross machine queries](../Samples/LinqPad/Queries/IE_IIS/Readme.md). Response time of IE on client machine and IIS on server.
- [No Manifest queries](../Samples/LinqPad/Queries/NoManifest/Readme.md). - limited way of using Tx even if you don't have the manifest.

## Using Tx from C# code

- [When to use Tx?](WhenToUse.md) vs. using just LINQ-to-Objects or Rx
- [Hello Tx](HelloTx.md) : Building a console app that does query on ETW trace
- [Type Generation](TypeGeneration.md) : How to generate C# types from Manifests and other schema descriptions
- [Playback samples](../Samples/Playback/Readme.md) : API to replay events from one or more file(s)/session(s) in order of occurrence
- [Features of Playback](PlaybackFeatures.md) : Multiplexed sequences, Heterogeneity, Same API for past & real-time, etc.
- [Synthetic Performance Counters](../Samples/SyntheticCounters/Readme.md) : Defining "counters" as queries on events from ETW real-time session (nothing hits disk)

## Conceptual overviews

- [The visual intuition](PlaybackConcepts.md) behind the Playback API
- [Playback Internals](PlaybackInternals.md) of the Playback
- [Extending](../Samples/Introduction/UlsLogs/Readme.md) Tx with semi-structured text logs like SharePoint's ULS format ("Unified Logging Service").
- [TimeSource](TimeSource.md): creation of virtual time from timestamps on the event

[Troubleshooting](Troubleshooting.md)