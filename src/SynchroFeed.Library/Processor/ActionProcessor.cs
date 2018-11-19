#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="ActionProcessor.cs">
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
using SynchroFeed.Library.Factory;
using SynchroFeed.Library.Settings;

namespace SynchroFeed.Library.Processor
{
    /// <summary>
    /// The ActionProcessor is a static processor class for processing actions.
    /// </summary>
    /// <remarks>
    /// The ProcessActions method takes the feed configuration as input and executes
    /// the configured actions.
    /// </remarks>
    public class ActionProcessor : IActionProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionProcessor"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="applicationSettings">The application settings.</param>
        /// <exception cref="ArgumentNullException">
        /// serviceProvider
        /// or
        /// applicationSettings
        /// or
        /// loggerFactory
        /// </exception>
        public ActionProcessor(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ApplicationSettings applicationSettings)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            ApplicationSettings = applicationSettings ?? throw new ArgumentNullException(nameof(applicationSettings));
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));
            Logger = loggerFactory.CreateLogger<ActionProcessor>();
        }

        /// <summary>
        /// Gets the application settings.
        /// </summary>
        /// <value>The application settings.</value>
        public ApplicationSettings ApplicationSettings { get; }

        /// <summary>
        /// Gets the service provider.
        /// </summary>
        /// <value>The service provider.</value>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private ILogger Logger { get; }

        /// <summary>
        /// The ProcessActions method is the main entry point of processing actions. It takes
        /// the feed configuration as input and executes the configured actions.
        /// </summary>
        /// <param name="actions">A list of actions within the config file to run. Run all enabled actions if the parameter is null or empty.</param>
        /// <exception cref="ArgumentNullException">Thrown if the feedConfig is null</exception>
        /// <exception cref="InvalidOperationException">Thrown if an action type wasn't found.</exception>
        public void Execute(List<string> actions = null)
        {
            if (ApplicationSettings.Actions.Count == 0)
            {
                Logger.LogError("Aborting process since there are no configured actions. Check your configuration.");
                return;
            }

            Settings.Action[] actionsToExecute;

            if (actions == null || actions.Count == 0)
            {
                actionsToExecute = ApplicationSettings.Actions.Where(a => a.Enabled).ToArray();
                if (actionsToExecute.Length == 0)
                {
                    Logger.LogError("No enabled action found. Aborting execution.");
                    return;
                }
            }
            else
            {
                actionsToExecute = ApplicationSettings.Actions.Where(a => actions.Contains(a.Name, StringComparer.CurrentCultureIgnoreCase)).ToArray();
                if (actionsToExecute.Length == 0)
                {
                    Logger.LogError($"No actions found matching any action names: \"{string.Join(", ", actions)}\". Aborting execution.");
                    return;
                }

                // Validate all of the actions to run have been found
                var missingActions = actions.Except(actionsToExecute.Select(p => p.Name), StringComparer.CurrentCultureIgnoreCase).ToArray();
                if (missingActions.Any())
                {
                    Logger.LogWarning($"The following actions weren't found in the configuration file and are being ignored: \"{string.Join(", ", missingActions)}\".");
                }
            }

            Logger.LogDebug($"Processing {actionsToExecute.Length} enabled actions");
            foreach (var actionSettings in actionsToExecute)
            {
                try
                {
                    using (var scope = ServiceProvider.CreateScope())
                    {
                        var action = CreateAction(scope, actionSettings);
                        action.ObserverManager.NotifyObservers(
                                                               new ActionEvent(
                                                                               action,
                                                                               ActionEventType.ActionStarted,
                                                                               $"{actionSettings.Name}[{actionSettings.Type}] has started"));
                        action.Run();
                        action.ObserverManager.NotifyObservers(
                                                               new ActionEvent(
                                                                               action,
                                                                               ActionEventType.ActionCompleted,
                                                                               $"{actionSettings.Name}[{actionSettings.Type}] has completed"));
                        Logger.LogDebug($"Running {actionSettings.Type} on \"{actionSettings.Name}\" Action...done");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error running {actionSettings.Type} on \"{actionSettings.Name}\" Action. Error: {ex.Message}. Ignoring.");
                }
            }
        }

        /// <summary>
        /// Creates an action given the ServiceScope and the action settings.
        /// </summary>
        /// <param name="scope">The service scope.</param>
        /// <param name="actionSettings">The action settings to create the action from.</param>
        /// <returns>Action.IAction.</returns>
        public Action.IAction CreateAction(IServiceScope scope, Settings.Action actionSettings)
        {
            Logger.LogTrace($"Creating {actionSettings.Type} on \"{actionSettings.Name}\" Action");

            var actionFactory = scope.ServiceProvider.GetNamedFactory<IActionFactory>(actionSettings.Type);

            Logger.LogInformation($"Running {actionSettings.Type} on \"{actionSettings.Name}\" Action");
            return actionFactory.Create(actionSettings);
        }
    }
}