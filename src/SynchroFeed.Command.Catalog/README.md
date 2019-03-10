# SynchroFeed Catalog Command
The *Catalog* *Command* is an addon to the *SynchroFeed* framework that takes a package as input and
writes package and assembly metadata to an Entity Framework database. 

## Plugin Type Name
The name for the *Catalog* command to use in configuration is **Catalog**.

## Configuration
The following documents the configuration for the Catalog command.

| Setting Name             | Description |
|--------------------------|-------------|
| FailureAction            | An enumeration that determines what action should occur when the command has marked the package as failed. The options are: `Continue`, `FailPackage` and `FailAction`.  `Continue` is the default and it means to continue processing any subsequent Commands and Actions. `FailPackage` means to fail the package being processed and don't call any subsequent Commands but continue with the Action. `FailAction` means to fail the package being processed and stop the Action from any further processing of packages.
| ConnectionStringName     | The name of the database connection string to use for the Catalog. The name must be defined as a valid connection string within .NET config. |
| CreateDatabaseIfNotFound | ```Default=False``` This is a boolean value that determines whether the database should be created if it doesn't exist. |
| NormalizeRegEx           | A regular expression that can be used to remove unwanted values from an assembly name. For example, if your assembly has the version embedded in it like MyAssembly.1.2 but you want all of the assemblies with that name cataloged under the same assembly, the following regular expression can be used "(\\.\\d+)+". Whatever matches this regular expression is removed from the assembly name so in this example, the assembly name used for cataloging would be MyAssembly.

## Tables
The following documents the tables schema for the Catalog database. **Bold** column names are the primary keys for the table.

### Packages
The Packages table contains a row for each unique package name which is also called the package ID.

| Column Name        | Data Type            | Description |
|--------------------|----------------------|-------------|
| **PackageID**      | int                  | The package ID is the primary key for the package which is an autoincrementing integer value. |
| Name               | nvarchar(100)        | The case-insensitive package identifier, which must be unique within the repository the package resides in. This is sometimes referred to as the package ID within the nuget metadata. |
| Title              | nvarchar(200)        | A human-friendly title of the package, typically used in UI displays. |
| CreatedUtcDateTime | datetimeoffset       | A UTC datetime when the row was created in the database. |

### PackageEnvironments
The PackageEnvironments table is a one-to-many table to track the environments where a package 
was found. An environment is typically the name of the source feed where the package was retrieved.

| Column Name        | Data Type            | Description |
|--------------------|----------------------|-------------|
| **PackageID**      | int                  | The package ID is a foreign key to the Packages table. |
| **Name**           | nvarchar(100)        | The name of the environment the package was found. This is typically the name of the feed where the package was found. |
| CreatedUtcDateTime | datetimeoffset       | A UTC datetime when the row was created in the database. |


### PackageVersions
The PackageVersions table is a one-to-many table that contains a row for each version of a package found.

| Column Name          | Data Type            | Description |
|----------------------|----------------------|-------------|
| **PackageVersionID** | int                  | The package version ID an autoincrementing integer value that is the primary key for this version of a package. |
| PackageID            | int                  | The package ID is a foreign key to the Packages table. |
| Version              | nvarchar(20)         | A string representation of the version of the package. |
| MajorVersion         | int                  | An integer containing the major portion of the version number. i.e. major.minor.build.revision |
| MinorVersion         | int                  | An integer containing the minor portion of the version number. i.e. major.minor.build.revision |
| BuildVersion         | int                  | An integer containing the build portion of the version number. i.e. major.minor.build.revision |
| RevisionVersion      | int                  | An integer containing the revision portion of the version number. i.e. major.minor.build.revision |
| IsPrerelease         | bit                  | A flag that determines whether the package is a prerelease version. |
| CreatedUtcDateTime   | datetimeoffset       | A UTC datetime when the row was created in the database. |

### Assemblies
The Assemblies table contains a row for each unique assembly found. 

| Column Name        | Data Type            | Description |
|--------------------|----------------------|-------------|
| **AssemblyID**     | int                  | The assembly ID is the primary key for the assembly which is an autoincrementing integer value. |
| Name               | nvarchar(100)        | The name is the .NET assembly name of the assembly. |
| Title              | nvarchar(200)        | The title is the .NET assembly Fullname of the assembly containing the name, version, culture and public key token. |
| CreatedUtcDateTime | datetimeoffset       | A UTC datetime when the row was created in the database. |

### AssemblyVersions
The AssemnlyVersions table is a one-to-many table that contains a row for each version of an assembly.

| Column Name          | Data Type            | Description |
|----------------------|----------------------|-------------|
| **AssemblyVersionID**| int                  | The assembly version ID an autoincrementing integer value that is the primary key for this version of an assembly. |
| AssemblyID           | int                  | The assembly ID is a foreign key to the Assemblies table. |
| Version              | nvarchar(20)         | A string representation of the version of the assembly. |
| MajorVersion         | int                  | An integer containing the major portion of the version number. i.e. major.minor.build.revision |
| MinorVersion         | int                  | An integer containing the minor portion of the version number. i.e. major.minor.build.revision |
| BuildVersion         | int                  | An integer containing the build portion of the version number. i.e. major.minor.build.revision |
| RevisionVersion      | int                  | An integer containing the revision portion of the version number. i.e. major.minor.build.revision |
| CreatedUtcDateTime   | datetimeoffset       | A UTC datetime when the row was created in the database. |

