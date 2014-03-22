# Synthetic Performance Counters

This sample illustrates how to use ETW events to produce "synthetic performance counters" defined as Rx queries.

The benefits of this approach compared to real performance counters (the one you see in PerfMon) are:

- xcopy deployment: no need for creation of counter categories (.Net) or deploying counter manifests via lodctr. No registry changes etc. 
- The "names" of the counters can be derived from the event payload
- Queries are not restricted to one-dimensional hierarchy

## Projects in the solution

You can find the solution in Samples\SyntheticCounters in the source code

Here is a list of projects:

* [DemoUI](DemoUI/TcpSyntheticCounters.cs) - simple WinForms application that hosts the query
* [IE Automation](IEAutomation/Program.cs) - tool to make Internet Explorer hit some web pages and thus cause TCP traffic
* [SynCtr](SynCtr/Program.cs) - console app hosting the same query, to test the CPU/memory impact without the overhead of drawing
* [TcpHog](TcpHog/Program.cs) - console app that generates high volume TCP trafic

## Running the demo
To understand the concepts, it is best to run the demo

* in the Properties of the IEAutomation project, enter list of url-s., For example: http://bing.com http://google.com
* start DemoUI, and wait a while to see if there is traffic when the machine is idle
* start IEAutomation

Here is a screenshot:

![SyntheticCounters.JPG](SyntheticCounters.JPG)

Here the top right is the experience of counters - per each IP address we see the trend of bytes received, aggregated over 1 sec window. 

## Performance Test

To test performance:

* Start SynCtr
* Start TcpHog
* Open PerfMon and add counters like in the picture below
TcpHog works by sending/receiving a burst of TCP traffic and then waiting for a while.

Here is example trend of events logged per second:

![SynCtr_Performance.JPG](SynCtr_Performance.JPG)

You can use the data collector above to capture this trend on your machine.
To do this, with PerfMon:

* Expand the node "Data Collector Sets" in the left tree
* Right click on User Defined, New, Data Collector Set
* Chose "Create From Template" and browse to Samples\SyntheticCounters\SynCtr\SynCtrDCS.xml
* Start the collector, do some experiments and then stop it
* Click on the little box (second icon in the menu) and browse for the .blg files that was created
