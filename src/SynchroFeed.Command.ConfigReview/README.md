# SynchroFeed ConfigReview Command

The *ConfigReview Command* is an addon to the *SynchroFeed* framework that takes a package as input and evaluates the configuration within the package to ensure it is proper.

## Plugin Type Name

The name for the *ConfigReview* command to use in configuration is **ConfigReview**.

## Configuration

The following documents the configuration for the command:

| Setting Name             | Description |
|--------------------------|-------------|
| FailureAction            | An enumeration that determines what action should occur when the command has marked the package as failed. The options are: `Continue`, `FailPackage` and `FailAction`.  `Continue` means to continue processing any subsequent Commands and Actions. `FailPackage` means to fail the package being processed and don't call any subsequent Commands but continue with the Action. `FailAction` means to fail the package being processed and stop the Action from any further processing of packages. Default: `Continue` |
| PackageIdRegex           | An optional Regex to determine which packages are reviewed by the command (case will be ignored).  Default: `.*` |

For any other settings that are supplied to the configuration, it will be interpreted as the name of the AppSetting to check within the configuration.  If the name is prefixed with a `+`, then the setting MUST be present.  The value will be interpreted as a Regex (with case-insensitivity) that, if it matches, will result in an error.

### Example Log Level Settings:

| Key|Value|
|----|-----|
|Setting1|.*
|+Setting2|false