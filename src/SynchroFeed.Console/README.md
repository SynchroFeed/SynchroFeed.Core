# SynchroFeed Console
*SynchroFeed* is an extensible framework written in Microsoft .NET C# for integrating with a Nuget-like feed. 
The *Console* program is a command line program that exposes the features of the *SynchroFeed* *Library* through 
configuration.

The *Console* program is driven by a JSON configuration file that supports the following configuration:

- Configuring feeds leveraging plugins such as *Nuget*, *Proget* and *Directory* plugins that implement the IRepository interface
- Configuring *Actions* using plugins such as *Sync* and *Process* that implement the IAction interface
- Adding *Commands* to *Actions* to process or validate packages processed by the *Action*. Plugins such as *Catalog* and *ApplicationIs64bit* and others that implement the *ICommand* interface can be associated to an *Action* and chained together.
- Adding *Observers* to *Actions* to handle processing and validation events such as sending a message to Slack.

The following documents the command line and configuration options for the *Console* program.

## Command Line Options
The *Console* is a command line program that supports a couple command line arguments:

```
SynchroFeed.Console.exe [-c|--config <configFilename>] [action...] [-?|-h|--help]
```

The following table documents the command line arguments.

| Switch                     | Description 
|----------------------------|-------------
| `-c | --config <config>` | Specifies the configuration file that *SynchroFeed* will use. If none is specified, `app.json` will be used. The filename follows the switch. 
| `action`        | Specifies the name of one or more actions contained in the configuration file to run even if that action is disabled. If no action is specified, then all enabled actions within the configuration file are executed.
| `-? | -h | --help`       | Displays the command line options help

If no command line arguments are passed, then *Console* will look for a file named `app.json` in the current directory. If found,
all enabled *Actions* will be processed in the order they appear in the configuration file.

## Configuration
All of the configuration for the *Console* program is contained in a JSON file. The default name for the configuration file is
`app.json` but it can be overwritten through a command line argument. The *Console* project has an example of an [app.json](App.json) 
file.

### FeedSettings
The root node of the JSON object is the `FeedSettings` node. This node contains the configuration for other nodes like 
`SettingsGroups`, `Feeds` and `Actions`.

###### FeedSettings Configuration Example
```
{
  "FeedSettings": {
    "SettingsGroups": {
        ...
    },
    "Feeds": [
        ...
    ],
    "Actions": [
        ...
    ]
  }
}
```

### SettingsGroups and Settings
The `SettingsGroups` is an optional dictionary node that contains a collection of named settings. These names can be referenced in the
settings configuration of *Feeds*, *Actions*, *Commands* and *Observers*. Since many times settings are shared with other nodes,
a collection of settings can be easily shared by referencing a name of a `SettingsGroups`.

The table below details the properties on a `SettingsGroups`.

| Property          | Required? | Description
|-------------------|:---------:|------------
| SettingsGroupName | Yes       | This is the name to give to the collection of settings name/value pairs contained within the SettingsGroup
| Name              | Yes       | The name of the setting
| Value             | Yes       | The value of the setting associated with the Name

The `Name` used in the setting depends on the settings supported by the consuming object. Please refer to the individual documentation
for the valid options for `Name`.

###### SettingsGroups Configuration Example
```
    "SettingsGroups": {
      "MySettingsGroupName1": {
        "Name1": "Value1",
        "Name2": true,
        "Name3": "Value3",
        "Name4": "Value4"
      },
      "feed.mycompany.com": {
        "ApiKey": "MyApiKey",
        "DeleteFromTarget": true,
        "Username": "MyUsername",
        "Password": "MyPassword"
      },
      "catalogDatabaseSettings": {
        "ConnectionStringName": "SynchroFeed_Catalog",
        "CreateDatabaseIfNotFound": "true"
      },
      "Hipchat": {
        "Url": "https://api.hipchat.com/v2/room/123456/notification?auth_token=myhipchattoken",
        "MessageTemplate-ActionStarted": "{\"message_format\": \"text\",\"message\": \"Starting {Action.ActionSettings.Name}\"}",
        "MessageTemplate-ActionCompleted": "{\"message_format\": \"text\",\"message\": \"Completed {Action.ActionSettings.Name}\"}"
      },
      "Slack": {
        "Url": "https://hooks.slack.com/services/T12345678/SomeOtherToken",
        "MessageTemplate-ActionStarted": "{\"attachments\": [{\"fallback\": \"Starting {Action.ActionSettings.Name}\",\"color\": \"good\",\"pretext\": \"Package Validation\",\"title\": \"Starting {Action.ActionSettings.Name}\"}]}",
        "MessageTemplate-ActionCompleted": "{\"attachments\": [{\"fallback\": \"Completed {Action.ActionSettings.Name}\",\"color\": \"good\",\"pretext\": \"Package Validation\",\"title\": \"Completed {Action.ActionSettings.Name}\"}]}"
      }
    }
```

