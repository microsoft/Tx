# Troubleshooting Tx

First step is to [get the Tx  code and docs](GetTxCode.md) on your local machine

## 1.ETW Type Generation

Common problem is having some syntax error in the ETW manifest. This is unlikely with generated manifests from [EventSource](http://blogs.msdn.com/b/vancem/archive/2012/07/09/logging-your-own-etw-events-in-c-system-diagnostics-tracing-eventsource.aspx), but happens often with manually written manifests.

If you are using LINQPad, this will manifest as failure to create connection with exception saying something about XML error. This is because [type generation](TypeGeneration.md) is the first step Tx does.

To test just this, in Visual Studio:

* open Source\Tx.sln
* set EtwEventTypeGen as startup project
* in this project properties, Debug, Command line arguments, point to your manifest

Example:  /m:myManifest.man 

If you see .cs file generated as output, try adding it to some project and compile. Sometimes the generation succeeds but the produced code has syntax errors.

## 2.Reading and formatting event occurrences

Many people don't use Tx directly, but use [SvcPerf](http://svcperf.codeplex.com). The default view of SvcPerf reads all events and uses the "Time-line Mode" described in [Playback Features](PlaybackFeatures.md).

To troubleshoot Tx does read event occurences and can format them:

* Set the console application [TxFmt](../Source/TxFmt/Program.cs) as default project
* In the Properties\Debugging add some manifest and etl files as command line arguments
* Enable first-chance exceptions and run

This allows to debug all non-UI parts of Tx (type-generation, reading, formatting). It is useful to catch problems such as manifest that mismatches the event occurrences.

## 3.Troubleshooting the LINQPad Driver

In Visual Studio:

* open Source\Tx.sln
* set Tx.LinqPad as startup project
* in this project properties, Debug, set "Start external program"
* Browse for tx\References\LinqPad\LINQPad.exe 

Start debugging







