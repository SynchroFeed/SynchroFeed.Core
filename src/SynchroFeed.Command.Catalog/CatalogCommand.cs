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
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using SynchroFeed.Command.Catalog.Entity;
using SynchroFeed.Library;
using SynchroFeed.Library.Action;
using SynchroFeed.Library.Command;
using SynchroFeed.Library.Model;
using SynchroFeed.Library.Reflection;
using SynchroFeed.Library.Repository;
using SynchroFeed.Library.Zip;
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

            if (action.ActionSettings.IncludePrerelease)
            {
                Logger.LogWarning($"The {nameof(CatalogCommand)} does not support pre-release versions.");
            }
        }

        /// <summary>Initializes the CatalogCommand by making sure the database has been created and all migrations have been run.</summary>
        public void Initialize()
        {
            CreateOrUpdateDatabase();
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

            if (package.IsPrerelease)
            {
                Logger.LogWarning($"The {package} skipped because it is a pre-release.");
            }
            else if (packageEvent == PackageEvent.Deleted)
            {
                return ProcessPackageDeleted(package);
            }
            else if (packageEvent == PackageEvent.Added || packageEvent == PackageEvent.Promoted || packageEvent == PackageEvent.Processed)
            {
                return ProcessPackageAdded(package);
            }

            return new CommandResult(this);
        }

        private CommandResult ProcessPackageDeleted(Package package)
        {
            try
            {
                var packageEntity = GetPackageEntity(package);
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
        /// <param name="package">The package that was added to the repository.</param>
        /// <returns>CommandResult.</returns>
        protected virtual CommandResult ProcessPackageAdded(Package package)
        {
            try
            {
                var packageEntity = GetorAddPackageEntity(package);
                // ReSharper disable once UnusedVariable
                var packageVersionEntity = GetorAddPackageVersionEntity(package, packageEntity);
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

        private void CreateOrUpdateDatabase()
        {
            if (Logger.IsEnabled(LogLevel.Trace))
                dbContext.Database.Log = Console.Write;

            if (Settings.Settings.CreateDatabaseIfNotFound())
            {
                Logger.LogDebug($"Setting to \"create database if not found\" for CatalogAction for connection string:{dbContext.Database.Connection.ConnectionString}");
                if (dbContext.Database.CreateIfNotExists())
                {
                    Logger.LogInformation($"Database created for CatalogAction for connection string:{dbContext.Database.Connection.ConnectionString}");
                }

                dbContext.Database.Initialize(true);
            }
        }

        private Entity.Package GetPackageEntity(Package package)
        {
            return dbContext.Packages.FirstOrDefault(s => s.Name == package.Id);
        }

        private Entity.Package GetorAddPackageEntity(Package package)
        {
            // Get package from database or add it if not found
            var packageEntity = GetPackageEntity(package);
            if (packageEntity == null)
            {
                // Package not found in database. Add it.
                packageEntity = new Entity.Package
                {
                    Name = package.Id,
                    Title = string.IsNullOrEmpty(package.Title) ? package.Id : package.Title
                };
                dbContext.Packages.Add(packageEntity);
                dbContext.SaveChanges();

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
                dbContext.SaveChanges();

                Logger.LogInformation($"Environment {packageEnvironment.Name} for Package {packageEntity.Name} added to database");
            }

            return packageEnvironment;
        }

        private PackageVersion GetPackageVersionEntity(Package package, Entity.Package packageEntity)
        {
            return packageEntity.PackageVersions.FirstOrDefault(s => s.PackageId == packageEntity.PackageId && s.Version == package.Version);
        }

        private PackageVersion GetorAddPackageVersionEntity(Package package, Entity.Package packageEntity)
        {
            var packageVersionEntity = GetPackageVersionEntity(package, packageEntity);

            // This version of the package was already processed, don't process again
            if (packageVersionEntity != null)
            {
                Logger.LogDebug($"{package.Id}.{package.Version} package version already exists. Ignoring.");
                return packageVersionEntity;
            }

            DbContextTransaction transaction = null;

            try
            {
                transaction = dbContext.Database.BeginTransaction();

                // Get package contents from repository
                var tempVersion = new Version(package.Version);
                packageVersionEntity = new PackageVersion
                {
                    Version = package.Version,
                    MajorVersion = Math.Max(0, tempVersion.Major),
                    MinorVersion = Math.Max(0, tempVersion.Minor),
                    BuildVersion = Math.Max(0, tempVersion.Build),
                    RevisionVersion = Math.Max(0, tempVersion.Revision)
                };
                packageEntity.PackageVersions.Add(packageVersionEntity);
                dbContext.SaveChanges();

                Logger.LogInformation($"{package.Id}.{package.Version} package version added to database.");

                PopulatePackageAssembliesFromPackageContent(package, packageEntity, packageVersionEntity);

                transaction.Commit();

                return packageVersionEntity;
            }
            catch
            {
                Logger.LogError("Exception thrown.  Rolling back changes.");

                transaction?.Rollback();
                throw;
            }
        }

        private void PopulatePackageAssembliesFromPackageContent(Package package, Entity.Package packageEntity, PackageVersion packageVersionEntity)
        {
            var uniqueAssemblies = new Dictionary<string, AssemblyName>();
            var coreAssembly = typeof(object).Assembly;

            using var byteStream = new MemoryStream(package.Content);
            using var archive = ArchiveFactory.Open(byteStream);
            using var lc = new MetadataLoadContext(new ZipAssemblyResolver(archive, coreAssembly), coreAssembly.FullName);
            foreach (var archiveEntry in archive.Entries)
            {
                if (archiveEntry.IsDirectory
                    || (!archiveEntry.Key.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase) && !archiveEntry.Key.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase)))
                    continue;

                System.Reflection.Assembly assembly;

                using var entryStream = archiveEntry.ExtractToStream();
                try
                {
                    assembly = lc.LoadFromStream(entryStream);
                }
                catch (FileLoadException)
                {
                    Logger.LogError($"{packageEntity.Name} (v{packageVersionEntity.Version}) - {archiveEntry.Key} - could not be loaded.");
                    continue;
                }
                catch (Exception e)
                {
                    Logger.LogError(e, $"{packageEntity.Name} (v{packageVersionEntity.Version}) - {archiveEntry.Key} - threw an exception.");
                    continue;
                }

                foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
                {
                    if (!uniqueAssemblies.ContainsKey(referencedAssembly.FullName))
                    {
                        uniqueAssemblies.Add(referencedAssembly.FullName, referencedAssembly);
                    }
                }

                var assemblyName = assembly.GetName();
                Logger.LogDebug($"Processing assembly {assemblyName.Name}, version={assemblyName.Version}");

                var assemblyEntity = GetOrAddAssemblyEntity(assemblyName);
                var assemblyVersionEntity = GetOrAddAssemblyVersionEntity(assemblyEntity, assemblyName);
                var packageAssemblyVersionEntity = GetOrAddPackageAssemblyVersionEntity(packageEntity, packageVersionEntity, assemblyEntity, assemblyVersionEntity);
                packageAssemblyVersionEntity.ReferenceIncluded = true;
            }

            foreach (var uniqueAssembly in uniqueAssemblies.Values)
            {
                Logger.LogDebug($"Processing referenced assembly {uniqueAssembly.Name}, version={uniqueAssembly.Version}");

                var assemblyEntity = GetOrAddAssemblyEntity(uniqueAssembly);
                var assemblyVersionEntity = GetOrAddAssemblyVersionEntity(assemblyEntity, uniqueAssembly);

                // ReSharper disable once UnusedVariable
                var packageAssemblyVersionEntity = GetOrAddPackageAssemblyVersionEntity(packageEntity, packageVersionEntity, assemblyEntity, assemblyVersionEntity);
            }
        }

        private PackageVersionAssembly GetOrAddPackageAssemblyVersionEntity(Entity.Package packageEntity, PackageVersion packageVersionEntity, Assembly assemblyEntity, AssemblyVersion assemblyVersionEntity)
        {
            var packageVersionAssemblyEntity = packageVersionEntity.PackageVersionAssemblies.FirstOrDefault(pva => (pva.AssemblyVersion.AssemblyId == assemblyEntity.AssemblyId) && (pva.AssemblyVersion.Version == assemblyVersionEntity.Version));
            if (packageVersionAssemblyEntity == null)
            {
                packageVersionAssemblyEntity = new PackageVersionAssembly()
                {
                    AssemblyVersion = assemblyVersionEntity,
                    PackageVersion = packageVersionEntity,
                };

                packageVersionEntity.PackageVersionAssemblies.Add(packageVersionAssemblyEntity);
                Logger.LogInformation($"{assemblyEntity.Name}.{assemblyVersionEntity.Version} assembly for package {packageEntity.Name}.{packageVersionEntity.Version} added to PackageVersionAssembly.");
            }
            else
            {
                Logger.LogDebug($"{assemblyEntity.Name}.{assemblyVersionEntity.Version} assembly found for package {packageEntity.Name}.{packageVersionEntity.Version}. Ignoring.");
            }

            return packageVersionAssemblyEntity;
        }

        private AssemblyVersion GetOrAddAssemblyVersionEntity(Assembly assemblyEntity, AssemblyName assemblyName)
        {
            if (assemblyName.Version == null)
            {
                throw new ArgumentNullException(nameof(assemblyName.Version));
            }
            var assemblyVersion = assemblyName.Version?.ToString();
            var assemblyVersionEntity = dbContext.AssemblyVersions.FirstOrDefault(s => s.AssemblyId == assemblyEntity.AssemblyId && s.Version == assemblyVersion);
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
                dbContext.SaveChanges();

                Logger.LogInformation($"{assemblyEntity.Name}.{assemblyVersionEntity.Version} assembly added to Assembly database");
            }
            else
            {
                Logger.LogDebug($"{assemblyEntity.Name}.{assemblyVersionEntity.Version} assembly already added to Assembly database. Ignoring.");
            }

            return assemblyVersionEntity;
        }
        
        private Assembly GetOrAddAssemblyEntity(AssemblyName assemblyName)
        {
            var normalizedAssemblyName = NormalizedName(assemblyName.Name);
            var assemblyEntity = dbContext.Assemblies.FirstOrDefault(s => s.Name == normalizedAssemblyName);
            if (assemblyEntity == null)
            {
                assemblyEntity = new Assembly()
                {
                    Name = normalizedAssemblyName,
                };
                dbContext.Assemblies.Add(assemblyEntity);
                dbContext.SaveChanges();

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
}
