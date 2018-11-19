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
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.Logging;
using SynchroFeed.Command.Catalog.Entity;
using SynchroFeed.Library.Action;
using SynchroFeed.Library.Command;
using SynchroFeed.Library.DomainLoader;
using SynchroFeed.Library.Repository;
using Assembly = SynchroFeed.Command.Catalog.Entity.Assembly;
using Package = SynchroFeed.Library.Model.Package;
using Settings = SynchroFeed.Library.Settings;

namespace SynchroFeed.Command.Catalog
{
    public class CatalogCommand : BaseCommand
    {
        private readonly PackageModelContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="CatalogCommand" /> class.
        /// </summary>
        /// <param name="action">The action running with this command.</param>
        /// <param name="commandSettings">The command settings associated with this command.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <exception cref="ArgumentNullException">loggerFactory</exception>
        public CatalogCommand(
            IAction action,
            Settings.Command commandSettings,
            ILoggerFactory loggerFactory)
            : base(action, commandSettings, loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            Logger = loggerFactory.CreateLogger<CatalogCommand>();
            dbContext = new PackageModelContext(action.ActionSettings.Settings.ConnectionStringName() ?? "PackageModel");
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

        public override CommandResult Execute(Package package)
        {
            Debug.Assert(package != null);

            try
            {
                EnsureDatabaseExists();
                var packageEntity = GetorAddPackageEntity(dbContext, package);
                // ReSharper disable once UnusedVariable
                var packageEnvironmentEntity = GetOrAddPackageEnvironmentEntity(Action.SourceRepository, packageEntity);
                // ReSharper disable once UnusedVariable
                var packageVersionEntity = GetorAddPackageVersionEntity(dbContext, Action.SourceRepository, package, packageEntity);

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

        private bool checkedForDatabaseExistense;
        private void EnsureDatabaseExists()
        {
            if (!checkedForDatabaseExistense)
            {
                CreateOrUpdateDatabase(dbContext);
                checkedForDatabaseExistense = true;
            }
        }

        private void CreateOrUpdateDatabase(PackageModelContext dbcontext)
        {
            if (Logger.IsEnabled(LogLevel.Trace))
                dbcontext.Database.Log = Console.Write;

            if (Action.ActionSettings.Settings.CreateDatabaseIfNotFound())
            {
                Logger.LogDebug($"Setting to create database if not found for CatalogAction for connection string:{dbcontext.Database.Connection.ConnectionString}");
                if (dbcontext.Database.CreateIfNotExists())
                {
                    Logger.LogInformation($"Database created for CatalogAction for connection string:{dbcontext.Database.Connection.ConnectionString}");
                }

                dbcontext.Database.Initialize(false);
            }
        }

        private Entity.Package GetorAddPackageEntity(PackageModelContext dbcontext, Package package)
        {
            // Get package from database or add it if not found
            var packageEntity = dbcontext.Packages.FirstOrDefault(s => s.Name == package.Id);
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

        private PackageEnvironment GetOrAddPackageEnvironmentEntity(IRepository<Package> repository, Entity.Package packageEntity)
        {
            var packageEnvironment =
                packageEntity.PackageEnvironments.FirstOrDefault(s => s.PackageId == packageEntity.PackageId && s.Name == repository.Name);
            if (packageEnvironment == null)
            {
                packageEnvironment = new PackageEnvironment
                {
                    PackageId = packageEntity.PackageId,
                    Name = repository.Name
                };
                packageEntity.PackageEnvironments.Add(packageEnvironment);
                Logger.LogInformation($"Environment {packageEnvironment.Name} for Package {packageEntity.Name} added to database");
            }

            return packageEnvironment;
        }

        private PackageVersion GetorAddPackageVersionEntity(PackageModelContext dbcontext, IRepository<Package> repository, Package package, Entity.Package packageEntity)
        {
            var packageVersionEntity = packageEntity.PackageVersions.FirstOrDefault(s => s.PackageId == packageEntity.PackageId && s.Version == package.Version);

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
            using (var packageZipFileStream = new MemoryStream(packageWithContent.Content))
            using (var zipFile = new ZipFile(packageZipFileStream))
            {
                foreach (ZipEntry zipEntry in zipFile)
                {
                    if (!zipEntry.IsFile)
                        continue;

                    AssemblyName assemblyName = GetAssemblyNameFromZipEntry(zipFile, zipEntry);
                    // Result is null if not a .NET assembly
                    if (assemblyName == null)
                        continue;

                    var version = assemblyName.Version;
                    Logger.LogDebug($"Processing assembly {assemblyName.Name}, version={version}");

                    var assemblyEntity = GetOrAddAssemblyEntity(dbcontext, assemblyName);
                    var assemblyVersionEntity = GetOrAddAssemblyVersionEntity(dbcontext, assemblyEntity, assemblyName);
                    // ReSharper disable once UnusedVariable
                    var packageAssemblyVersionEntity = GetOrAddPackageAssemblyVersionEntity(packageEntity, packageVersionEntity, assemblyEntity, assemblyVersionEntity);
                }
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

        private AssemblyName GetAssemblyNameFromZipEntry(ZipFile zipFile, ZipEntry zipEntry)
        {
            AssemblyName zipFileAssemblyName = null;
            if (zipEntry.Name.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
            {
                using (Stream zipStream = zipFile.GetInputStream(zipEntry))
                {
                    using (var memoryStream = new MemoryStream((int)zipEntry.Size))
                    {
                        zipStream.CopyTo(memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);

                        using (AssemblyReflectionManager arm = new AssemblyReflectionManager())
                        {
                            var proxy = arm.LoadAssembly(memoryStream.ToArray());
                            if (proxy == null)
                            {
                                Logger.LogTrace($"Ignoring non-.NET assembly - {zipEntry.Name}");
                                return null;
                            }

                            zipFileAssemblyName = proxy.AssemblyName;
                        }
                    }
                }
            }

            return zipFileAssemblyName;
        }

        private Assembly GetOrAddAssemblyEntity(PackageModelContext dbcontext, AssemblyName assemblyName)
        {
            var assemblyEntity = dbcontext.Assemblies.FirstOrDefault(s => s.Name == assemblyName.Name);
            if (assemblyEntity == null)
            {
                assemblyEntity = new Assembly()
                {
                    Name = assemblyName.Name,
                    Title = assemblyName.FullName
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
    }
}
