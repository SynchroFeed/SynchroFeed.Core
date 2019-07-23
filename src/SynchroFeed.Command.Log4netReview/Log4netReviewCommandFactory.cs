using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SynchroFeed.Library.Action;
using SynchroFeed.Library.Command;
using SynchroFeed.Library.Factory;
using System;
using System.Diagnostics;
using Settings = SynchroFeed.Library.Settings;

namespace SynchroFeed.Command.Log4netReview
{
    /// <summary>
    /// The Log4Net review command scans a package's Log4net config to scan for any invalid configuration.
    /// </summary>
    public class Log4netReviewCommandFactory : BaseCommandFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Log4netReviewCommandFactory" /> class.
        /// </summary>
        /// <param name="applicationSettings">The settings that have been configured for this application.</param>
        /// <param name="serviceProvider">The dependency injection service provider.</param>
        /// <param name="loggerFactory">The logging factory instance.</param>
        /// <exception cref="ArgumentNullException">applicationSettings
        /// or
        /// serviceProvider
        /// or
        /// loggingFactory</exception>
        public Log4netReviewCommandFactory(
            Settings.ApplicationSettings applicationSettings,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory)
            : base(applicationSettings, serviceProvider, loggerFactory)
        {
            Logger = loggerFactory.CreateLogger<Log4netReviewCommandFactory>();
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private ILogger Logger { get; }

        /// <summary>
        /// Gets the type of the action.
        /// </summary>
        /// <value>The type of the action.</value>
        public override string Type { get; } = "Log4netReview";

        /// <summary>
        /// Creates the specified action.
        /// </summary>
        /// <param name="action">The action associated with this Command.</param>
        /// <param name="commandSettings">The settings associated with this Command.</param>
        /// <returns>Returns an instance of the SyncAction which is an instance of IAction.</returns>
        /// <exception cref="ArgumentNullException">actionSettings
        /// or
        /// settings
        /// or
        /// serviceProvider</exception>
        /// <exception cref="InvalidOperationException">actionSettings
        /// or
        /// settings
        /// or
        /// serviceProvider</exception>
        /// <exception cref="NotImplementedException"></exception>
        public override ICommand Create(IAction action, Settings.Command commandSettings)
        {
            if (commandSettings == null) throw new ArgumentNullException(nameof(commandSettings));
            Debug.Assert(commandSettings.Type.Equals(Type, StringComparison.CurrentCultureIgnoreCase));
            var command = ActivatorUtilities.CreateInstance<Log4netReviewCommand>(ServiceProvider, action,
                                                                       commandSettings.CloneAndMergeSettings(ApplicationSettings.SettingsGroups.Find(commandSettings.SettingsGroup)));

            return command;
        }
    }
}