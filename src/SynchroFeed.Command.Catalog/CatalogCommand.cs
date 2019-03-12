#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="CatalogCommand.cs">
// MIT License
// 
// Copyright(c) 2018 Robert Vandehey
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mono.Cecil;
using SynchroFeed.Command.Catalog.Entity;
using SynchroFeed.Library;
using SynchroFeed.Library.Action;
using SynchroFeed.Library.Command;
using SynchroFeed.Library.Model;
using SynchroFeed.Library.Repository;
using Assembly = SynchroFeed.Command.Catalog.Entity.Assembly;
using Package = SynchroFeed.Library.Model.Package;
using Settings = SynchroFeed.Library.Settings;

namespace SynchroFeed.Command.Catalog
{
    /// <summary>The CatalogCommand is an implementation of <see cref="ICommand"/> that catalogs a package into a database.</summary>
    public class CatalogCommand : BaseCommand, IInitializable
    {
        private readonly PackageModelContext dbContext;
        private readonly Regex normalizeNameRegex;

        /// <summary>
        /// Initializes a new instance of the <see cref="CatalogCommand" /> class.
        /// </summary>
        /// <param name="action">The action running with this command.</param>
        /// <param name="commandSettings">The command settings associated with this command.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">The configuration service</param>
        /// <exception cref="ArgumentNullException">loggerFactory</exception>
        public CatalogCommand(
            IAction action,
            Settings.Command commandSettings,
            ILoggerFactory loggerFactory,
            IConfiguration configuration)
            : base(action, commandSettings, loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            Logger = loggerFactory.CreateLogger<CatalogCommand>();
            var connectionStringName = commandSettings.Settings.ConnectionStringName();
            if (connectionStringName == null)
            {
                connectionStringName = "PackageModel";
                Logger.LogInformation("No connection string name configured for Catalog command. Using \"PackageModel\".");
            }
            var connectionString = configuration.GetConnectionString(connectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException($"No connection string found with the name \"{commandSettings.Settings.ConnectionStringName()}\" ");
            dbContext = new PackageModelContext(connectionString);
            if (!string.IsNullOrEmpty(commandSettings.Settings.NormalizeRegEx()))
            {
                normalizeNameRegex = new Regex(commandSettings.Settings.NormalizeRegEx());
            }
        }

        /// <summary>Initializes the CatalogCommand by making sure the database has been created and all migrations have been run.</summary>
        public void Initialize()
        {
            CreateOrUpdateDatabase(dbContext);
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private ILogger Logger { get; }

        /// <summary>
        /// Gets the action type of this action.
        /// </summary>
        /// <value>The action type of this action.</value>
        public override string Type => "Catalog";

        /// <summary>The method that executes the appropriate command to process the package.</summary>
        /// <param name="package">The package for the command to handle.</param>
        /// <param name="packageEvent">The event associated with the package.</param>
        /// <returns>Returns the CommandResult for the package.</returns>
        public override CommandResult Execute(Package package, PackageEvent packageEvent)
        {
            Debug.Assert(package != null);

            if (packageEvent == PackageEvent.Deleted)
            {
                return ProcessPackageDeleted(package);
            }
            else if (packageEvent == PackageEvent.Added || packageEvent == PackageEvent.Promoted)
            {
                return ProcessPackageAdded(package);
            }

            return new CommandResult(this);
        }

        private CommandResult ProcessPackageDeleted(Package package)
        {
            try
            {
                var packageEntity = GetPackageEntity(dbContext, package);
                if (packageEntity == null)
                {
                    return new CommandResult(this);
                }

                // ReSharper disable once UnusedVariable
                var packageVersionEntity = GetPackageVersionEntity(package, packageEntity);
                if (packageVersionEntity == null)
                {
                    return new CommandResult(this);
                }

                // ReSharper disable once UnusedVariable
                var packageEnvironmentEntity = GetPackageEnvironmentEntity(Action.SourceRepository, packageVersionEntity);
                if (packageEnvironmentEntity == null)
                {
                    return new CommandResult(this);
                }

                packageVersionEntity.PackageEnvironments.Remove(packageEnvironmentEntity);
                dbContext.SaveChanges();
                return new CommandResult(this, true, $"{package.Id} in {Action.SourceRepository} was removed from catalog successfully");
            }
            catch (DbEntityValidationException ex)
            {
                Logger.LogError($"Entity Validation Error Cataloging package {package.Id}. Error: {ex.Message}. Reverting changes.");
                dbContext.RevertChanges(ex);
                return new CommandResult(this, false, $"Entity Validation Error Cataloging package {package.Id}. Error: {ex.Message}.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error Cataloging package {package.Id}. Error: {ex.Message}.");
                return new CommandResult(this, false, $"Error Cataloging package {package.Id}. Error: {ex.Message}.");
            }
        }

        /// <summary>Processes a package that was added to a repository.</summary>
        /// <param name="package">The package that was added to the respository.</param>
        /// <returns>CommandResult.</returns>
        protected virtual CommandResult ProcessPackageAdded(Package package)
        {
            try
            {
                var packageEntity = GetorAddPackageEntity(dbContext, package);
                // ReSharper disable once UnusedVariable
                var packageVersionEntity = GetorAddPackageVersionEntity(dbContext, Action.SourceRepository, package, packageEntity);
                // ReSharper disable once UnusedVariable
                var packageEnvironmentEntity = GetOrAddPackageEnvironmentEntity(Action.SourceRepository, packageEntity, packageVersionEntity);

                dbContext.SaveChanges();
                return new CommandResult(this, true, $"{package.Id} was cataloged successfully");
            }
            catch (DbEntityValidationException ex)
            {
                Logger.LogError($"Entity Validation Error Cataloging package {package.Id}. Error: {ex.Message}. Reverting changes.");
                dbContext.RevertChanges(ex);
                return new CommandResult(this, false, $"Entity Validation Error Cataloging package {package.Id}. Error: {ex.Message}.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error Cataloging package {package.Id}. Error: {ex.Message}.");
                return new CommandResult(this, false, $"Error Cataloging package {package.Id}. Error: {ex.Message}.");
            }
        }

        private void CreateOrUpdateDatabase(PackageModelContext dbcontext)
        {
            if (Logger.IsEnabled(LogLevel.Trace))
                dbcontext.Database.Log = Console.Write;

            if (Settings.Settings.CreateDatabaseIfNotFound())
            {
                Logger.LogDebug($"Setting to \"create database if not found\" for CatalogAction for connection string:{dbcontext.Database.Connection.ConnectionString}");
                if (dbcontext.Database.CreateIfNotExists())
                {
                    Logger.LogInformation($"Database created for CatalogAction for connection string:{dbcontext.Database.Connection.ConnectionString}");
                }

                dbcontext.Database.Initialize(true);
            }
        }

        private Entity.Package GetPackageEntity(PackageModelContext dbcontext, Package package)
        {
            return dbcontext.Packages.FirstOrDefault(s => s.Name == package.Id);
        }

        private Entity.Package GetorAddPackageEntity(PackageModelContext dbcontext, Package package)
        {
            // Get package from database or add it if not found
            var packageEntity = GetPackageEntity(dbcontext, package);
            if (packageEntity == null)
            {
                // Package not found in database. Add it.
                packageEntity = new Entity.Package
                {
                    Name = package.Id,
                    Title = string.IsNullOrEmpty(package.Title) ? package.Id : package.Title
                };
                dbcontext.Packages.Add(packageEntity);
                Logger.LogInformation($"Package {packageEntity.Name} added to database");
            }

            return packageEntity;
        }

        private PackageVersionEnvironment GetPackageEnvironmentEntity(IRepository<Package> repository, PackageVersion packageVersionEntity)
        {
            return packageVersionEntity.PackageEnvironments.FirstOrDefault(s => s.PackageVersionId == packageVersionEntity.PackageVersionId && s.Name == repository.Name);
        }

        private PackageVersionEnvironment GetOrAddPackageEnvironmentEntity(IRepository<Package> repository, Entity.Package packageEntity, PackageVersion packageVersionEntity)
        {
            var packageEnvironment = GetPackageEnvironmentEntity(repository, packageVersionEntity);
            if (packageEnvironment == null)
            {
                packageEnvironment = new PackageVersionEnvironment
                {
                    PackageVersionId = packageVersionEntity.PackageVersionId,
                    Name = repository.Name
                };
                packageVersionEntity.PackageEnvironments.Add(packageEnvironment);
                Logger.LogInformation($"Environment {packageEnvironment.Name} for Package {packageEntity.Name} added to database");
            }

            return packageEnvironment;
        }

        private PackageVersion GetPackageVersionEntity(Package package, Entity.Package packageEntity)
        {
            return packageEntity.PackageVersions.FirstOrDefault(s => s.PackageId == packageEntity.PackageId && s.Version == package.Version);
        }

        private PackageVersion GetorAddPackageVersionEntity(PackageModelContext dbcontext, IRepository<Package> repository, Package package, Entity.Package packageEntity)
        {
            var packageVersionEntity = GetPackageVersionEntity(package, packageEntity);

            // This version of the package was already processed, don't process again
            if (packageVersionEntity != null)
            {
                Logger.LogDebug($"{package.Id}.{package.Version} package version already exists. Ignoring.");
                return packageVersionEntity;
            }

            // Get package contents from repository
            var packageWithContent = repository.Fetch(package);
            var tempVersion = new Version(packageWithContent.Version);
            packageVersionEntity = new PackageVersion
            {
                Version = packageWithContent.Version,
                MajorVersion = Math.Max(0, tempVersion.Major),
                MinorVersion = Math.Max(0, tempVersion.Minor),
                BuildVersion = Math.Max(0, tempVersion.Build),
                RevisionVersion = Math.Max(0, tempVersion.Revision)
            };
            packageEntity.PackageVersions.Add(packageVersionEntity);
            Logger.LogInformation($"{package.Id}.{package.Version} package version added to database.");

            PopulatePackageAssembliesFromPackageContent(dbcontext, packageWithContent, packageEntity, packageVersionEntity);

            return packageVersionEntity;
        }

        private void PopulatePackageAssembliesFromPackageContent(PackageModelContext dbcontext, Package packageWithContent, Entity.Package packageEntity, PackageVersion packageVersionEntity)
        {
            var uniqueAssemblies = new Dictionary<string, AssemblyName>();
            using (var packageZipFileStream = new MemoryStream(packageWithContent.Content))
            using (var zipFile = new ZipFile(packageZipFileStream))
            {
                foreach (ZipEntry zipEntry in zipFile)
                {
                    if (!zipEntry.IsFile)
                        continue;

                    AssemblyInfo assemblyInfo = GetAssemblyInfoFromZipEntry(zipFile, zipEntry);
                    // Result is null if not a .NET assembly
                    if (assemblyInfo == null)
                        continue;

                    foreach (var referencedAssembly in assemblyInfo.ReferencedAssemblies)
                    {
                        if (!uniqueAssemblies.ContainsKey(referencedAssembly.FullName))
                        {
                            uniqueAssemblies.Add(referencedAssembly.FullName, referencedAssembly);
                        }
                    }
                    var version = assemblyInfo.AssemblyName.Version;
                    Logger.LogDebug($"Processing assembly {assemblyInfo.AssemblyName.Name}, version={version}");

                    var assemblyEntity = GetOrAddAssemblyEntity(dbcontext, assemblyInfo.AssemblyName);
                    var assemblyVersionEntity = GetOrAddAssemblyVersionEntity(dbcontext, assemblyEntity, assemblyInfo.AssemblyName);
                    // ReSharper disable once UnusedVariable
                    var packageAssemblyVersionEntity = GetOrAddPackageAssemblyVersionEntity(packageEntity, packageVersionEntity, assemblyEntity, assemblyVersionEntity);
                }
            }

            foreach (var uniqueAssembly in uniqueAssemblies.Values)
            {
                Logger.LogDebug($"Processing referenced assembly {uniqueAssembly.Name}, version={uniqueAssembly.Version}");

                var assemblyEntity = GetOrAddAssemblyEntity(dbcontext, uniqueAssembly);
                var assemblyVersionEntity = GetOrAddAssemblyVersionEntity(dbcontext, assemblyEntity, uniqueAssembly);
                // ReSharper disable once UnusedVariable
                var packageAssemblyVersionEntity = GetOrAddPackageAssemblyVersionEntity(packageEntity, packageVersionEntity, assemblyEntity, assemblyVersionEntity);
            }
        }

        private object GetOrAddPackageAssemblyVersionEntity(Entity.Package packageEntity, PackageVersion packageVersionEntity, Assembly assemblyEntity, AssemblyVersion assemblyVersionEntity)
        {
            var packageAssemblyVersionEntity = packageVersionEntity.AssemblyVersions.FirstOrDefault(s => s.AssemblyId == assemblyVersionEntity.AssemblyId && s.Version == assemblyVersionEntity.Version);
            if (packageAssemblyVersionEntity == null)
            {
                packageVersionEntity.AssemblyVersions.Add(assemblyVersionEntity);
                Logger.LogInformation($"{assemblyEntity.Name}.{assemblyVersionEntity.Version} assembly for package {packageEntity.Name}.{packageVersionEntity.Version} added to PackageVersion database");
            }
            else
            {
                Logger.LogDebug($"{assemblyEntity.Name}.{assemblyVersionEntity.Version} assembly found for package {packageEntity.Name}.{packageVersionEntity.Version}. Ignoring.");
            }

            return packageAssemblyVersionEntity;
        }

        private AssemblyVersion GetOrAddAssemblyVersionEntity(PackageModelContext dbcontext, Assembly assemblyEntity, AssemblyName assemblyName)
        {
            var assemblyVersion = assemblyName.Version.ToString();
            var assemblyVersionEntity = dbcontext.AssemblyVersions.FirstOrDefault(s => s.AssemblyId == assemblyEntity.AssemblyId && s.Version == assemblyVersion);
            if (assemblyVersionEntity == null)
            {
                assemblyVersionEntity = new AssemblyVersion
                {
                    AssemblyId = assemblyEntity.AssemblyId,
                    Version = assemblyVersion,
                    MajorVersion = Math.Max(0, assemblyName.Version.Major),
                    MinorVersion = Math.Max(0, assemblyName.Version.Minor),
                    BuildVersion = Math.Max(0, assemblyName.Version.Build),
                    RevisionVersion = Math.Max(0, assemblyName.Version.Revision),
                };
                assemblyEntity.AssemblyVersions.Add(assemblyVersionEntity);
                Logger.LogInformation($"{assemblyEntity.Name}.{assemblyVersionEntity.Version} assembly added to Assembly database");
            }
            else
            {
                Logger.LogDebug($"{assemblyEntity.Name}.{assemblyVersionEntity.Version} assembly already added to Assembly database. Ignoring.");
            }

            return assemblyVersionEntity;
        }

        private AssemblyInfo GetAssemblyInfoFromZipEntry(ZipFile zipFile, ZipEntry zipEntry)
        {
            AssemblyInfo assemblyInfo = null;
            if (zipEntry.Name.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
            {
                using (Stream zipStream = zipFile.GetInputStream(zipEntry))
                {
                    var tempFilename = Path.GetTempFileName();
                    using (var fileStream = new FileStream(tempFilename, FileMode.Append, FileAccess.Write, FileShare.Read))
                    {
                        zipStream.CopyTo(fileStream);
                    }
                    var assemblyDef = GlobalAssemblyResolver.Instance.GetAssemblyDefinition(tempFilename);
                    File.Delete(tempFilename);

                    if (assemblyDef == null)
                    {
                        Logger.LogDebug($"No assembly definition found for {zipEntry.Name}. Ignoring.");
                        return null;
                    }

                    assemblyInfo = new AssemblyInfo
                    {
                        AssemblyName = new AssemblyName() {FullName = assemblyDef.FullName, Name = assemblyDef.Name.Name, Version = assemblyDef.Name.Version, FrameworkVersion = assemblyDef.TargetFrameworkAttributeValue}
                    };

                    foreach (var referencedAssembly in assemblyDef.MainModule.AssemblyReferences)
                    {
                        assemblyInfo.ReferencedAssemblies.Add(new AssemblyName() {FullName = referencedAssembly.FullName, Name = referencedAssembly.Name, Version = referencedAssembly.Version});
                    }
                }
            }

            return assemblyInfo;
        }

        private Assembly GetOrAddAssemblyEntity(PackageModelContext dbcontext, AssemblyName assemblyName)
        {
            var normalizedAssemblyName = NormalizedName(assemblyName.Name);
            var assemblyEntity = dbcontext.Assemblies.FirstOrDefault(s => s.Name == normalizedAssemblyName);
            if (assemblyEntity == null)
            {
                assemblyEntity = new Assembly()
                {
                    Name = normalizedAssemblyName,
                };
                dbcontext.Assemblies.Add(assemblyEntity);
                Logger.LogInformation($"Assembly ({assemblyEntity.Name}) added to database");
            }
            else
            {
                Logger.LogDebug($"Assembly ({assemblyEntity.Name}) already added to database. Ignoring.");
            }

            return assemblyEntity;
        }

        private string NormalizedName(string assemblyName)
        {
            if (normalizeNameRegex != null)
            {
                var match = normalizeNameRegex.Match(assemblyName);
                if (match.Success)
                {
                    return assemblyName.Replace(match.Value, "");
                }
            }

            return assemblyName;
        }
    }

    internal class AssemblyInfo
    {
        public AssemblyInfo()
        {
            ReferencedAssemblies = new List<AssemblyName>();
        }

        public AssemblyName AssemblyName { get; set; }
        public List<AssemblyName> ReferencedAssemblies { get; }
    }

    internal class AssemblyName
    {
        public string FullName { get; set; }
        public string Name { get; set; }
        public Version Version { get; set; }
        public string FrameworkVersion { get; set; }
    }
}
