# Troubleshooting Tx

First step is to [get the Tx  code and docs](GetTxCode.md) on your local machine

## LINQPad Driver

In Visual Studio:

* open Source\Tx.sln
* set Tx.LinqPad as startup project
* in this project properties, Debug, 
** configure "Start external program": C:\git\tx\References\LinqPad\LINQPad.exe 

## ETW Type Generation

In Visual Studio:

* open Source\Tx.sln
* set EtwEventTypeGen as startup project
* in this project properties, Debug, Command line arguments, point to your manifest

Example:  /m:myManifest.man 

## Run-time reading + formatting (e.g. issues with SvnPerf)

Many people don't use Tx directly, but use [SvcPerf](http://svcperf.codeplex.com). The default view of SvcPerf reads all events and uses the "Timeline Mode" described in [Playback Features](PlaybackFeatures.md).

To troubleshoot Tx in a simpler way

* Set the console application [TxFmt](../Source/TxFmt/Program.cs) as default project
* In the Properties\Debugging add some manifest and etl files as command line arguments
* Enable first-chance exceptions and run

This allows to debug all non-UI parts of Tx (type-generation, reading, formatting)




