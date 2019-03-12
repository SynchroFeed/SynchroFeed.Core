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
using System.Data;
using System.Data.Services.Client;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
        public const string ApiKeyHeaderName = "X-NUGET-APIKEY";

        /// <summary>
        /// Initializes a new instance of the <see cref="NugetRepository" /> class.
        /// </summary>
        /// <param name="feedSettings">The feed settings to initialize this repository.</param>
        /// <exception cref="ArgumentNullException">Thrown if feedConfig is null</exception>
        public NugetRepository(Library.Settings.Feed feedSettings)
            : base(feedSettings)
        {
        }

        /// <summary>
        /// Gets the type of the repository.
        /// </summary>
        /// <value>The type of the repository.</value>
        public override string RepositoryType => "Nuget";

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

            var request = new HttpRequestMessage(HttpMethod.Put, new Uri(Uri).Combine("package/"));
            request.Headers.Authorization = AuthorizationHeader;
            if (!string.IsNullOrEmpty(ApiKey))
            {
                request.Headers.Add(ApiKeyHeaderName, ApiKey);
            }

            using (var multiPartContent = new MultipartFormDataContent())
            {
                using (var fileContent = new ByteArrayContent(package.Content))
                {
                    fileContent.Headers.Add("Content-Type", "application/octet-stream");
                    multiPartContent.Add(fileContent, "file", package.Id);
                    request.Content = multiPartContent;
                    var response = HttpClientFactory.GetHttpClient().SendAsync(request);
                    response.Wait();
                    response.Result.EnsureSuccessStatusCode();
                }
            }
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
        /// <exception cref="ObjectNotFoundException">Throws ObjectNotFoundException if the package was not found in the repo.</exception>
        /// <exception cref="WebException">Thrown if an error occurs with the communication to the web server.</exception>
        public override Package Fetch(Package package)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));

            try
            {
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
                    using (var webresponse = dataServiceContext.GetReadStream(packageEntity))
                    {
                        if (webresponse != null)
                        {
                            using (var byteStream = new MemoryStream(new byte[packageEntity.PackageSize], true))
                            {
                                using (var stream = webresponse.Stream)
                                {
                                    stream.CopyTo(byteStream);
                                }

                                return byteStream.ToArray();
                            }
                        }
                    }

                    return null;
                }

            }
            catch (DataServiceClientException ex)
            {
                throw new ObjectNotFoundException("Package not found", ex);
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
        protected virtual IEnumerable<Package> GetQueryResponse(DataServiceContext context, Expression<Func<Package, bool>> query)
        {
            const string ODataContentType = "application/atom+xml";

            var entityQuery = (DataServiceQuery<Package>)context.CreateQuery<Package>("Packages")
                .Where(query);
            try
            {
                var response = (QueryOperationResponse<Package>)entityQuery.Execute();
                // The response sometimes comes back as a 200 but the content type is an error page from a proxy server
                var contentType = response?.Headers["Content-Type"];
                if (contentType == null || !contentType.Equals(ODataContentType, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new WebException($"Unexpected content type. Expected {ODataContentType}. Response was {contentType}.");
                }

                return response.AsEnumerable();

            }
            catch (DataServiceQueryException ex)
            {
                if (ex.Response.StatusCode == 404)
                    return new List<Package>();
                throw;
            }
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

        /// <summary>
        /// A helper property that gets the Web Service Proxy from the Settings.
        /// </summary>
        /// <value>The Proxy or null if the Proxy is not found.</value>
        protected string Proxy
        {
            get
            {
                const string ProxySettingName = "Proxy";
                Settings.TryGetValue(ProxySettingName, out var proxy);
                return proxy;
            }
        }

        protected virtual DataServiceContext DataServiceContext()
        {
            var context = new DataServiceContext(new Uri(Uri))
            {
                IgnoreMissingProperties = true,
                Timeout = 100000
            };

            if (!string.IsNullOrEmpty(Username) || !string.IsNullOrEmpty(Password))
                context.Credentials = new NetworkCredential(Username, Password);

            context.SendingRequest += OnSendingDataContextRequest;
            context.ReadingEntity += OnReadingDataContextEntity;
            context.WritingEntity += OnWritingDataContextEntity;

            return context;
        }

        /// <summary>
        /// A virtual method that allows overriding what occurs before sending the request.
        /// </summary>
        /// <param name="sender">The sender (DataServiceContext) of the event.</param>
        /// <param name="args">The <see cref="SendingRequestEventArgs"/> instance containing the event data.</param>
        protected virtual void OnSendingDataContextRequest(object sender, SendingRequestEventArgs args)
        {
            if (!string.IsNullOrEmpty(ApiKey))
            {
                args.RequestHeaders.Add(ApiKeyHeaderName, ApiKey);
            }

            if (!string.IsNullOrEmpty(Proxy))
                args.Request.Proxy = new WebProxy(Proxy);
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

        protected virtual WebRequest CreateNewRequest(HttpWebRequest argsRequest, string newUrl)
        {
            HttpWebRequest newRequest = (HttpWebRequest) WebRequest.Create(newUrl);
            newRequest.Credentials = argsRequest.Credentials;
            newRequest.UserAgent = argsRequest.UserAgent;
            newRequest.Timeout = argsRequest.Timeout;
            foreach (string header in argsRequest.Headers)
            {
                if (!header.Equals("User-Agent", StringComparison.CurrentCultureIgnoreCase))
                {
                    var value = argsRequest.Headers.Get(header);
                    newRequest.Headers.Add(header, value);
                }
            }
            return newRequest;
        }

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
