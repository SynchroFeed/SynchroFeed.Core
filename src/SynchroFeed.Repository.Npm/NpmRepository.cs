#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="NpmRepository.cs">
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
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using SynchroFeed.Library.Repository;
using NpmPackage = SynchroFeed.Repository.Npm.Model.NpmPackage;
using Package = SynchroFeed.Library.Model.Package;

namespace SynchroFeed.Repository.Npm
{
    /// <summary>
    /// The NpmRepository is an implementation of IRepository for accessing
    /// packages at a NPM compatible web service.
    /// </summary>
    /// <seealso cref="SynchroFeed.Library.Repository.BaseEntityRepository" />
    public class NpmRepository : BaseEntityRepository
    {
        const string ApiKeyHeaderName = "X-ApiKey";

        private NpmClient client;

        /// <summary>Initializes a new instance of the <see cref="T:SynchroFeed.Repository.Npm.NugetRepository"/> class.</summary>
        /// <param name="feedSettings">The feed settings to initialize this repository.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <exception cref="ArgumentNullException">Thrown if feedConfig is null</exception>
        public NpmRepository(Library.Settings.Feed feedSettings, ILoggerFactory loggerFactory)
            : base(feedSettings)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            Logger = loggerFactory.CreateLogger<NpmRepository>();
        }

        /// <summary>
        /// Gets the type of the repository.
        /// </summary>
        /// <value>The type of the repository.</value>
        public override string RepositoryType => "Npm";

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private ILogger Logger { get; }

        private NpmClient Client
        {
            get { return client ?? (client = new NpmClient(this, Uri, ApiKey, Name, Logger)); }
        }

        /// <summary>
        /// Adds the specified package to the repository.
        /// </summary>
        /// <param name="package">The package to add to the repository.</param>
        /// <exception cref="ArgumentNullException">Thrown if the package is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the Content property of the package is null.</exception>
        /// <exception cref="HttpRequestException">Thrown if the add operation is unsuccessful.</exception>
        public override void Add(Package package)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));

            NpmPackage npmPackage = package.ToNpmPackage();
            var (_, error) = Client.NpmAddPackageAsync(npmPackage, package.Content).Result;
            if (error != null)
                throw new WebException(error.ErrorMessage);
        }

        /// <summary>
        /// Deletes the specified package from the repository.
        /// </summary>
        /// <param name="package">The package to delete from the repository.</param>
        /// <exception cref="ArgumentNullException">Thrown if the package is null.</exception>
        /// <exception cref="HttpRequestException">Thrown if the delete operation is unsuccessful.</exception>
        public override void Delete(Package package)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));

            var error = Client.NpmDeletePackageAsync(package).Result;
            if (error != null)
                throw new WebException(error.ErrorMessage);
        }

        /// <summary>
        /// Fetches all of the packages from the repository that matches the expression.
        /// </summary>
        /// <param name="expression">The expression to filter the packages.</param>
        /// <returns>IEnumerable&lt;Package&gt;.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the expression is null.</exception>
        /// <exception cref="WebException">Thrown if an error occurs with the communication to the web server.</exception>
        /// <value>Returns the IEnumerable query instance.</value>
        public override IEnumerable<Package> Fetch(Expression<Func<Package, bool>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            var (packages, error) = Client.NpmFetchPackagesAsync(expression, false).Result;
            if (error != null)
            {
                throw new WebException(error.ErrorMessage);
            }

            return packages;
        }

        /// <summary>
        /// Fetches the specified package and its content from the repository.
        /// </summary>
        /// <param name="package">The package to fetch from the repository. The package is
        /// used to determine the keys to fetch the entity from the repository.</param>
        /// <returns>Returns the Package with the package contents or null if the package is not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the package is null.</exception>
        /// <exception cref="WebException">Thrown if an error occurs with the communication to the web server.</exception>
        public override Package Fetch(Package package)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));

            var (foundNpmPackage, error) = Client.NpmGetPackageAsync(package.Id, package.Version).Result;
            if (error != null)
            {
                if (error.StatusCode == HttpStatusCode.NotFound)
                    return null;
                throw new WebException(error.ErrorMessage);
            }

            var foundPackage = foundNpmPackage.ToPackage();
            var response = Client.NpmDownloadPackageAsync(foundPackage).Result;
            if (response.Error != null)
                throw new WebException(response.Error.ErrorMessage);

            foundPackage.Content = response.Content;
            return foundPackage;
        }

        /// <summary>
        /// A helper property that gets the API key from the Settings.
        /// </summary>
        /// <value>The API key or null if the API key is not found.</value>
        protected string ApiKey
        {
            get
            {
                const string ApiKeySettingName = "ApiKey";
                Settings.TryGetValue(ApiKeySettingName, out var apiKey);
                return apiKey;
            }
        }
    }
}
