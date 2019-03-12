#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="BaseActionFactory.cs">
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
using SynchroFeed.Library.Action;
using SynchroFeed.Library.Model;
using SynchroFeed.Library.Repository;
using SynchroFeed.Library.Settings;

namespace SynchroFeed.Library.Factory
{
    /// <summary>The BaseActionFactory is the base class for implementing an <see cref="IActionFactory"/>.</summary>
    public abstract class BaseActionFactory : IActionFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseActionFactory" /> class.
        /// </summary>
        /// <param name="applicationSettings">The settings that have been configured for this application.</param>
        /// <param name="serviceProvider">The dependency injection service provider.</param>
        /// <param name="loggerFactory">The logging factory instance.</param>
        /// <exception cref="System.ArgumentNullException">applicationSettings
        /// or
        /// serviceProvider
        /// or
        /// loggerFactory</exception>
        /// <exception cref="ArgumentNullException">applicationSettings
        /// or
        /// serviceProvider
        /// or
        /// loggerFactory</exception>
        protected BaseActionFactory(ApplicationSettings applicationSettings, IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
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
        private ILogger Logger { get; }

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
        public abstract IAction Create(Settings.Action actionSettings);

        /// <summary>Gets the repository for the specified feedName.</summary>
        /// <param name="feedName">The name of the feed to get the repository for.</param>
        /// <exception cref="InvalidOperationException">Thrown if the feedName isn't found in the application settings.</exception>
        protected IRepository<Package> GetRepository(string feedName)
        {
            Logger.LogDebug($"Creating repository for feed \"{feedName}\"");
            var feedSettings = ApplicationSettings.Feeds.FirstOrDefault(f => f.Name.Equals(feedName, StringComparison.CurrentCultureIgnoreCase));
            if (feedSettings == null)
                throw new InvalidOperationException($"Feed \"{feedName}\" not found in Feeds application settings");

            var repositoryFactory = ServiceProvider.GetNamedFactory<IRepositoryFactory>(feedSettings.Type);
            var repository = repositoryFactory.Create(feedSettings);
            return repository;
        }
    }
}