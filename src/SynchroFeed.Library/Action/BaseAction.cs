#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="BaseAction.cs">
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
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SynchroFeed.Library.Action.Observer;
using SynchroFeed.Library.Command;
using SynchroFeed.Library.Factory;
using SynchroFeed.Library.Model;
using SynchroFeed.Library.Repository;
using SynchroFeed.Library.Settings;

namespace SynchroFeed.Library.Action
{
    /// <summary>
    /// Class BaseAction.
    /// </summary>
    /// <seealso cref="SynchroFeed.Library.Action.IAction" />
    public abstract class BaseAction : IAction
    {
        /// <summary>
        /// The variable is used to lock access to the commands collection when it is being built
        /// </summary>
        private readonly object synclock = new object();

        /// <summary>
        /// The commands associate with this Action.
        /// </summary>
        private List<ICommand> commands;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseAction" /> class.
        /// </summary>
        /// <param name="applicationSettings">The application settings.</param>
        /// <param name="actionSettings">The action settings.</param>
        /// <param name="sourceRepository">The source repository.</param>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <exception cref="System.ArgumentNullException">applicationSettings
        /// or
        /// actionSettings
        /// or
        /// sourceRepository
        /// or
        /// serviceProvider
        /// or
        /// actionObserverManager</exception>
        /// <exception cref="ArgumentNullException">applicationSettings
        /// or
        /// actionSettings
        /// or
        /// sourceRepository
        /// or
        /// serviceProvider
        /// or
        /// actionObserverManager</exception>
        protected BaseAction(
            ApplicationSettings applicationSettings,
            Settings.Action actionSettings,
            IRepository<Package> sourceRepository,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory)
        {
            ApplicationSettings = applicationSettings ?? throw new ArgumentNullException(nameof(applicationSettings));
            ActionSettings = actionSettings ?? throw new ArgumentNullException(nameof(actionSettings));
            SourceRepository = sourceRepository ?? throw new ArgumentNullException(nameof(sourceRepository));
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Logger = loggerFactory.CreateLogger<BaseAction>();
            Enabled = ActionSettings.Enabled;

            ObserverManager = ActivatorUtilities.CreateInstance<ActionObserverManager>(ServiceProvider, this, loggerFactory);
        }

        /// <summary>
        /// Gets the action settings.
        /// </summary>
        /// <value>The action settings.</value>
        public Settings.Action ActionSettings { get; }

        /// <summary>
        /// Gets the action type of this action.
        /// </summary>
        /// <value>The action type of this action.</value>
        public abstract string ActionType { get; }

        /// <summary>
        /// Gets the application settings.
        /// </summary>
        /// <value>The application settings.</value>
        public ApplicationSettings ApplicationSettings { get; }

        /// <summary>
        /// Gets the list of commands for this Action.
        /// </summary>
        /// <value>The commands for this Action.</value>
        public List<ICommand> Commands
        {
            get
            {
                if (this.commands == null)
                {
                    lock (this.synclock)
                    {
                        if (this.commands == null)
                        {
                            this.commands = new List<ICommand>(this.ActionSettings.Commands.Count);
                            foreach (var command in this.ActionSettings.Commands)
                            {
                                var commandFactory = this.ServiceProvider.GetNamedFactory<ICommandFactory>(command.Type);
                                var commandInstance = commandFactory.Create(this, command);
                                if (commandInstance is IInitializable initializable)
                                {
                                    initializable.Initialize();
                                }
                                this.commands.Add(commandInstance);
                            }
                        }
                    }
                }

                return this.commands;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IAction" /> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled { get; }

        /// <summary>
        /// Gets the action observer manager.
        /// </summary>
        /// <value>The action observer manager.</value>
        public IActionObserverManager ObserverManager { get; }

        /// <summary>
        /// Gets the service provider used for dependency injection.
        /// </summary>
        /// <value>The service provider.</value>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the source repository.
        /// </summary>
        /// <value>The source repository.</value>
        public IRepository<Package> SourceRepository { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private ILogger Logger { get; }

        /// <summary>
        /// Runs the specified Action using the feed configuration section.
        /// </summary>
        public abstract void Run();

        /// <summary>Gets the packages.</summary>
        /// <param name="repo">The package repository.</param>
        /// <returns>Package[].</returns>
        protected Package[] GetPackages(IRepository<Package> repo)
        {
            Logger.LogDebug($"Retrieving packages from feed: {repo.Name}");
            IEnumerable<Package> packages = ActionSettings.OnlyLatestVersion
                                                ? repo.Fetch(t => (ActionSettings.IncludePrerelease || !t.IsPrerelease) && t.IsLatestVersion)
                                                : repo.Fetch(t => (ActionSettings.IncludePrerelease || !t.IsPrerelease));
            var packagesArray = packages as Package[] ?? packages.ToArray();
            return packagesArray;
        }

        /// <summary>
        /// Determines whether the package should be ignored.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <returns><c>true</c> if the package should be ignored, <c>false</c> otherwise.</returns>
        protected bool IgnorePackage(string packageId)
        {
            // TODO - Should convert this to a regex
            return ActionSettings.PackagesToIgnore.Any(p => p.Equals(packageId, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// The method in the action that processes the package.
        /// </summary>
        /// <param name="package">The package to process.</param>
        /// <param name="packageEvent">The type of event associated with the package.</param>
        /// <returns><c>true</c> if the process was successful and processing should continue, <c>false</c> otherwise.</returns>
        public abstract bool ProcessPackage(Package package, PackageEvent packageEvent);

        /// <summary>Processes the configured Commands against the package.</summary>
        /// <param name="package">The package for the Commands to process.</param>
        /// <param name="packageEvent">The event associated with the package.</param>
        /// <returns>Returns the Settings.CommandFailureAction.</returns>
        protected CommandFailureAction ProcessCommands(Package package, PackageEvent packageEvent)
        {
            foreach (var command in Commands)
            {
                var result = command.Execute(package, packageEvent);
                if (result.ResultValid)
                {
                    this.ObserverManager.NotifyObservers(new ActionEvent(this, ActionEventType.ActionCommandSuccess, result.Result, result.Command, package));
                }
                else
                {
                    this.ObserverManager.NotifyObservers(new ActionEvent(this, ActionEventType.ActionCommandFailed, result.Result, result.Command, package));
                    if (command.Settings.FailureAction == CommandFailureAction.FailPackage)
                        return CommandFailureAction.FailPackage;
                    if (command.Settings.FailureAction == CommandFailureAction.FailAction)
                        return CommandFailureAction.FailAction;
                }
            }

            return CommandFailureAction.Continue;
        }
    }
}