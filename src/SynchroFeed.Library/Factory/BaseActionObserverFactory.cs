#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="BaseActionObserverFactory.cs">
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
using SynchroFeed.Library.Action.Observer;
using SynchroFeed.Library.Settings;

namespace SynchroFeed.Library.Factory
{
    public abstract class BaseActionObserverFactory : IActionObserverFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseActionFactory"/> class.
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
        protected BaseActionObserverFactory(ApplicationSettings applicationSettings, IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            ApplicationSettings = applicationSettings ?? throw new ArgumentNullException(nameof(applicationSettings));
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            Logger = loggerFactory.CreateLogger<BaseActionFactory>();
        }

        /// <summary>
        /// Gets the application settings.
        /// </summary>
        /// <value>The application settings.</value>
        public ApplicationSettings ApplicationSettings { get; }

        /// <summary>
        /// Gets the service provider used for dependency injection.
        /// </summary>
        /// <value>The service provider.</value>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the type of the action.
        /// </summary>
        /// <value>The type of the action.</value>
        public abstract string Type { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private ILogger Logger { get; }

        /// <summary>
        /// Creates the specified action.
        /// </summary>
        /// <param name="observerSettings">The settings associated with the action observer.</param>
        /// <returns>Returns an instance of the IActionObserver.</returns>
        /// <exception cref="ArgumentNullException">actionSettings
        /// or
        /// settings
        /// or
        /// serviceProvider</exception>
        /// <exception cref="InvalidOperationException"></exception>
        public abstract IActionObserver Create(Observer observerSettings);
    }
}