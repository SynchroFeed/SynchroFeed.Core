# SynchroFeed Directory Repository
*SynchroFeed* is an extensible framework written in Microsoft .NET C# for integrating with a Nuget-like feed. A *Repository* 
is an abstraction of a feed that supports fetching, adding and deleting packages on that feed. The *Directory* repository 
enables a folder to be used as both a source and destination feed which is useful for synching a feed to a folder.

## Plugin Type Name
The name for the *Directory* repository to use in configuration is **Directory**.

## Configuration
The follow table documents the configurable options for the *Directory* *Repository*.

| Setting Name      | Description |
| ----------------- | ----------- |
| Name              | The name to give the repository. This typicall matches the name of the Nuget feed like nuget.org or maybe choco.dev for an internal development feed containing Chocolatey packages ready for testing. |
| Uri               | The URI to the directory. For example: C:\repo\feed.local. |
