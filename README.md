# Tx (LINQ to Traces)?

Tx is set of code samples showing how to use LINQ to events, such as:

* **Real-Time standing queries:**. E.g. producing a histogram every second how many bytes were send/received over TCP per IP address. 
* **Queries on past history from trace/log files:** E.g. from past trace of IIS find the slow requests by correlating "begin" and "end" events.

The initial set of supported technologies is:

* Event Tracing for Windows (ETW)
* Windows Event Logs
* SQL Server Extended Events (XEvent)

We also ship the parsers from Tx on NuGet. These parsers demultiplex and transform real events and expose them as  IObservable sequences. 

From that point on you can use your own code or any technology available in C#. 


 






