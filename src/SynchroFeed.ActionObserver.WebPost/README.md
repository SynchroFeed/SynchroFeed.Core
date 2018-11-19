# SynchroFeed WebPost Observer
*SynchroFeed* is an extensible framework written in Microsoft .NET C# for integrating with a Nuget-like feed. 
An *Observer* listens to events raised by the SynchroFeed framework. The *WebPost* *Observer* listens to events 
raised by the SynchroFeed framework and if there is a template that matches the name of the event, then a post 
with the template content is sent to the configured URL. A template also supports binding data with tags in the 
template to personalize the message. 

## Plugin Type Name
The name for the *Observer* to use in configuration is **WebPost**.

## Configuration
The follow table documents the configurable options for the *Process* *Action* to customize its behavior:

| Setting Name      | Description |
| ----------------- | ----------- |
| Name              | A descriptive name to give to the *Observer* |
| Url               | The URL of the web post. The URL is typically provided by the application receiving the web post. For example, for Slack, the URL might look something like https://hooks.slack.com/services/T12345678/SomeOtherToken. |
| ContentType       | ```Default=application/json``` This is a string value that configured the content type of the web post. Typically this is application/json or application/xml depending on whether the application expects JSON or XML but check with the documentation of your web post to determine the correct value. |
| Settings          | The *Settings* collection contains the message templates of the interested events. If a setting exists with a name that matches the event name, then a web post will be sent for that event. See the Event table below for more details. |

### Events
The following table documents the events that *SynchroFeed* raises and the setting name to configure the message
template for that event. The setting name always take the form of MessageTemplate-{Event Name}.

| Event Name           | Template Setting Name                | Description |
|----------------------|--------------------------------------|-------------|
| ActionStarted        | MessageTemplate-ActionStarted        | The event used to denote the Action has started. |
| ActionCompleted      | MessageTemplate-ActionCompleted      | The event used to denote the Action has completed. |
| ActionFailed         | MessageTemplate-ActionFailed         | The event used to denote the package failed and the Action is being stopped from processing further packages. |
| ActionCommandSuccess | MessageTemplate-ActionCommandSuccess | The event used to denote the Action Command result was successful. |
| ActionCommandFailed  | MessageTemplate-ActionCommandFailed  | The event used to denote the Action Command result was a failure. |
| ActionPackageIgnored | MessageTemplate-ActionPackageIgnored | The event used to denote the Action is ignoring the package. |
| ActionPackageFailed  | MessageTemplate-ActionPackageFailed  | The event used to denote the package failed but the Action continues with the next package. |
| ActionPackageSuccess | MessageTemplate-ActionPackageSuccess | The event used to denote the package was successfully processed by the commands. |

### Template Property Binding
Templates can contain tags that get replaced with property values from the currently executing *Action*, *Command* and *Package*.
The object that gets bound to the template is an instance of the [IActionEvent](../../src/SynchroFeed.Library/Action/Observer/IActionEvent.cs).
For example, to bind to the Action Name, use the tag: `{Action.ActionSettings.Name}`. To bind to the Package ID, use the
tag: `{Package.ID}`. To bind to the message associated with the event, use the tag: `{Message}`.

### Example Slack Message Templates
Below are a couple examples of Slack message templates that post to a Slack channel.

`
"MessageTemplate-ActionStarted": "{\"attachments\": [{\"fallback\": \"Starting {Action.ActionSettings.Name}\",\"color\": \"good\",\"pretext\": \"Package Validation\",\"title\": \"Starting {Action.ActionSettings.Name}\"}]}"
`

`
"MessageTemplate-ActionCompleted": "{\"attachments\": [{\"fallback\": \"Completed {Action.ActionSettings.Name}\",\"color\": \"good\",\"pretext\": \"Package Validation\",\"title\": \"Completed {Action.ActionSettings.Name}\"}]}"
`

`
"MessageTemplate-ActionCommandFailed": "{\"attachments\": [{\"fallback\": \"{Package.ID} contains a 32-bit application\",\"color\": \"danger\",\"pretext\": \"Package Validation\",\"title\": \"{Package.ID}\",\"title_link\": \"{Package.PackageUrl}\",\"text\": \"{Message}\",\"fields\": [{\"title\": \"Version\",\"value\": \"{Package.Version}\",\"short\": true},{\"title\": \"Feed\",\"value\": \"{Action.ActionSettings.SourceFeed}\",\"short\": true}]}]}"
`