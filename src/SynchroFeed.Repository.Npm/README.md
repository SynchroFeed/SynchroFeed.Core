# SynchroFeed NPM Repository
*SynchroFeed* is an extensible framework written in Microsoft .NET C# for integrating with feeds. A *Repository* 
is an abstraction of a feed that supports fetching, adding and deleting packages on that feed. The NPM repository 
supports accessing packages in an NPM compatible feed.

## Plugin Type Name
The name for the *Npm* repository to use in configuration is **Npm**.

## Configuration
The follow table documents the configurable options for the *Npm* *Repository*.

| Setting Name      | Description |
| ----------------- | ----------- |
| Name              | The name to give the repository. This typicall matches the name of the NPM feed. |
| Uri               | The URI to the NPM feed. For NPM, this would be https://registry.npmjs.org/. |
| ApiKey            | The API key to use to access the feed if one is required. |