### PackageVersionAssemblies
The PackageVersionAssemblies table is a many-to-many table that contains a row for each assembly version that was 
found in a particular version of a package.

| Column Name          | Data Type            | Description |
|----------------------|----------------------|-------------|
| **AssemblyVersionID**| int                  | The assembly version ID is the foreign key for this version of an assembly. |
| **PackageVersionID** | int                  | The package version ID is the foreign key for this version of a package. |


## Views
The following documents the views within the Catalog database.

### PackageVersionsView
The PackageVersionsView joins the Packages table with the PackageVersions table.

| Column Name          | Data Type            | Description |
|----------------------|:--------------------:|-------------|
| Name                 | nvarchar(100)        | The case-insensitive package identifier, which must be unique within the repository the package resides in. This is sometimes referred to as the package ID within the nuget metadata. |
| Title                | nvarchar(200)        | A human-friendly title of the package, typically used in UI displays. |
| Version              | nvarchar(20)         | A string representation of the version of the assembly. |
| MajorVersion         | int                  | An integer containing the major portion of the version number. i.e. major.minor.build.revision |
| MinorVersion         | int                  | An integer containing the minor portion of the version number. i.e. major.minor.build.revision |
| BuildVersion         | int                  | An integer containing the build portion of the version number. i.e. major.minor.build.revision |
| RevisionVersion      | int                  | An integer containing the revision portion of the version number. i.e. major.minor.build.revision |

### AssemblyVersionsView
The AssemblyVersionsView joins the Assemblies table with the AssemblyVersions table.

| Column Name          | Data Type            | Description |
|----------------------|:--------------------:|-------------|
| Name                 | nvarchar(100)        | The name is the .NET assembly name of the assembly. |
| Version              | nvarchar(20)         | A string representation of the version of the assembly. |
| MajorVersion         | int                  | An integer containing the major portion of the version number. i.e. major.minor.build.revision |
| MinorVersion         | int                  | An integer containing the minor portion of the version number. i.e. major.minor.build.revision |
| BuildVersion         | int                  | An integer containing the build portion of the version number. i.e. major.minor.build.revision |
| RevisionVersion      | int                  | An integer containing the revision portion of the version number. i.e. major.minor.build.revision |
| AssemblyID           | int                  | The assembly ID from the Assemblies table. |
| AssemblyVersionID    | int                  | The assembly version ID from the AssemblyVersions table. |

### PackageVersionAssembliesView
The PackageVersionAssembliesView joins the Packages and their associated PackageVersions and the PackageVersionAssemblies
and their associated Assemblies and AssemblyVersions. This view can be used to quickly query for packages that contain
a particular version of an assembly.

| Column Name             | Data Type            | Description |
|-------------------------|:--------------------:|-------------|
| PackageID               | int                  | The package ID is a foreign key to the Packages table. |
| PackageName             | nvarchar(100)        | The case-insensitive package identifier, which must be unique within the repository the package resides in. This is sometimes referred to as the package ID within the nuget metadata. |
| PackageVersionID        | int                  | The package version ID from the PackageVersion table. |
| PackageVersion          | nvarchar(20)         | A string representation of the version of the package. |
| PackageMajorVersion     | int                  | An integer containing the major portion of the package version number. i.e. major.minor.build.revision |
| PackageMinorVersion     | int                  | An integer containing the minor portion of the package version number. i.e. major.minor.build.revision |
| PackageBuildVersion     | int                  | An integer containing the build portion of the package version number. i.e. major.minor.build.revision |
| PackageRevisionVersion  | int                  | An integer containing the revision portion of the package version number. i.e. major.minor.build.revision |
| AssemblyID              | int                  | The assembly ID is a foreign key to the Assemblies table. |
| AssemblyName            | nvarchar(100)        | The name is the .NET assembly name of the assembly. |
| AssemblyVersionID       | int                  | The assembly version ID from the AssemblyVersions table. |
| AssemblyVersion         | nvarchar(20)         | A string representation of the version of the assembly. |
| AssemblyMajorVersion    | int                  | An integer containing the major portion of the assembly version number. i.e. major.minor.build.revision |
| AssemblyMinorVersion    | int                  | An integer containing the minor portion of the assembly version number. i.e. major.minor.build.revision |
| AssemblyBuildVersion    | int                  | An integer containing the build portion of the assembly version number. i.e. major.minor.build.revision |
| AssemblyRevisionVersion | int                  | An integer containing the revision portion of the assembly version number. i.e. major.minor.build.revision |

### MaxPackageVersion
The MaxPackageVersion view returns the maximum version of each package.

| Column Name             | Data Type            | Description |
|-------------------------|:--------------------:|-------------|
| PackageID               | int                  | The package ID is a foreign key to the Packages table. |
| Name                    | nvarchar(100)        | The case-insensitive package identifier, which must be unique within the repository the package resides in. This is sometimes referred to as the package ID within the nuget metadata. |
| MaxVersion              | nvarchar(20)         | A string representation of the maximum version of the package. |

### MaxAssemblyVersion
The MaxAssemblyVersion view returns the maximum version of each assembly.

| Column Name             | Data Type            | Description |
|-------------------------|:--------------------:|-------------|
| Name                    | nvarchar(100)        | The name is the .NET assembly name of the assembly. |
| MaxVersion              | nvarchar(20)         | A string representation of the maximum version of the assembly. |
