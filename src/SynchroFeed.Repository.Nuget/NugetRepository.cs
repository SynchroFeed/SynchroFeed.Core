#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="NugetRepository.cs">
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
using System.Data.Services.Client;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SynchroFeed.Library;
using SynchroFeed.Library.Repository;
using Package = SynchroFeed.Library.Model.Package;

namespace SynchroFeed.Repository.Nuget
{
    /// <summary>
    /// The NugetRepository is an implementation of IRepository for accessing
    /// packages at a NuGet compatible OData web service.
    /// </summary>
    /// <seealso cref="SynchroFeed.Library.Repository.BaseEntityRepository" />
    public class NugetRepository : BaseEntityRepository
    {
        /// <summary>A constant value that contains the name of the header that contains the Nuget API key.</summary>
        public const string ApiKeyHeaderName = "X-NUGET-APIKEY";

        /// <summary>Initializes a new instance of the <see cref="T:SynchroFeed.Repository.Nuget.NugetRepository"/> class.</summary>
        /// <param name="feedSettings">The feed settings to initialize this repository.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <exception cref="ArgumentNullException">Thrown if feedConfig is null</exception>
        public NugetRepository(Library.Settings.Feed feedSettings, ILoggerFactory loggerFactory)
            : base(feedSettings)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            Logger = loggerFactory.CreateLogger<NugetRepository>();
        }

        /// <summary>
        /// Gets the type of the repository.
        /// </summary>
        /// <value>The type of the repository.</value>
        public override string RepositoryType => "Nuget";

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private ILogger Logger { get; }

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

            if (package.Content == null)
                throw new InvalidOperationException("The package doesn't have any content associated with it.");

            var existingPackage = Fetch(p => p.Id == package.Id && p.Version == package.Version).FirstOrDefault();
            if (existingPackage != null)
            {
                Logger.LogWarning($"Package already exists ({package.Id}). Ignoring.");
                return;
            }

            var request = new HttpRequestMessage(HttpMethod.Put, new Uri(Uri).Combine("package/"));
            request.Headers.Authorization = AuthorizationHeader;
            if (!string.IsNullOrEmpty(ApiKey))
            {
                request.Headers.Add(ApiKeyHeaderName, ApiKey);
            }

            using var multiPartContent = new MultipartFormDataContent();
            using var fileContent = new ByteArrayContent(package.Content);
            fileContent.Headers.Add("Content-Type", "application/octet-stream");
            multiPartContent.Add(fileContent, "file", package.Id);
            request.Content = multiPartContent;
            var response = HttpClientFactory.GetHttpClient().SendAsync(request);
            response.Wait();
            response.Result.EnsureSuccessStatusCode();
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

            var request = new HttpRequestMessage(HttpMethod.Delete, new Uri(Uri).Combine($"package/{package.Id}/{package.Version}"));
            request.Headers.Authorization = AuthorizationHeader;
            if (!string.IsNullOrEmpty(ApiKey))
            {
                request.Headers.Add(ApiKeyHeaderName, ApiKey);
            }
            var response = HttpClientFactory.GetHttpClient().SendAsync(request);
            response.Wait();
            response.Result.EnsureSuccessStatusCode();
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

            var context = DataServiceContext();
            var packages = GetQueryResponse(context, expression).ToArray();
            PreProcessPackages(context, packages);
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

            var context = DataServiceContext();
            var entity = GetQueryResponse(context, t => t.Id == package.Id && t.Version == package.Version).FirstOrDefault();
            if (entity == null)
                return null;
            entity.Content = GetContentForPackage(context, entity);
            entity.PackageDownloadUrl = GetPackageDownloadUrl(context, entity);
            entity.PackageUrl = GetPackageUrl(context, entity);

            return entity;

