#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="BaseEntityRepository.cs">
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
using System.Linq.Expressions;
using SynchroFeed.Library.Model;
using SynchroFeed.Library.Settings;

namespace SynchroFeed.Library.Repository
{
    public abstract class BaseEntityRepository : IRepository<Package>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseEntityRepository" /> class.
        /// </summary>
        /// <param name="feedSettings">The feed settings.</param>
        /// <exception cref="ArgumentNullException">Thrown if feedConfig is null</exception>
        protected BaseEntityRepository(Feed feedSettings)
        {
            FeedSettings = feedSettings ?? throw new ArgumentNullException(nameof(feedSettings));
            Name = feedSettings.Name;
            Settings = feedSettings.Settings;
        }

        /// <summary>
        /// Gets or sets the feed settings associated with this repository.
        /// </summary>
        /// <value>The feed settings associated with this repository.</value>
        public Feed FeedSettings { get; set; }

        /// <summary>
        /// Gets the name of the repository.
        /// </summary>
        /// <value>The name of the repository.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the type of the repository.
        /// </summary>
        /// <value>The type of the repository.</value>
        public abstract string RepositoryType { get; }

        protected Dictionary<string, string> Settings { get; }

        /// <summary>
        /// A helper property that gets the URI from the Settings.
        /// </summary>
        /// <value>The URI or null if the URI is not found.</value>
        protected string Uri
        {
            get
            {
                const string UriSettingName = "Uri";
                Settings.TryGetValue(UriSettingName, out var uri);
                return uri;
            }
        }

        /// <summary>
        /// Adds the specified package to the repository.
        /// </summary>
        /// <param name="package">The package to add to the repository.</param>
        public abstract void Add(Package package);

        /// <summary>
        /// Deletes the specified package from the repository.
        /// </summary>
        /// <param name="package">The package to delete from the repository.</param>
        public abstract void Delete(Package package);

        /// <summary>
        /// Fetches all of the packages from the repository that matches the expression.
        /// </summary>
        /// <param name="expression">The expression to filter the packages.</param>
        /// <returns>IEnumerable&lt;Package&gt;.</returns>
        /// <value>Returns the IQueryable query instance.</value>
        public abstract IEnumerable<Package> Fetch(Expression<Func<Package, bool>> expression);

        /// <summary>
        /// Fetches the specified package from the repository.
        /// </summary>
        /// <param name="entity">The package to fetch from the repository. The package is
        /// used to determine the keys to fetch the entity from the repository.</param>
        /// <returns>Returns the Package or null if the package is not found.</returns>
        public abstract Package Fetch(Package entity);
    }
}