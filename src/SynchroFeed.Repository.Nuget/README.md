# SynchroFeed Nuget Repository
*SynchroFeed* is an extensible framework written in Microsoft .NET C# for integrating with a Nuget-like feed. A *Repository* 
is an abstraction of a feed that supports fetching, adding and deleting packages on that feed. The Nuget repository is built 
on top of the Microsoft.Data.Services.Client library that provides a LINQ-enabled client API for issuing OData queries and 
consuming OData payloads. Since Nuget feeds expose an OData API, this is an efficient way to access the feed.

## Plugin Type Name
The name for the *Nuget* repository to use in configuration is **Nuget**.

## Configuration
The follow table documents the configurable options for the *Nuget* *Repository*.

| Setting Name      | Description |
| ----------------- | ----------- |
| Name              | The name to give the repository. This typicall matches the name of the Nuget feed like nuget.org or maybe choco.dev for an internal development feed containing Chocolatey packages ready for testing. |
| Uri               | The URI to the Nuget feed. For nuget.org, this would be https://www.nuget.org/api/v2. |
| ApiKey            | The API key to use to access the feed if one is required. |
| Username          | The Username to use to access the feed if one is required. |
| Password          | The Password to use to access the feed if one is required. |
| Proxy             | If you need to route through a proxy server or if you need to troubleshoot using a tool like Fiddler or Charles, configure this setting with a proper Uri that can be passed to the [WebProxy](https://docs.microsoft.com/en-us/dotnet/api/system.net.webproxy.-ctor?view=netframework-4.7.2#System_Net_WebProxy__ctor_System_String_) class. |
