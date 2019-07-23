# SynchroFeed ChocoLog4netReview Command

The *Log4netReview Command* is an addon to the *SynchroFeed* framework that takes a package as input and evaluates the Log4net configuration within the package to ensure it is configured properly.

## Plugin Type Name

The name for the *Log4netReview* command to use in configuration is **Log4netReview**.

## Configuration

The following documents the configuration for the command:

| Setting Name             | Description |
|--------------------------|-------------|
| ConversionPattern        | The expected ConversionPattern for any appenders found in the config.  If not supplied, the check is ignored.
| FailureAction            | An enumeration that determines what action should occur when the command has marked the package as failed. The options are: `Continue`, `FailPackage` and `FailAction`.  `Continue` means to continue processing any subsequent Commands and Actions. `FailPackage` means to fail the package being processed and don't call any subsequent Commands but continue with the Action. `FailAction` means to fail the package being processed and stop the Action from any further processing of packages. Default: `Continue` |
| PackageIdRegex           | An optional Regex to determine which packages are reviewed by the command (case will be ignored).  Default: `.*` |

For any other settings that are supplied to the configuration, it will be interpreted as an XPath statement to find an element with a level child element.  The value will be treated as a pipe-delimited list of valid log levels.

|Available Log Levels|
|-|
|Off|
|Fatal|
|Error|
|Warn|
|Info|
|Debug|
|Trace|
|All|

### Example Log Level Settings:

| Key|Value|
|----|-----|
|/configuration/log4net/root|OFF\|FATAL
|/configuration/log4net/logger[starts-with(@name, 'LoggerName')]|INFO\|TRACE