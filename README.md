[![Nuget](https://img.shields.io/nuget/vpre/Tx.All.svg)](https://www.nuget.org/packages/Tx.All/)
[![Build status](https://ci.appveyor.com/api/projects/status/6n09scr9d74lb9vp?svg=true)](https://ci.appveyor.com/project/SergeyBaranchenkov/tx-6doyh)

# Tx (LINQ to Logs and Traces)
 
Tx allows you to do Language Integrated Query (LINQ) directly on raw event sources:

- ad-hoc query on past history in trace and log files
- standing queries on real-time feeds, such as Event Tracing for Windows (ETW) sessions

The Tx approach is different than Databases, Hadoop, Splunk, Dapper, etc. which all require a stage of uploading before the events become available to queries.

Instead, mixing Reactive Extensions (Rx) and LINQ-to-Objects allows the query to be embedded anywhere including:

- In light-weight UI tools like [LINQPad](Source/Tx.LinqPad/Readme.md)  and [SvcPerf](http://svcperf.codeplex.com)
- On original source machines, such as [Synthetic Counters](Samples/SyntheticCounters/Readme.md)


The following picture shows the dependencies among the main Tx components:

![TxCodeMap.JPG](TxCodeMap.JPG)

Here:

- Dark green is .Net
- Light green is mature open source
- Light grey are framework components, also available on NuGet
- Darker gray are tool experiences that come with Tx
- White are samples

For more see the [documentation](Doc/Readme.md)

## Contributing

There are lots of ways to contribute to the project, and we appreciate our [contributors](Contributors.txt). We strongly welcome and encourage contributions to this project. Please read the [contributor's guide][ContribGuide]. If making a large change we request that you open an [issue][GitHubIssue] first. We follow the [Git Flow][GitFlow] approach to branching. 

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

[ContribGuide]: Doc/CONTRIBUTING.md
[GitFlow]: http://nvie.com/posts/a-successful-git-branching-model/
[GitHubIssue]: https://github.com/Microsoft/Tx/issues
