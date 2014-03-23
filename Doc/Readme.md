## Using Tx from LINQPad

[LINQPad](http://www.linqpad.net/) is the best way to learn the cutting edge features of C#

In the LINQPad experience of Tx is **as if all the events were in a Database**
Except, no database is involved - the the query happens directly on raw logs/traces or real-time sessions 

- [Getting started](https://tx.codeplex.com/wikipage?title=LINQPad%20Driver) with the Tx LINQPad Driver
- [Queries on trace from HTTP.sys](https://tx.codeplex.com/wikipage?title=HTTP%20Samples)  - the kernel driver used by IIS (Internet Information Services)
- [Average and Deviation](https://tx.codeplex.com/wikipage?title=Performance%20Counter%20Samples). This query is based on .blg file (the default output of PerfMon for counters)
- [Cross /provider & cross machine queries](https://tx.codeplex.com/wikipage?title=IE_IIS). Response time of IE on client machine and IIS on server.

## Using Tx from C# code

- [When to use Tx?](WhenToUse.md) vs. using just LINQ-to-Objects or Rx
- [Hello Tx](HelloTx.md) : Building a console app that does query on ETW trace
- [Type Generation](TypeGeneration.md) : How to generate C# types from Manifests and other schema descriptions
- [Playback samples](../Samples/Playback/Readme.md) : API to replay events from one or more file(s)/session(s) in order of occurrence
- [Features of Playback](PlaybackFeatures.md) : Multiplexed sequences, Heterogeneity, Same API for past & real-time, etc.
- [Synthetic Performance Counters](../Samples/SyntheticCounters/Readme.md) : Defining "counters" as queries on events from ETW real-time session (nothing hits disk)

## Conceptual overviews

- [The visual intuition](http://tx.codeplex.com/wikipage?title=Playback%20conceptual%20model) behind the Playback API
- [Internals](http://tx.codeplex.com/wikipage?title=Playback%20Internals) of the Playback
- [Extending](http://tx.codeplex.com/wikipage?title=ULS%20Sample) Tx with semi-structured text logs  (SharePoint's ULS format).
- [TimeSource](https://tx.codeplex.com/wikipage?title=TimeSource) creation of virtual time from timestamps on the event

[Troubleshooting](https://tx.codeplex.com/wikipage?title=Troubleshooting)