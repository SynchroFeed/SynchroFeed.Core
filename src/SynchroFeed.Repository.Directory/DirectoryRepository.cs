#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="DirectoryRepository.cs">
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
using System.IO;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using SynchroFeed.Library.Model;
using SynchroFeed.Library.Repository;

namespace SynchroFeed.Repository.Directory
{
    /// <summary>
    /// The DirectoryRepository class implements the <see cref="IRepository{TEntity}" /> interface
    /// for a nuget repository in a directory.
    /// </summary>
    /// <seealso cref="SynchroFeed.Library.Repository.BaseEntityRepository" />
    public class DirectoryRepository : BaseEntityRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryRepository" /> class.
        /// </summary>
        /// <param name="feedSettings">The feed settings to initialize the repository.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public DirectoryRepository(Library.Settings.Feed feedSettings, ILoggerFactory loggerFactory)
            : base(feedSettings)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            Logger = loggerFactory.CreateLogger<DirectoryRepository>();
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private ILogger Logger { get; }

        /// <summary>
        /// Gets the type of the repository.
        /// </summary>
        /// <value>The type of the repository.</value>
        public override string RepositoryType => "Directory";


        /// <summary>
        /// Fetches all of the packages from the repository that matches the expression.
        /// </summary>
        /// <param name="expressionFunc">The expression to filter the packages.</param>
        /// <returns>IEnumerable&lt;Package&gt;.</returns>
        /// <value>Returns the IQueryable query instance.</value>
        public override IEnumerable<Package> Fetch(Expression<Func<Package, bool>> expressionFunc)
        {
            if (expressionFunc == null)
                throw new ArgumentNullException(nameof(expressionFunc));

            if (!System.IO.Directory.Exists(Uri))
                yield break;

            var files = System.IO.Directory.GetFiles(Uri);
            var expression = expressionFunc.Compile();
            foreach (var file in files)
            {
                var package = Package.CreateFromFile(file);
                if (expression(package))
                    yield return package;
            }
        }

        /// <summary>
        /// Fetches the specified package from the repository.
        /// </summary>
        /// <param name="package">The package to fetch from the repository. The package is
        /// used to determine the keys to fetch the entity from the repository.</param>
        /// <returns>Returns the Package with the package contents or null if the package is not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the package is null.</exception>
        public override Package Fetch(Package package)
        {
            var filename = Path.Combine(Uri, $"{package.Id}.{package.Version}.nupkg");
            if (!File.Exists(filename))
                return null;

            return Package.CreateFromFile(filename);
        }

        /// <summary>
        /// Adds the specified package to the repository.
        /// </summary>
        /// <param name="package">The package to add to the repository.</param>
        public override void Add(Package package)
        {
            if (!System.IO.Directory.Exists(Uri))
            {
                Logger.LogWarning($"Local repository directory doesn't exist. Attempting to create {Uri}");
                System.IO.Directory.CreateDirectory(Uri);
            }

            var filename = Path.Combine(Uri, $"{package.Id}.{package.Version}.nupkg");
            if (File.Exists(filename))
            {
                //TODO - Should check for a setting to overwrite
                Logger.LogWarning($"Package already exists ({filename}). Ignoring.");
                return;
            }

            Logger.LogTrace($"Creating Package file {filename}");
            using var outStream = File.Create(filename);
            outStream.WriteAsync(package.Content, 0, package.Content.Length).Wait();
        }

        /// <summary>
        /// Deletes the specified package from the repository.
        /// </summary>
        /// <param name="package">The package to delete from the repository.</param>
        public override void Delete(Package package)
        {
            Logger.LogDebug($"Deleting Package: {package.Id}.{package.Version} from {Name}");

            Logger.LogDebug($"Processing {package.Id}.{package.Version}...");
            var filename = Path.Combine(Uri, $"{package.Id}.{package.Version}.nupkg");
            if (File.Exists(filename))
            {
                Logger.LogDebug($"Deleting Package file {filename}");
                File.Delete(filename);
            }
        }
    }
}
