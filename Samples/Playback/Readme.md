# Playback Samples

The class **Playback** delivers the events from one or more trace/log files or real-time feeds in order of occurrence. 

Internally:

* Event occurrence records are transformed from their raw  format (e.g. native structures) to C# objects
* Multiple inputs (files or real-time feeds) are merged on timestamp

This [set of samples](Program.cs) illustrates what Playback does as component, without using much LINQ.

## Structured Mode
In this mode, 

* Lose analogy is that events of given type are like a "Table"
	* playback.GetObservable< T> is similar to dataContext.GetTable< T> in LINQ to SQL
* More precisely, IObservable is a **sequence** (i.e. order matters) 
	* This is the same distinction as IEnumerable v.s. SQL Table

Samples:

* Parsing single event type occurrences (GetObservable)
* Virtual Time as per event timestamps (VirtualTime)
* Parsing begin/end events (Get2Observables)
* Formatting the above events (Format2)
* Limitations of the structured mode (Count2) 

## Timeline Mode

This mode is useful to ask questions that are about the timeline of all events, regardless of type.

Samples: 

* Formatting all events as in EventVwr, TraceFmt, etc.(FormatAll)
* Counting all events (CountAll)
* Counting specific types (Count2And12)
* Counting accross different file formats (CountAllTwoFiles)
* Counting accross different event hierarchies (CountAcrossHierarchies)
* Counting in 5 sec Window in virtual time (Count5SecWindow)
 



