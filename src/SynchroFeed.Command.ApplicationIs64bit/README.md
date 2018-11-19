# SynchroFeed ApplicationIs64Bit Command
The *Catalog* *Command* is an addon to the *SynchroFeed* framework that takes a package as input and evaluates the assemblies 
and executables within that package for their bitness to validate that they are all either x64 or AnyCPU and not 32-bit preferred. 
It is useful to enforce everything in a package is 64-bit to eliminate having to manage both 32 bit and 64 bit machine.config 
configuration.

## Plugin Type Name
The name for the *ApplicationIs64Bit* command to use in configuration is **ApplicationIs64Bit**.

## Configuration
The following documents the configuration for the Catalog command.

| Setting Name             | Description |
|--------------------------|-------------|
| FailureAction            | An enumeration that determines what action should occur when the command has marked the package as failed. The options are: `Continue`, `FailPackage` and `FailAction`.  `Continue` is the default and it means to continue processing any subsequent Commands and Actions. `FailPackage` means to fail the package being processed and don't call any subsequent Commands but continue with the Action. `FailAction` means to fail the package being processed and stop the Action from any further processing of packages.
