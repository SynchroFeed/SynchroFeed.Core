#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="ProgetRepository.cs">
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
using System.Data.Services.Client;
using Microsoft.Extensions.Logging;
using SynchroFeed.Library.Model;
using Settings=SynchroFeed.Library.Settings;
using SynchroFeed.Repository.Nuget;

namespace SynchroFeed.Repository.Proget
{
    /// <summary>
    /// The ProgetRepository is a specialization of the NugetRepository with specific functionality
    /// Proget.
    /// </summary>
    public class ProgetRepository : NugetRepository
    {
        /// <summary>Initializes a new instance of the <see cref="T:SynchroFeed.Repository.Proget.ProgetRepository"/> class.</summary>
        /// <param name="feedSettings">The feed settings.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public ProgetRepository(Settings.Feed feedSettings, ILoggerFactory loggerFactory) 
            : base(feedSettings, loggerFactory)
        {
        }

        /// <summary>
        /// Gets the type of the repository.
        /// </summary>
        /// <value>The type of the repository.</value>
        public override string RepositoryType => "Proget";

        /// <summary>
        /// Gets the Proget URL to the packages web page on the feed.
        /// </summary>
        /// <param name="context">The DataServiceContext of the packages.</param>
        /// <param name="package">The package returned from the DataServiceContext to get the URL for.</param>
        /// <returns>System.String.</returns>
        protected override string GetPackageUrl(DataServiceContext context, Package package)
        {
            // Not sure if this works for all feed types but it works for nuget and choco feeds
            // Pretty sure it doesn't work with npm feeds
            var uri = new Uri(package.PackageDownloadUrl);
            return package.PackageDownloadUrl.Replace($"{uri.Segments[0]}{uri.Segments[1]}", "/feeds/").Replace("/package/", "/");
        }
    }
}