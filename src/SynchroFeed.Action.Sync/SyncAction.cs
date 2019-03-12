#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="SyncAction.cs">
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
using System.Linq;
using Microsoft.Extensions.Logging;
using SynchroFeed.Library;
using SynchroFeed.Library.Action;
using SynchroFeed.Library.Action.Observer;
using SynchroFeed.Library.Command;
using SynchroFeed.Library.Model;
using SynchroFeed.Library.Repository;
using Settings=SynchroFeed.Library.Settings;

namespace SynchroFeed.Action.Sync
{
    /// <summary>
    /// The SyncAction class is an implementation of <see cref="IAction"/> that syncs packages from a source feed
    /// to a target feed. It can optionally delete orphaned packages from the target feed that no longer exist on
    /// the source feed.
    /// </summary>
    /// <seealso cref="SynchroFeed.Library.Action.IAction" />
    public class SyncAction : BaseAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyncAction" /> class.
        /// </summary>
        /// <param name="applicationSettings">The application settings.</param>
        /// <param name="actionSettings">The settings associated with this action.</param>
        /// <param name="sourceRepository">The source repository for this action.</param>
        /// <param name="targetRepository">The target repository for this action.</param>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <exception cref="System.ArgumentNullException">loggerFactory
        /// or
        /// targetRepository</exception>
        /// <exception cref="ArgumentNullException">loggerFactory
        /// or
        /// targetRepository</exception>
        public SyncAction(
            Settings.ApplicationSettings applicationSettings, 
            Settings.Action actionSettings, 
            IRepository<Package> sourceRepository,
            IRepository<Package> targetRepository,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory)
            : base(applicationSettings, actionSettings, sourceRepository, serviceProvider, loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            Logger = loggerFactory.CreateLogger<SyncAction>();

            TargetRepository = targetRepository ?? throw new ArgumentNullException(nameof(targetRepository));
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private ILogger Logger { get; }

        /// <summary>
        /// Gets the target repository.
        /// </summary>
        /// <value>The target repository.</value>
        public IRepository<Package> TargetRepository { get; }

        /// <summary>
        /// Gets the action type of this action.
        /// </summary>
        /// <value>The action type of this action.</value>
        public override string ActionType => "Sync";

        /// <summary>
        /// Runs the specified Action using the feed configuration section.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the source or target feed can't be found in the &lt;feeds&gt; configuration.</exception>
        public override void Run()
        {
            Logger.LogInformation($"Running SyncAction for \"{ActionSettings.Name}\". Source Feed:{ActionSettings.SourceFeed}, Target Feed:{ActionSettings.TargetFeed}, Only Latest Version: {ActionSettings.OnlyLatestVersion}, Include Prerelease:{ActionSettings.IncludePrerelease}, Fail on Error:{ActionSettings.FailOnError}");

            var sourcePackagesArray = GetPackages(SourceRepository);
            var targetPackagesArray = GetPackages(TargetRepository);
            if (AddNewPackagesOnSourceToTarget(sourcePackagesArray, targetPackagesArray))
            {
                if (ActionSettings.Settings.DeleteFromTarget())
                {
                    Logger.LogDebug($"Deleting orphaned packages from target feed: {TargetRepository.Name}");
                    DeleteOrphanedPackagesFromTarget(targetPackagesArray, sourcePackagesArray);
                }
            }
        }

        private bool AddNewPackagesOnSourceToTarget(Package[] sourcePackagesArray, Package[] targetPackagesArray)
        {
            Logger.LogDebug($"Determining differences between source feed:{SourceRepository.Name} and target feed:{TargetRepository.Name}");
            var packages = sourcePackagesArray.Except(targetPackagesArray).ToArray();

            Logger.LogTrace($"Packages missing on target: {packages.Length}. Adding packages.");

            Logger.LogDebug("Iterating through missing packages");
            foreach (var package in packages)
            {
                if (!ProcessPackage(package, PackageEvent.Added))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// The method in the action that processes the package.
        /// </summary>
        /// <param name="package">The package to process.</param>
        /// <param name="packageEvent">The type of event associated with the package.</param>
        /// <returns><c>true</c> if the process was successful and processing should continue, <c>false</c> otherwise.</returns>
        public override bool ProcessPackage(Package package, PackageEvent packageEvent)
        {
            switch (packageEvent)
            {
                case PackageEvent.Deleted:
                    return ProcessPackageDelete(package);
                default:
                    return ProcessPackageAdded(package, packageEvent);
            }
        }

        protected virtual bool ProcessPackageAdded(Package package, PackageEvent packageEvent)
        {
            if (IgnorePackage(package.Id))
            {
                Logger.LogDebug($"Package ({package.Id} is being ignored due to configuration");
                this.ObserverManager.NotifyObservers(new ActionEvent(this, ActionEventType.ActionPackageIgnored,
                                                                     $"Package ({package.Id} is being ignored due to configuration", null, package));
                return true;
            }

            Logger.LogDebug($"Fetching package {package.Id}.{package.Version} from {SourceRepository.Name}");
            var packageWithContent = SourceRepository.Fetch(package);
            Logger.LogTrace($"Fetching package {package.Id}.{package.Version} from {SourceRepository.Name}...done");
            try
            {
                switch (ProcessCommands(package, packageEvent))
                {
                    case CommandFailureAction.Continue:
                        Logger.LogInformation($"Adding package {packageWithContent.Id}.{packageWithContent.Version} to {TargetRepository.Name}");
                        TargetRepository.Add(packageWithContent);
                        Logger.LogTrace($"Adding package {packageWithContent.Id}.{packageWithContent.Version} to {TargetRepository.Name}...done");
                        this.ObserverManager.NotifyObservers(new ActionEvent(this, ActionEventType.ActionPackageSuccess,
                                                                             $"Package added {packageWithContent.Id}.{packageWithContent.Version} to {TargetRepository.Name}",
                                                                             null, package));
                        break;
                    case CommandFailureAction.FailPackage:
                        Logger.LogDebug($"Failure Action on {package.Id}.{package.Version} is FailPackage. Ignoring package.");
                        this.ObserverManager.NotifyObservers(new ActionEvent(this, ActionEventType.ActionPackageFailed,
                                                                             $"Failure Action on {package.Id}.{package.Version} is FailPackage. Ignoring package.",
                                                                             null, package));
                        return true;
                    case CommandFailureAction.FailAction:
                        Logger.LogDebug($"Failure Action on {package.Id}.{package.Version} is FailAction. Aborting action.");
                        this.ObserverManager.NotifyObservers(new ActionEvent(this, ActionEventType.ActionFailed,
                                                                             $"Failure Action on {package.Id}.{package.Version} is FailAction. Aborting action.",
                                                                             null,
                                                                             package));
                        return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error adding package {packageWithContent.Id}.{packageWithContent.Version}. Exception: {ex.Message}", ex);
                if (ActionSettings.FailOnError)
                    throw;
            }

            return true;
        }

        protected virtual bool ProcessPackageDelete(Package package)
        {
            try
            {
                Logger.LogInformation($"Deleting package: {package.Id}.{package.Version} from {TargetRepository.Name}");
                TargetRepository.Delete(package);
                Logger.LogTrace($"Deleting package: {package.Id}.{package.Version} from {TargetRepository.Name}...done");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error adding package {package.Id}.{package.Version}. Exception: {ex.Message}", ex);
                if (ActionSettings.FailOnError)
                    throw;
            }

            return true;
        }

        private void DeleteOrphanedPackagesFromTarget(Package[] targetPackagesArray, Package[] sourcePackagesArray)
        {
            Logger.LogDebug($"Determining differences between target feed:{TargetRepository.Name} and source feed:{SourceRepository.Name}");
            var packagesToDelete = targetPackagesArray.Except(sourcePackagesArray).ToArray();

            Logger.LogTrace(($"Packages on target not on source: {packagesToDelete.Length}. Deleting packages."));

            Logger.LogDebug("Iterating through orphaned packages");
            foreach (var package in packagesToDelete)
            {
                ProcessPackage(package, PackageEvent.Deleted);
            }
        }
    }

    internal static class SyncActionSettingsExtension
    {
        public static bool DeleteFromTarget(this Settings.SettingsCollection settings)
        {
            return settings.GetCustomSetting<bool>("DeleteFromTarget");
        }
    }
}