            byte[] GetContentForPackage(DataServiceContext dataServiceContext, Package packageEntity)
            {
                using DataServiceStreamResponse response = dataServiceContext.GetReadStreamAsync(packageEntity, new DataServiceRequestArgs(), null).Result;
                if (response != null)
                {
                    long packageSize = long.Parse(response.Headers["Content-Length"]);
                    using var byteStream = new MemoryStream(new byte[packageSize], true);
                    using var stream = response.Stream;
                    stream.CopyTo(byteStream);

                    return byteStream.ToArray();
                }

                return null;
            }
        }

        /// <summary>
        /// The PreProcessPackages method allows an inherited class to do
        /// some preprocessing like adding a Url to the package before returning.
        /// </summary>
        /// <param name="context">The DataServiceContext of the packages.</param>
        /// <param name="packages">The packages returned from the DataServiceContext.</param>
        protected virtual void PreProcessPackages(DataServiceContext context, IEnumerable<Package> packages)
        {
            foreach (var package in packages)
            {
                PreProcessPackage(context, package);
            }
        }

        /// <summary>
        /// The PreProcessPackages method allows an inherited class to do
        /// some preprocessing like adding a Url to the package before returning.
        /// </summary>
        /// <param name="context">The DataServiceContext of the packages.</param>
        /// <param name="package">The package returned from the DataServiceContext.</param>
        protected virtual void PreProcessPackage(DataServiceContext context, Package package)
        {
            package.PackageDownloadUrl = GetPackageDownloadUrl(context, package);
            if (package.PackageDownloadUrl != null)
                package.PackageUrl = GetPackageUrl(context, package);
        }

        /// <summary>
        /// Gets the URL to the packages download on the feed.
        /// </summary>
        /// <param name="context">The DataServiceContext of the packages.</param>
        /// <param name="package">The package returned from the DataServiceContext to get the URL for.</param>
        /// <returns>System.String.</returns>
        protected virtual string GetPackageDownloadUrl(DataServiceContext context, Package package)
        {
            return context.GetReadStreamUri(package)?.OriginalString;
        }

        /// <summary>
        /// Gets the URL to the packages web page on the feed.
        /// </summary>
        /// <param name="context">The DataServiceContext of the packages.</param>
        /// <param name="package">The package returned from the DataServiceContext to get the URL for.</param>
        /// <returns>System.String.</returns>
        protected virtual string GetPackageUrl(DataServiceContext context, Package package)
        {
            // The following will probably only work for packages actually from nuget.org
            return $"{new Uri(package.PackageDownloadUrl).GetLeftPart(UriPartial.Authority)}/packages/{package.Id}/{package.Version}";
        }

        /// <summary>
        /// Gets the query response for the data service context and expression
        /// </summary>
        /// <param name="context">The data service context to use to execute the query.</param>
        /// <param name="query">The expression to use to query against the data service context.</param>
        /// <returns>IEnumerable&lt;Package&gt;.</returns>
        /// <exception cref="WebException">Thrown if an error occurs with the communication to the web server.</exception>
        /// <exception cref="DataServiceClientException">Thrown if a unknown DataService client error is encountered.</exception>
        protected virtual IEnumerable<Package> GetQueryResponse(DataServiceContext context, Expression<Func<Package, bool>> query)
        {
            var entityQuery = (DataServiceQuery<Package>)context.CreateQuery<Package>("Packages")
                .Where(query);
            try
            {
                var response = entityQuery.ExecuteAsync(null).Result;

                return response.AsEnumerable();
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    if (e is DataServiceQueryException dsqEx)
                    {
                        switch (dsqEx.Response.StatusCode)
                        {
                            case 401:
                                throw new WebException("Access unauthorized (401)", ex);
                            case 404:
                                return new List<Package>();
                            default:
                                throw new WebException($"Unexpected Exception ({dsqEx.Response.StatusCode})", ex);
                        }
                    }
                    if (e is DataServiceClientException dscEx)
                    {
                        switch (dscEx.StatusCode)
                        {
                            case 401:
                                throw new WebException("Access unauthorized (401)", ex);
                            case 404:
                                return new List<Package>();
                            default:
                                throw new WebException($"Unexpected Exception ({dscEx.StatusCode})", ex);
                        }
                    }

                    throw new WebException($"Unknown Exception: {e.Message}", e);
                }
            }

            // It should never get here
            return null;
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

        /// <summary>
        /// A helper property that gets the Username from the Settings.
        /// </summary>
        /// <value>The Username or null if the Username is not found.</value>
        protected string Username
        {
            get
            {
                const string UsernameSettingName = "Username";
                Settings.TryGetValue(UsernameSettingName, out var username);
                return username;
            }
        }

        /// <summary>
        /// A helper property that gets the Password from the Settings.
        /// </summary>
        /// <value>The Password or null if the Password is not found.</value>
        protected string Password
        {
            get
            {
                const string UsernameSettingName = "Password";
                Settings.TryGetValue(UsernameSettingName, out var password);
                return password;
            }
        }

        /// <summary>  Gets the DataServiceContext that is used to access the atom repository.</summary>
        /// <returns>DataServiceContext that is used to access the atom repository.</returns>
        protected virtual DataServiceContext DataServiceContext()
        {
            var context = new DataServiceContext(new Uri(Uri))
            {
                IgnoreMissingProperties = true
            };

            if (!string.IsNullOrEmpty(Username) || !string.IsNullOrEmpty(Password))
                context.Credentials = new NetworkCredential(Username, Password);

            context.SendingRequest2 += OnSendingRequest;
            context.ReadingEntity += OnReadingDataContextEntity;
            context.WritingEntity += OnWritingDataContextEntity;

            return context;
        }

        private void OnSendingRequest(object sender, SendingRequest2EventArgs args)
        {
            if (!string.IsNullOrEmpty(ApiKey))
            {
                args.RequestMessage.SetHeader(ApiKeyHeaderName, ApiKey);
            }
        }

        /// <summary>
        /// A virtual method that allows overriding what occurs on reading the entity.
        /// </summary>
        /// <param name="sender">The sender (DataServiceContext) of the event.</param>
        /// <param name="args">The <see cref="ReadingWritingEntityEventArgs"/> instance containing the event data.</param>
        protected virtual void OnReadingDataContextEntity(object sender, ReadingWritingEntityEventArgs args)
        {
            // Do nothing - available for override
            // Sure wish I could put the logic for assigning the PackageUrl here
            // but unfortunately, the context (sender) isn't tracking the entity
            // at this point.
        }

        /// <summary>
        /// A virtual method that allows overriding what occurs on writing the entity.
        /// </summary>
        /// <param name="sender">The sender (DataServiceContext) of the event.</param>
        /// <param name="args">The <see cref="ReadingWritingEntityEventArgs" /> instance containing the event data.</param>
        private void OnWritingDataContextEntity(object sender, ReadingWritingEntityEventArgs args)
        {
            // Do nothing - available for override
        }

        /// <summary>Gets the authorization header to add to the web request.</summary>
        /// <value>The authorization header to add to the web request.</value>
        protected virtual AuthenticationHeaderValue AuthorizationHeader
        {
            get
            {
                if (!string.IsNullOrEmpty(Username) || !string.IsNullOrEmpty(Password))
                    return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{Username}:{Password}")));

                return null;
            }
        }
    }
}
