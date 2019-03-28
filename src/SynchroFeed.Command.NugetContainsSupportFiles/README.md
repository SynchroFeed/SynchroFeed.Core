# SynchroFeed NugetContainsSupportFiles Command

The *NugetContainsSupportFiles Command* is an addon to the *SynchroFeed* framework that takes a package as input and evaluates the assemblies and executables within that package to ensure they contain support files (i.e. .xml and .pdb files) for the assemblies contained within.

The assemblies that are examined can be limited by a regex against the assembly's file name (excluding the extension, which automatically be .dll).

## Plugin Type Name

The name for the *NugetContainsSupportFiles* command to use in configuration is **NugetContainsSupportFiles**.

## Configuration

The following documents the configuration for the command:

| Setting Name             | Description |
|--------------------------|-------------|
| CheckForPdb              | A boolean flag to indicate that pdb files (i.e. *.pdb) will be checked.  Default: `true`
| CheckForXml              | A boolean flag to indicate that comment files (i.e. *.xml) will be checked.  Default: `true`
| FailureAction            | An enumeration that determines what action should occur when the command has marked the package as failed. The options are: `Continue`, `FailPackage` and `FailAction`.  `Continue` means to continue processing any subsequent Commands and Actions. `FailPackage` means to fail the package being processed and don't call any subsequent Commands but continue with the Action. `FailAction` means to fail the package being processed and stop the Action from any further processing of packages. Default: `Continue` |
| FileRegex                | An optional Regex to determine which files are reviewed by the command (case will be ignored).  The command will automatically only review assemblies (i.e. dlls), so the regex supplied will only apply to the filename without the extension.  If a value is supplied, you may leverage the placehold of "`~PackageId~`" to indicate the package's Id.  Default: `.*` |
| PackageIdRegex           | An optional Regex to determine which packages are reviewed by the command (case will be ignored).  Default: `.*` |