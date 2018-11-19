# SynchroFeed Proget Repository
*SynchroFeed* is an extensible framework written in Microsoft .NET C# for integrating with a Nuget-like feed. A *Repository* 
is an abstraction of a feed that supports fetching, adding and deleting packages on that feed. The *Proget* repository is built 
on top of the [Nuget](src/SynchroFeed.Repository.Nuget/README.md) repository which is built on top of the 
Microsoft.Data.Services.Client library.

## Plugin Type Name
The name for the *Proget* repository to use in configuration is **Proget**.

## Configuration
The configuration for a *Proget* repository is the same as for a *Nuget* respository. See the [*Nuget* configuration documentation](../SynchroFeed.Repository.Nuget/README.md) for details.