### Feeds
The `Feeds` is a required node that contains a collection of one or more `Feed`. A feed is represented in *SynchroFeed* as a *Repository*. 
*Nuget*, *Proget* and *Directories* are all examples of types of feed plugins that come with the base *SynchroFeed* release.

The table below details the properties on a `Feed`.

| Property | Required? | Description
|----------|-----------|------------
| Name     | Yes       | This is the name of the feed. The name is used to configure the source and/or target feed on an *Action* to associate a feed with the *Action*.
| Type     | Yes       | The type of feed to configure. This value will match the name of an *Plugin Type Name* for a *Repository* such as *Nuget*, *Proget* or *Directory*. The value is case insensitive.
| SettingsGroup | Optional | The value is the name of the associated *SettingGroup* from the `SettingsGroups` collection. If this node exists and has a value that matches a name in the `SettingsGroups`, the settings under that group are added to the settings collection for this feed.
| Settings | No        | This is a collection of name/value pairs that are used to configure the specific plugin. Refer to the plugin documentation for the names of the settings to put in this collection.

#### Note
If a `SettingsGroup` is configured, then the settings from the `SettingsGroup` are added to the `Settings` collection unless a 
setting with the same name exists. In other words, settings in the `Settings` collection supercedes a setting with the same from
the `SettingsGroup`.

###### Feeds Configuration Example
```
"Feeds": [
    {
    "Name": "progetfeed.mycompany.com",
    "Type": "proget",
    "SettingsGroup": "feed.mycompany.com",
    "Settings": {
        "Uri": "https://feed.mycompany.com/nuget/myprogetfeed/"
    }
    },
    {
    "Name": "feed.local",
    "Type": "Directory",
    "Settings": {
        "Uri": "C:\\Temp\\repo\\feed.local"
    }
    }
]
```

### Actions
The `Actions` is a required node that contains a collection of one or more `Action`. *Process* and *Sync* are examples of two
action plugins that come with the base *SynchroFeed* release. 

The table below details the settings that are common for an `Action`. Refer to the plugin documentation for details on what settings
are available for a particular *Action* plugin.

| Property | Required? | Description
|----------|-----------|------------
| Name     | Yes       | This is the name of the *Action*. The name can be passed as a command line argument to *Console* to run only the *Action* that matches that name.
| Type     | Yes       | The type of *Action* to configure. This value will match the name of an *Plugin Type Name* for an *Action* such as *Process* or *Sync*. The value is case insensitive.
| SourceFeed | Yes     | This configures the name of the feed to use as the source feed. This name must match a name in the `Feeds` configuration.
| Enabled  | Optional  | `Default=true` A boolean value that determines if the *Action* is enabled. A disabled *Action* will only be executed if passed as a command line argument. This is useful for testing *Actions*.
| OnlyLatestVersion | Optional | `Default=true` A boolean value that determines if only the latest version of the package is retrieved from the source feed. If this value is false, then all packages are retrieved from the source feed.
| IncludePrerelease | Optional | `Default=false` A boolean value that determines if prelease packages are retrieved from the source feed. If this value is false, then prerelease packages are not retrieved from the source feed.
| PackagesToIgnore  | Optional | This is a collection of package IDs that should be ignored. If a package ID in this collection exactly matches (case insensitive) a package ID on the source feed, that package will not be processed. |
| FailOnError       | Optional | ```Default=false``` This is a boolean value that determines whether processing should continue when an error has been encountered. True means to stop processing additional packages from the source feed and to end the action. False means to just log the error and continue processing. |
| Commands          | Optional | This is a collection of *Commands* that process each package from the source feed. A Process Action should have at least one *Command* otherwise each package will be pulled from the source feed but there is no command to process the package. See the *Commands* section below for details. |
| Observers         | Optional | This is a collection of *Observers* that observe events that occur as actions and commands are executed. An observer can be used to do things like send a web post like a Slack channel, send an email or write to a database.  See the *Observers* section below for details. |
| Settings          | Optional | This is a collection of *Settings* that are specific to a particular action. Review the documentation for a specific *Action* for settings for that action. |
| SettingsGroup | Optional | The value is the name of the associated *SettingGroup* from the `SettingsGroups` collection. If this node exists and has a value that matches a name in the `SettingsGroups`, the settings under that group are added to the settings collection for this feed.

