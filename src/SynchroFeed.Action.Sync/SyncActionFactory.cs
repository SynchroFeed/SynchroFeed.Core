#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="SyncActionFactory.cs">
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
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SynchroFeed.Library.Action;
using SynchroFeed.Library.Factory;
using SynchroFeed.Library.Settings;

namespace SynchroFeed.Action.Sync
{
    public class SyncActionFactory : BaseActionFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyncActionFactory" /> class.
        /// </summary>
        /// <param name="applicationSettings">The settings that have been configured for this application.</param>
        /// <param name="serviceProvider">The dependency injection service provider.</param>
        /// <param name="loggerFactory">The logging factory instance.</param>
        /// <exception cref="ArgumentNullException">applicationSettings
        /// or
        /// serviceProvider
        /// or
        /// loggingFactory</exception>
        public SyncActionFactory(
            ApplicationSettings applicationSettings, 
            IServiceProvider serviceProvider, 
            ILoggerFactory loggerFactory)
            : base(applicationSettings, serviceProvider, loggerFactory)
        {
            Logger = loggerFactory.CreateLogger<SyncActionFactory>();
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
        public override string Type { get; } = "Sync";

        /// <summary>
        /// Creates the specified action.
        /// </summary>
        /// <param name="actionSettings">The settings associated with the action.</param>
        /// <returns>Returns an instance of the SyncAction which is an instance of IAction.</returns>
        /// <exception cref="ArgumentNullException">actionSettings
        /// or
        /// settings
        /// or
        /// serviceProvider</exception>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        /// <exception cref="NotImplementedException"></exception>
        public override IAction Create(Library.Settings.Action actionSettings)
        {
            if (actionSettings == null) throw new ArgumentNullException(nameof(actionSettings));
            if (string.IsNullOrEmpty(actionSettings.SourceFeed)) throw new InvalidOperationException($"Source feed for action ({actionSettings.Name}) is empty. The Sync action must have a source feed");
            if (string.IsNullOrEmpty(actionSettings.TargetFeed)) throw new InvalidOperationException($"Target feed for action ({actionSettings.Name}) is empty. The Sync action must have a target feed");
            Debug.Assert(actionSettings.Type.Equals(Type, StringComparison.CurrentCultureIgnoreCase));

            var sourceRepository = GetRepository(actionSettings.SourceFeed);
            var targetRepository = GetRepository(actionSettings.TargetFeed);

            var action = ActivatorUtilities.CreateInstance<SyncAction>(ServiceProvider, 
                                                                       actionSettings.CloneAndMergeSettings(ApplicationSettings.SettingsGroups.Find(actionSettings.SettingsGroup)), 
                                                                       sourceRepository, 
                                                                       targetRepository);

            return action;
        }
    }
}
