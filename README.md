# SynchroFeed
[![Slack](https://a.slack-edge.com/66f9/img/landing/header_logo_sprite.png)](https://synchrofeed.slack.com/)

[![Build status](https://ci.appveyor.com/api/projects/status/909aq2v1cuqijsal?svg=true)](https://ci.appveyor.com/project/bvandehey/synchrofeed)
[![NuGet version](https://badge.fury.io/nu/SyncroFeed.Library.svg)](https://badge.fury.io/nu/SynchroFeed.Library)

*SynchroFeed* is an extensible framework written in Microsoft .NET C# for integrating with a Nuget-like feed to perform 
use cases like the following:

### Synchronize packages on a source feed to a target feed
The *Sync* *Action* is useful when there is a need to have another feed that mirrors another feed. For example, perhaps your company
has an internal feed that isn't exposed externally but there is a need for your external customers
to access packages on the feed. Rather than creating a route to the internal server, an external server can be stood up
and *SynchroFeed* can be used to synchronize packages from the internal feed to the external feed.

Another possible use case for synchronizing is rather than have your build server access your feed directly, a process
can be created within *SynchroFeed* to synchronize a nuget feed into a folder directly on the build server. This can 
reduce load on your feed server as well as improve build performance.

### Analyze packages on a source feed
The *Process* *Action* and its associated *Commands* are useful to analyze and validate packages on a feed. For example, if your 
company has a hard time enforcing consistency of packages created by your development teams, *SynchroFeed* can be used to 
analyze packages on a feed to make sure they conform to your standards. For example, perhaps your environment only allows 64-bit 
assemblies to be deployed. A command can be created that checks packages for their bitness and notify the developer that the 
package is out of compliance before it is deployed.

Nearly anything can be validated by creating commands to extend SynchroFeed. For example, 
- Package naming conventions can be enforced
- Configuration files can be validated to make sure things like logging settings are at the correct default level
- Validate the Chocolatey scripts contained within a Chocolatey nuget package
- Validate that a package contains the expected PDB and XML files.

### Catalog package versions and their dependencies in a database
The *Catalog* *Command* analyzes a package for its version information and included assemblies and catalogs the information
in a Entity Framework database. This is useful to easily query a database to find package dependencies that might be necessary
to identify packages that need a new release to include a critical update of a dependent assembly.

# SynchroFeed Terminology
*SynchroFeed* is extensible by building on top of the functionality, interfaces and plugin extensibility contained in the
*SynchroFeed* *Library*. This extensibility allows the creating of plugons that extend *Repository*, *Action*, *Command* 
and *Observers*. The following explains these terms and introduces the base plugons.

## Library
The *SynchroFeed* *Library* contains the based functionality, interfaces and plugin extensibility for *SynchroFeed*. Most of the 
functionality of *SynchroFeed* is provided through plugins. More details on the *Library* cab be found in the
[SynchroFeed.Library](src/SynchroFeed.Library/README.md) project.

## Repository
A *Repository* is an abstraction of a feed that supports fetching, adding and deleting Nuget packages on that feed. Three base 
repositories have been included with *SynchroFeed*: Nuget, Proget and Directory. 

### Nuget Repository (Plugin)
The Nuget repository is built on top of the Microsoft.Data.Services.Client library that provides a LINQ-enabled 
client API for issuing OData queries and consuming OData payloads. Since Nuget feeds expose an OData API, this is an efficient
way to access the feed. More details on the *Nuget* *Repository* can be found in the 
[SynchroFeed.Repository.Nuget](src/SynchroFeed.Repository.Nuget/README.md) project.

### Proget Repository (Plugin)
The Proget repository is built on top of the Nuget repository but overrides how URLs are formed to match the format of
Proget URLs. More details on the *Proget* *Repository* can be found in the 
[SynchroFeed.Repository.Proget](src/SynchroFeed.Repository.Proget/README.md) project.

### Directory Repository (Plugin)
The Directory repository enables a folder to be used as both a source and destination feed which is useful for synching a
feed to a folder. More details on the *Directory* *Repository* can be found in the 
[SynchroFeed.Repository.Directory](src/SynchroFeed.Repository.Directory/README.md) project.

*SynchroFeed* can be extended with new repositories by implementing the `SynchroFeed.Library.Repository.IRepository<Package>` 
interface. See the [SynchroFeed.Repository.Nuget](src/SynchroFeed.Repository.Nuget/README.md) project for an example of how 
to create a new repository.

## Action
An *Action* defines what to do with a feed and its packages. There are currently two actions: Sync and Process.

### Sync Action (Plugin)
The *Sync* *Action* takes a source repository and synchronizes the packages on the source repository to a target repository.
More details on the *Sync* *Action* can be found in the [SynchroFeed.Action.Sync](src/SynchroFeed.Action.Sync/README.md) project.

### Process Action (Plugin)
The *Process* *Action* takes a source repository and processes the packages through the configured *Commands*. 
More details on the *Process* *Action* can be found in the [SynchroFeed.Action.Process](src/SynchroFeed.Action.Process/README.md) project.

*SynchroFeed* can be extended with additional actions by implementing the `SynchroFeed.Library.Action.IAction` interface. 
See the [SynchroFeed.Action.Sync](src/SynchroFeed.Action.Sync/README.md) or [SynchroFeed.Action.Process](src/SynchroFeed.Action.Process/README.md) 
project for an example of how to create a new Action.

## Command
A *Command* processes or validates a package. Zero, one or more *Commands* are associated with an *Action*. Each package 
that is processed by the action is passed to the associated *Command(s)* for additional processing or validation. The 
*Command* can do things many tasks like validate attributes of the package, reflect its embedded assemblies and/or write 
to a database. A [CommandResult](src/SynchroFeed.Library/Command/CommandResult.cs) is returned from the *Command* which 
the process uses to determine whether to continue processing the package or skip the package. The possibilities of commands 
are endless and it is expected to be the most active plugins to *SynchroFeed*. There are currently only two 
*Commands*: Catalog and ApplicationIs64bit.

### Catalog (Plugin)
The *Catalog* *Command* writes package and assembly metadata to an Entity Framework database. More details on the *Catalog*
*Command* can be found in the [SynchroFeed.Command.Catalog](src/SynchroFeed.Command.Catalog/README.md) project.

### ApplicationIs64bit (Plugin)
The *ApplicationIs64bit* *Command* evaluates the assemblies and executables within a package for their bitness to validate
that they are all either x64 or AnyCPU and not 32-bit preferred. It is useful to enforce everything in a package is 64-bit to 
eliminate having to manage both 32 bit and 64 bit machine.config configuration. More details on the *ApplicationIs64bit* 
*Command* can be found in the [SynchroFeed.Command.ApplicationIs64bit](src/SynchroFeed.Command.ApplicationIs64bit/README.md) 
project.

## Observer
An *Observer* listens to events raised by the *SynchroFeed* framework and can do things with the event such as sending a webpost
to a Slack room. New listeners can be written to send emails, SMS, PagerDuty or whatever task is necessary to raise visibility
to processes running or possible validation issues with packages. Currently there is only one *Observer*: WebPost.

### WebPost (Plugin)
The *WebPost* *Observer* listens to events raised by the *SynchroFeed* framework and if there is a template that matches the name of
the event, then a post with the template content is sent to the configured URL. A template also supports binding data with
tags in the template to personalize the message. More details on the *WebPost* *Observer* can be found in the [SynchroFeed.ActionObserver.WebPost](src/SynchroFeed.ActionObserver.WebPost/README.md) 
project.

## Console Program
A *Console* program has been written that exposes the features of the *SynchroFeed* *Library* through configuration. More details 
on the *Console* program can be found in the [SynchroFeed.Console](src/SynchroFeed.Console/README.md) 
project.

# License
Licensed under the terms of the [MIT License](./LICENSE).