###### Actions Configuration Example
```
"Actions": [
    {
    "Name": "Sync progetfeed.mycompany.com to feed.local",
    "Type": "Sync",
    "SourceFeed": "progetfeed.mycompany.com",
    "TargetFeed": "feed.local",
    "OnlyLatestVersion": true,
    "Enabled": true,
    "Settings": {}
    },
    {
    "Name": "Catalog progetfeed.mycompany.com",
    "Type": "Process",
    "SourceFeed": "progetfeed.mycompany.com",
    "OnlyLatestVersion": false,
    "Enabled": false,
    "SettingsGroup": null,
    "Settings": {},
    "PackagesToIgnore": [],
    "Commands": [
        {
        "Type": "Catalog",
        "FailureAction": "Continue",
        "SettingsGroup": "catalogDatabaseSettings",
        "Settings": {}
        }
    ],
    "Observers": [
        {
        "Name": "WebPost",
        "SettingsGroup": "Slack",
        "Settings": {
            "MessageTemplate-ActionCommandSuccess-hipchat": "{\"color\":\"green\",\"message_format\":\"text\",\"message\":\"{Package.Id}v{Package.Version} in {Action.ActionSettings.SourceFeed}.{Message}\",\"card\":{\"style\":\"application\",\"url\":\"{Package.PackageUrl}\",\"format\":\"medium\",\"id\":\"{EventId}\",\"title\":\"{Package.Id}\",\"description\":\"{Message}\",\"icon\":{\"url\":\"https://goo.gl/QUNNA5\"},\"attributes\":[{\"label\":\"Version\",\"value\":{\"label\":\"{Package.Version}\",\"style\":\"lozenge-complete\"}},{\"label\":\"Feed\",\"value\":{\"label\":\"{Action.ActionSettings.SourceFeed}\"}}]}}",
            "MessageTemplate-ActionCommandFailed-hipchat": "{\"color\":\"red\",\"message_format\":\"text\",\"notify\":\"true\",\"message\":\"{Package.Id}v{Package.Version} in {Action.ActionSettings.SourceFeed}.{Message}\",\"card\":{\"style\":\"application\",\"url\":\"{Package.PackageUrl}\",\"format\":\"medium\",\"id\":\"{EventId}\",\"title\":\"{Package.Id}\",\"description\":\"{Message}\",\"icon\":{\"url\":\"https://goo.gl/QUNNA5\"},\"attributes\":[{\"label\":\"Version\",\"value\":{\"label\":\"{Package.Version}\",\"style\":\"lozenge-error\"}},{\"label\":\"Feed\",\"value\":{\"label\":\"{Action.ActionSettings.SourceFeed}\"}}]}}",
            "MessageTemplate-ActionCommandFailed": "{\"attachments\": [{\"fallback\": \"{Package.ID} contains a 32-bit application\",\"color\": \"danger\",\"pretext\": \"Package Validation\",\"title\": \"{Package.ID}\",\"title_link\": \"{Package.PackageUrl}\",\"text\": \"{Message}\",\"fields\": [{\"title\": \"Version\",\"value\": \"{Package.Version}\",\"short\": true},{\"title\": \"Feed\",\"value\": \"{Action.ActionSettings.SourceFeed}\",\"short\": true}]}]}"
        }
        }
    ]
    },
    {
    "Name": "Audit progetfeed.mycompany.com for 32-bit applications",
    "Type": "Process",
    "SourceFeed": "progetfeed.mycompany.com",
    "OnlyLatestVersion": true,
    "Enabled": true,
    "PackagesToIgnore": [
        "MyCompany.MyPackage1",
        "MyCompany.MyPackage2",
        "MyCompany.MyPackage3"
    ],
    "Settings": {
    },
    "Commands": [
        {
        "Type": "ApplicationIs64Bit"
        "FailureAction": "FailPackage",
        }
    ],
    "Observers": [
        {
        "Name": "WebPost",
        "SettingsGroup": "Slack",
        "Settings": {
            "MessageTemplate-ActionCommandSuccess-hipchat": "{\"color\":\"green\",\"message_format\":\"text\",\"message\":\"{Package.Id}v{Package.Version} in {Action.ActionSettings.SourceFeed}.{Message}\",\"card\":{\"style\":\"application\",\"url\":\"{Package.PackageUrl}\",\"format\":\"medium\",\"id\":\"{EventId}\",\"title\":\"{Package.Id}\",\"description\":\"{Message}\",\"icon\":{\"url\":\"https://goo.gl/QUNNA5\"},\"attributes\":[{\"label\":\"Version\",\"value\":{\"label\":\"{Package.Version}\",\"style\":\"lozenge-complete\"}},{\"label\":\"Feed\",\"value\":{\"label\":\"{Action.ActionSettings.SourceFeed}\"}}]}}",
            "MessageTemplate-ActionCommandFailed-hipchat": "{\"color\":\"red\",\"message_format\":\"text\",\"notify\":\"true\",\"message\":\"{Package.Id}v{Package.Version} in {Action.ActionSettings.SourceFeed}.{Message}\",\"card\":{\"style\":\"application\",\"url\":\"{Package.PackageUrl}\",\"format\":\"medium\",\"id\":\"{EventId}\",\"title\":\"{Package.Id}\",\"description\":\"{Message}\",\"icon\":{\"url\":\"https://goo.gl/QUNNA5\"},\"attributes\":[{\"label\":\"Version\",\"value\":{\"label\":\"{Package.Version}\",\"style\":\"lozenge-error\"}},{\"label\":\"Feed\",\"value\":{\"label\":\"{Action.ActionSettings.SourceFeed}\"}}]}}",
            "MessageTemplate-ActionCommandFailed": "{\"attachments\": [{\"fallback\": \"{Package.ID} contains a 32-bit application\",\"color\": \"danger\",\"pretext\": \"Package Validation\",\"title\": \"{Package.ID}\",\"title_link\": \"{Package.PackageUrl}\",\"text\": \"{Message}\",\"fields\": [{\"title\": \"Version\",\"value\": \"{Package.Version}\",\"short\": true},{\"title\": \"Feed\",\"value\": \"{Action.ActionSettings.SourceFeed}\",\"short\": true}]}]}"
        }
        }
    ]
    }
]
```

