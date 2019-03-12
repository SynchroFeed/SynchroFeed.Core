#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="ProcessAction.cs">
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
using Microsoft.Extensions.Logging;
using SynchroFeed.Library.Action;
using SynchroFeed.Library.Action.Observer;
using SynchroFeed.Library.Command;
using SynchroFeed.Library.Model;
using SynchroFeed.Library.Repository;
using Settings=SynchroFeed.Library.Settings;

namespace SynchroFeed.Action.Process
{
    /// <summary>
    /// The AuditPackageAction class is an implementation of <see cref="IAction"/> that audits packages on a source feed.
    /// The characteristic of what is audited and the action to take is determined by the linked validators.
    /// </summary>
    /// <seealso cref="SynchroFeed.Library.Action.IAction" />
    public class ProcessAction : BaseAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessAction"/> class.
        /// </summary>
        /// <param name="applicationSettings">The application settings.</param>
        /// <param name="actionSettings">The action settings.</param>
        /// <param name="sourceRepository">The source repository.</param>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <exception cref="System.ArgumentNullException">loggerFactory</exception>
        public ProcessAction(
            Settings.ApplicationSettings applicationSettings,
            Settings.Action actionSettings,
            IRepository<Package> sourceRepository,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory)
            : base(applicationSettings, actionSettings, sourceRepository, serviceProvider, loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            Logger = loggerFactory.CreateLogger<ProcessAction>();
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
        public override string ActionType => "Process";

        /// <summary>
        /// Runs the specified Action using the feed configuration section.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the source or target feed can't be found in the &lt;feeds&gt; configuration.</exception>
        public override void Run()
        {
            Logger.LogInformation($"Running ProcessAction for \"{ActionSettings.Name}\". Source Feed:{ActionSettings.SourceFeed}, Only Latest Version: {ActionSettings.OnlyLatestVersion}, Include Prerelease:{ActionSettings.IncludePrerelease}, Fail on Error:{ActionSettings.FailOnError}");

            if (Commands.Count == 0)
            {
                Logger.LogInformation($"Aborting action \"{ActionSettings.Name}\" since the action doesn't have any commands.");
                return;
            }

            var sourcePackagesArray = GetPackages(SourceRepository);

            foreach (var package in sourcePackagesArray)
            {
                if (!ProcessPackage(package, PackageEvent.Processed))
                {
                    break;
                }
            }
        }

        /// <summary>The method in the action that processes the package.</summary>
        /// <param name="package">The package to process.</param>
        /// <param name="packageEvent">The type of event associated with the package.</param>
        /// <returns>
        ///   <c>true</c> if the process was successful and processing should continue, <c>false</c> otherwise.</returns>
        public override bool ProcessPackage(Package package, PackageEvent packageEvent)
        {
            if (IgnorePackage(package.Id))
            {
                Logger.LogDebug($"Package ({package.Id} is being ignored due to configuration");
                this.ObserverManager.NotifyObservers(new ActionEvent(this, ActionEventType.ActionPackageIgnored,
                                                                     $"Package ({package.Id} is being ignored due to configuration", null, package));
                return true;
            }

            try
            {
                Logger.LogDebug($"Fetching package {package.Id}.{package.Version} from {SourceRepository.Name}");
                var packageWithContent = SourceRepository.Fetch(package);
                Logger.LogTrace($"Fetching package {package.Id}.{package.Version} from {SourceRepository.Name}...done");

                switch (ProcessCommands(packageWithContent, packageEvent))
                {
                    case CommandFailureAction.Continue:
                        this.ObserverManager.NotifyObservers(new ActionEvent(this, ActionEventType.ActionPackageSuccess,
                                                                             $"Command successful on {package.Id}.{package.Version}", null, package));
                        return true;
                    case CommandFailureAction.FailPackage:
                        Logger.LogDebug($"Failure Action on {package.Id}.{package.Version} is FailPackage. Ignoring package.");
                        this.ObserverManager.NotifyObservers(new ActionEvent(this, ActionEventType.ActionPackageFailed,
                                                                             $"Failure Action on {package.Id}.{package.Version} is FailPackage. Ignoring package.", null, package));
                        return true;
                    case CommandFailureAction.FailAction:
                        Logger.LogDebug($"Failure Action on {package.Id}.{package.Version} is FailAction. Aborting action.");
                        this.ObserverManager.NotifyObservers(new ActionEvent(this, ActionEventType.ActionFailed,
                                                                             $"Failure Action on {package.Id}.{package.Version} is FailAction. Aborting action.", null, package));
                        return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error processing package {package.Id}.{package.Version}. Exception: {ex.Message}", ex);
                if (ActionSettings.FailOnError)
                    throw;
            }

            return true;
        }
    }
}
