#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="WebPostActionObserverFactory.cs">
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SynchroFeed.Library.Action.Observer;
using SynchroFeed.Library.Factory;
using Settings=SynchroFeed.Library.Settings;

namespace SynchroFeed.ActionObserver.WebPost
{
    /// <summary>
    /// The WebPostActionObserverFactory class is a factory class for creating a WebPostActionObserver.
    /// </summary>
    /// <seealso cref="SynchroFeed.Library.Factory.BaseActionObserverFactory" />
    public class WebPostActionObserverFactory : BaseActionObserverFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebPostActionObserverFactory"/> class.
        /// </summary>
        /// <param name="applicationSettings">The settings that have been configured for this application.</param>
        /// <param name="serviceProvider">The dependency injection service provider.</param>
        /// <param name="loggerFactory">The logging factory instance.</param>
        /// <exception cref="ArgumentNullException">
        /// applicationSettings
        /// or
        /// serviceProvider
        /// or
        /// loggingFactory
        /// </exception>
        public WebPostActionObserverFactory(Settings.ApplicationSettings applicationSettings, IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
            : base(applicationSettings, serviceProvider, loggerFactory)
        {
            Logger = loggerFactory.CreateLogger<WebPostActionObserverFactory>();
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
        public override string Type { get; } = "WebPost";

        /// <summary>
        /// Creates the specified action.
        /// </summary>
        /// <param name="observerSettings">The settings associated with the action observer.</param>
        /// <returns>Returns an instance of the SyncAction which is an instance of IAction.</returns>
        /// <exception cref="System.ArgumentNullException">actionSettings</exception>
        /// <exception cref="System.InvalidOperationException">
        /// </exception>
        /// <exception cref="ArgumentNullException">actionSettings
        /// or
        /// settings
        /// or
        /// serviceProvider</exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public override IActionObserver Create(Settings.Observer observerSettings)
        {
            if (observerSettings == null) throw new ArgumentNullException(nameof(observerSettings));
            var action = ActivatorUtilities.CreateInstance<WebPostActionObserver>(
                              ServiceProvider,
                              observerSettings.CloneAndMergeSettings(ApplicationSettings.SettingsGroups.Find(observerSettings.SettingsGroup)));

            return action;
        }
    }
}