### Commands
The `Commands` is an optional node that contains a collection of one or more `Command`. *Catalog* and *ApplicationIs64bit* are 
examples of two *Command* plugins that come with the base *SynchroFeed* release. Each *Command* is called in the order they have
been defined in the configuration. If a *Command* returns a failure, the `FailureAction` will determine whether additional *Commands*
are called to process the package.

The table below details the settings that are common for a `Command`. Refer to the plugin documentation for details on what settings
are available for a particular *Command* plugin.

| Property | Required? | Description
|----------|-----------|------------
| Type     | Yes       | The type of *Command* to configure. This value will match the name of an *Plugin Type Name* for an *Command* such as *Catalog* or *ApplicationIs64bit*. The value is case insensitive.
| FailureAction | Optional | `Default=Continue` An enumeration that determines what action should occur when the command has marked the package as failed. The options are: `Continue`, `FailPackage` and `FailAction`.  `Continue` is the default and it means to continue processing any subsequent Commands and Actions. `FailPackage` means to fail the package being processed and don't call any subsequent Commands but continue with the Action. `FailAction` means to fail the package being processed and stop the Action from any further processing of packages.
| Settings          | Optional | This is a collection of *Settings* that are specific to a particular action. Review the documentation for a specific *Action* for settings for that action. |
| SettingsGroup | Optional | The value is the name of the associated *SettingGroup* from the `SettingsGroups` collection. If this node exists and has a value that matches a name in the `SettingsGroups`, the settings under that group are added to the settings collection for this feed.

