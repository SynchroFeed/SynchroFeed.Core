#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="ProgetRepositoryFactory.cs">
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
using SynchroFeed.Library.Factory;
using SynchroFeed.Library.Model;
using SynchroFeed.Library.Repository;
using SynchroFeed.Library.Settings;

namespace SynchroFeed.Repository.Proget
{
    /// <summary>
    /// The NugetRepositoryFactory class is a factory class that is used to create a
    /// Nuget package repository for the configured feed.
    /// </summary>
    public class ProgetRepositoryFactory : BaseRepositoryFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgetRepositoryFactory"/> class.
        /// </summary>
        /// <param name="applicationSettings">The settings that have been configured for this application.</param>
        /// <param name="serviceProvider">The dependency injection service provider.</param>
        /// <param name="loggingFactory">The logging factory instance.</param>
        /// <exception cref="ArgumentNullException">
        /// applicationSettings
        /// or
        /// serviceProvider
        /// or
        /// loggingFactory
        /// </exception>
        public ProgetRepositoryFactory(ApplicationSettings applicationSettings, IServiceProvider serviceProvider, ILoggerFactory loggingFactory)
            : base("Proget", applicationSettings, serviceProvider, loggingFactory)
        {
        }

        /// <summary>
        /// A factory method that creates the specified repository.
        /// </summary>
        /// <param name="feedSettings">The settings that are used to initialize the repository.</param>
        /// <returns>Returns an instance of IRepository&lt;Package&gt; for the specified repository type.</returns>
        /// <exception cref="ArgumentNullException">feedSettings</exception>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        public override IRepository<Package> Create(Library.Settings.Feed feedSettings)
        {
            return Create<ProgetRepository>(feedSettings);
        }
    }
}
