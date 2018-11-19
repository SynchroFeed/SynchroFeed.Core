# SynchroFeed Sync Action
*SynchroFeed* is an extensible framework written in Microsoft .NET C# for integrating with a Nuget-like feed. 
An *Action* defines what to do with the feed and its packages. The *Sync* *Action* takes a source repository 
and synchronizes the packages on the source repository to a target repository. A *Sync* *Action* can also have
*Commands* configured that preprocess a package before syncing. Depending on the result of the *Command*, the
package might not be synced with the target feed.

## Plugin Type Name
The name for the *Sync* action to use in configuration is **Sync**.

## Configuration
The follow table documents the configurable options for the *Sync* *Action* to customize its behavior:

| Setting Name      | Description |
| ----------------- | ----------- |
| SourceFeed        | Configures the source feed that is used to determine what packages to sync |
| TargetFeed        | Configures the target feed that is used to mirror the packages on source feed |
| OnlyLatestVersion | ```Default=True``` This is a boolean value that determines whether all packages on the source feed are synchronized to the target feed. If this value is true, only the latest version on the source feed is synchronized.  |
| IncludePrerelease | ```Default=False``` This is a boolean value that determines whether prerelease versions on the source feed are synchronized to the target feed. If this value is true, all packages, including prerelease packages, are synced to the target feed. |
| PackagesToIgnore  | This is a collection of package IDs that should be ignored. If a package ID in this collection exactly matches (case insensitive) a package ID on the source feed, that package will not be synchronized to the target feed. |
| FailOnError       | ```Default=False``` This is a boolean value that determines whether processing should continue when an error has been encountered. True means to stop processing additional packages from the source feed and to end the action. False means to just log the error and continue processing. |
| Commands          | This is a collection of *Commands* that process each package from the source feed before it is synchronized to the target feed. The results of the command will determine if the package gets synchronized to the target feed. |
| Observers         | This is a collection of *Observers* that observe events that occur as actions and commands are executed. An observer can be used to do things like send a web post like a Slack channel, send an email or write to a database. |
| Settings          | This is a collection of *Settings* that are specific to a particular action. Review the documentation for a specific *Action* for settings for that action. |