###### Commands Configuration Example
```
"Actions": [
    {
    "Name": "Catalog progetfeed.mycompany.com",
    "Type": "Process",
    "SourceFeed": "progetfeed.mycompany.com",
    "Commands": [
        {
        "Type": "Catalog",
        "FailureAction": "Continue",
        "SettingsGroup": "catalogDatabaseSettings",
        "Settings": {}
        }
    ]
    "Observers": [
        {
        "Name": "WebPost",
        "SettingsGroup": "Slack",
        "Settings": {
            "MessageTemplate-ActionCommandSuccess-hipchat": "{\"color\":\"green\",\"message_format\":\"text\",\"message\":\"{Package.Id}v{Package.Version} in {Action.ActionSettings.SourceFeed}.{Message}\",\"card\":{\"style\":\"application\",\"url\":\"{Package.PackageUrl}\",\"format\":\"medium\",\"id\":\"{EventId}\",\"title\":\"{Package.Id}\",\"description\":\"{Message}\",\"icon\":{\"url\":\"https://goo.gl/QUNNA5\"},\"attributes\":[{\"label\":\"Version\",\"value\":{\"label\":\"{Package.Version}\",\"style\":\"lozenge-complete\"}},{\"label\":\"Feed\",\"value\":{\"label\":\"{Action.ActionSettings.SourceFeed}\"}}]}}",
            "MessageTemplate-ActionCommandFailed-hipchat": "{\"color\":\"red\",\"message_format\":\"text\",\"notify\":\"true\",\"message\":\"{Package.Id}v{Package.Version} in {Action.ActionSettings.SourceFeed}.{Message}\",\"card\":{\"style\":\"application\",\"url\":\"{Package.PackageUrl}\",\"format\":\"medium\",\"id\":\"{EventId}\",\"title\":\"{Package.Id}\",\"description\":\"{Message}\",\"icon\":{\"url\":\"https://goo.gl/QUNNA5\"},\"attributes\":[{\"label\":\"Version\",\"value\":{\"label\":\"{Package.Version}\",\"style\":\"lozenge-error\"}},{\"label\":\"Feed\",\"value\":{\"label\":\"{Action.ActionSettings.SourceFeed}\"}}]}}",
            "MessageTemplate-ActionCommandFailed": "{\"attachments\": [{\"fallback\": \"{Package.ID} contains a 32-bit application\",\"color\": \"danger\",\"pretext\": \"Package Validation\",\"title\": \"{Package.ID}\",\"title_link\": \"{Package.PackageUrl}\",\"text\": \"{Message}\",\"fields\": [{\"title\": \"Version\",\"value\": \"{Package.Version}\",\"short\": true},{\"title\": \"Feed\",\"value\": \"{Action.ActionSettings.SourceFeed}\",\"short\": true}]}]}"
        }
        }
    ]
    }
]
```


### Observers
The `Observers` is an optional node that contains a collection of one or more `Observer`. *WebPost* is an
examples of an *Observer* plugin that comes with the base *SynchroFeed* release. When SynchroFeed raises an event, each *Observer* 
is called in the order they have been defined in the configuration.

The table below details the settings that are common for an `Observer`. Refer to the plugin documentation for details on what settings
are available for a particular *Observer* plugin.

| Property | Required? | Description
|----------|-----------|------------
| Type     | Yes       | The type of *Observer* to configure. This value will match the name of an *Plugin Type Name* for an *Observer* such as *WebPost*. The value is case insensitive.
| Settings          | Optional | This is a collection of *Settings* that are specific to a particular action. Review the documentation for a specific *Action* for settings for that action. |
| SettingsGroup | Optional | The value is the name of the associated *SettingGroup* from the `SettingsGroups` collection. If this node exists and has a value that matches a name in the `SettingsGroups`, the settings under that group are added to the settings collection for this feed.

###### Commands Configuration Example
```
"Actions": [
      {
        "Name": "Audit progetfeed.mycompany.com for 32-bit applications",
        "Type": "Process",
        "SourceFeed": "progetfeed.mycompany.com",
        "Commands": [
          {
            "Type": "ApplicationIs64Bit",
            "FailureAction": "FailPackage"
          }
        ],
        "Observers": [
          {
            "Name": "WebPost",
            "SettingsGroup": "Slack",
            "Settings": {
              "MessageTemplate-ActionCommandFailed": "{\"attachments\": [{\"fallback\": \"{Package.ID} contains a 32-bit application\",\"color\": \"danger\",\"pretext\": \"Package Validation\",\"title\": \"{Package.ID}\",\"title_link\": \"{Package.PackageUrl}\",\"text\": \"{Message}\",\"fields\": [{\"title\": \"Version\",\"value\": \"{Package.Version}\",\"short\": true},{\"title\": \"Feed\",\"value\": \"{Action.ActionSettings.SourceFeed}\",\"short\": true}]}]}"
            }
          }
        ]
      }
]
```

