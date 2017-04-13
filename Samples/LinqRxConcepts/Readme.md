# LINQ and Rx concepts sample

This is introduction sample about the core concepts of Language INtegrated Query (LINQ) and Reactive Extensions (Rx.Net). It does not have references to Rx.Net as it exists today. Instead, it shows how and why IObservable was invented.

The core concepts in LINQ have order in which they were invented and build, which is illustrated in the [samples called in Main()](Program.cs). It is best to read the [animated slides](LinqRxConcepts.pptx), and then to go through the code by setting breakpoints.

###ExensionMethods
This allowed adding methods to classes that already exist, without changing the source code. LINQ is just one application, that adds methods like .Where(...) into any collection implementing IEnumerable.

###Functions vs. Anonymous Methods

Lambda expressions is feature of C# that allows definition of two new concepts:
* Anonymous functions, which produce result only based on arguments
* Anonymous methods, that can access variables not passed as arguments, and leave side-effects.

###LINQ to Objects
Classic LINQ can be used in two ways, based on developer preference:  
* comprehension syntax (ala SQL) 
* by building data-pipelines from functions (extension methods)

###LINQ Operators

This shows how to extend LINQ by implementing simple filter operator

### Rx Operators

The classic LINQ is all about collections - i.e. it assumes that data is stored (e.g. in-memory) **before** is can be queried. There are many implementations of LINQ on external storage like databases, XML, etc. which all use this assumption.

Reactive Extensions does the opposite:
- first, a query is build as pipeline of callback methods
- then, events occur and are **pushed** into the pipeline

In this approach events may not be stored at all. It is colloquially referred as "Standing Query" or "Complex Event Processing". 

Unlike the storage-and-query approach, the latency of Rx processing is microseconds, and there are many scale and noise-reduction advantages.


### Push Inside Pull

This is sample how to execute Push operator, in the context of Pull pipeline. 

 