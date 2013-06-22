# Tx (LINQ to Logs and Traces)

Tx is set of code samples showing how to use LINQ to events, such as:

* **Real-Time standing queries:**. E.g. producing a histogram every second how many bytes were send/received over TCP per IP address. 
* **Queries on past history from trace/log files:** E.g. from past trace of IIS find the slow requests by correlating "begin" and "end" events.

The initial set of supported technologies is:

* Event Tracing for Windows (ETW)
* Windows Event Logs
* W3C Logs from IIS
* Performance counter captures (.blg, ,csv, .tsv)
* SQL Server Extended Events (XEvent)

At its very core the Tx approach represents design pattern for building parsers.
I.e. extending it is easy.

Example of the extensibility is trace in the ULS (Unified Logging Service) format 


 






