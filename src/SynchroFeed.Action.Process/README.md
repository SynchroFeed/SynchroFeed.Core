# SynchroFeed Process Action
*SynchroFeed* is an extensible framework written in Microsoft .NET C# for integrating with a Nuget-like feed. 
An *Action* defines what to do with the feed and its packages. The *Process* *Action* takes a source repository 
and processes the packages through the configured *Commands*.

## Plugin Type Name
The name for the *Process* action to use in configuration is **Process**.

## Configuration
The follow table documents the configurable options for the *Process* *Action* to customize its behavior:

| Setting Name      | Description |
| ----------------- | ----------- |
| SourceFeed        | Configures the source feed that is used to determine what packages to process |
| OnlyLatestVersion | ```Default=True``` This is a boolean value that determines whether all packages on the source feed are processed. If this value is true, only the latest version on the source feed is processed.  |
| IncludePrerelease | ```Default=False``` This is a boolean value that determines whether prerelease versions on the source feed are processed. If this value is true, all packages, including prerelease packages, are processed. |
| PackagesToIgnore  | This is a collection of package IDs that should be ignored. If a package ID in this collection exactly matches (case insensitive) a package ID on the source feed, that package will not be processed. |
| FailOnError       | ```Default=False``` This is a boolean value that determines whether processing should continue when an error has been encountered. True means to stop processing additional packages from the source feed and to end the action. False means to just log the error and continue processing. |
| Commands          | This is a collection of *Commands* that process each package from the source feed. A Process Action should have at least one *Command* otherwise each package will be pulled from the source feed but there is no command to process the package. |
| Observers         | This is a collection of *Observers* that observe events that occur as actions and commands are executed. An observer can be used to do things like send a web post like a Slack channel, send an email or write to a database. |
| Settings          | This is a collection of *Settings* that are specific to a particular action. Review the documentation for a specific *Action* for settings for that action. |
