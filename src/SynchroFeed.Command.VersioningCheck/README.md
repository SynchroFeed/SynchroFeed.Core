# SynchroFeed VersioningCheck Command

The *VersioningCheck Command* is an addon to the *SynchroFeed* framework that takes a package as input and evaluates the assemblies and executables within that package to ensure their version matches the version of the package.

The versions of the dlls must match the package's version for the number of digits supplied for the package version.

For example, if the package version is v1.1.2, then the dlls can only be 1.1.2.X (where X can be any number).

While a package version of v5.2, then the dlls can be 5.2.X.X (where X can be any number).

## Plugin Type Name

The name for the *VersioningCheck* command to use in configuration is **VersioningCheck**.

## Configuration

The following documents the configuration for the command:

| Setting Name             | Description |
|--------------------------|-------------|
| FailureAction            | An enumeration that determines what action should occur when the command has marked the package as failed. The options are: `Continue`, `FailPackage` and `FailAction`.  `Continue` is the default and it means to continue processing any subsequent Commands and Actions. `FailPackage` means to fail the package being processed and don't call any subsequent Commands but continue with the Action. `FailAction` means to fail the package being processed and stop the Action from any further processing of packages. |
| FileRegex                | An optional Regex to determine which files are reviewed by the command (case will be ignored).  If a value is not supplied, all dlls and executables will be examined.  If a value is supplied, you may leverage the placehold of "`~PackageId~`" to indicate the package's Id. |
| PackageIdRegex           | An optional Regex to determine which packages are reviewed by the command (case will be ignored).  If a value is not supplied, the command will examine all packages. |