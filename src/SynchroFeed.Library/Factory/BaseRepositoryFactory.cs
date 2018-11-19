#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="BaseRepositoryFactory.cs">
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
using SynchroFeed.Library.Model;
using SynchroFeed.Library.Repository;
using SynchroFeed.Library.Settings;

namespace SynchroFeed.Library.Factory
{
    /// <summary>
    /// The RepositoryFactory class is a static class that is used to create a
    /// package repository for a configured feed.
    /// </summary>
    public abstract class BaseRepositoryFactory : IRepositoryFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseRepositoryFactory" /> class.
        /// </summary>
        /// <param name="feedType">Type of the feed.</param>
        /// <param name="applicationSettings">The settings that have been configured for this application.</param>
        /// <param name="serviceProvider">The dependency injection service provider.</param>
        /// <param name="loggingFactory">The logging factory instance.</param>
        /// <exception cref="System.ArgumentNullException">
        /// applicationSettings
        /// or
        /// serviceProvider
        /// or
        /// loggingFactory
        /// </exception>
        /// <exception cref="ArgumentNullException">applicationSettings
        /// or
        /// serviceProvider
        /// or
        /// loggingFactory</exception>
        protected BaseRepositoryFactory(
            string feedType,
            ApplicationSettings applicationSettings,
            IServiceProvider serviceProvider,
            ILoggerFactory loggingFactory)
        {
            ApplicationSettings = applicationSettings ?? throw new ArgumentNullException(nameof(applicationSettings));
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            if (loggingFactory == null) throw new ArgumentNullException(nameof(loggingFactory));
            Logger = loggingFactory.CreateLogger<BaseRepositoryFactory>();
            Type = feedType;
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
        /// Gets the type of the repository.
        /// </summary>
        /// <value>The type of the repository.</value>
        public string Type { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private ILogger Logger { get; }

        /// <summary>
        /// A factory method that creates the specified repository.
        /// </summary>
        /// <param name="feedSettings">The settings that are used to initialize the repository.</param>
        /// <returns>Returns an instance of IRepository&lt;Package&gt; for the specified repository type.</returns>
        /// <exception cref="ArgumentNullException">feedSettings</exception>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        public abstract IRepository<Package> Create(Feed feedSettings);

        /// <summary>
        /// A protected factory method that does the actual work of creating the specified feed.
        /// </summary>
        /// <typeparam name="T">The type of repository to create.</typeparam>
        /// <param name="feedSettings">The feed settings to initialize the repository.</param>
        /// <returns>IRepository&lt;Package&gt;.</returns>
        /// <exception cref="System.ArgumentNullException">feedSettings</exception>
        /// <exception cref="System.InvalidOperationException">If no repository can be found based on the feed type.</exception>
        protected IRepository<Package> Create<T>(Feed feedSettings)
            where T : IRepository<Package>
        {
            if (feedSettings == null) throw new ArgumentNullException(nameof(feedSettings));

            Debug.Assert(feedSettings.Type.Equals(Type, StringComparison.CurrentCultureIgnoreCase));

            Logger.LogDebug($"Creating Nuget Repository named \"{feedSettings.Name}\"");
            var repository = ActivatorUtilities.CreateInstance<T>(
                                                                  ServiceProvider,
                                                                  feedSettings.CloneAndMergeSettings(ApplicationSettings.SettingsGroups.Find(feedSettings.SettingsGroup)));

            return repository;
        }
    }
}