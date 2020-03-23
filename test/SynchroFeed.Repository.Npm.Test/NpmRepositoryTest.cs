#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="DirectoryRepositoryTest.cs">
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
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SynchroFeed.Library.Model;
using Xunit;
using Xunit.Abstractions;
using Settings = SynchroFeed.Library.Settings;

namespace SynchroFeed.Repository.Npm.Test
{
    public class NpmRepositoryTest
    {
        const string EnvVar_Url = "NPMTEST_URL";
        const string EnvVar_ApiKey = "NPMTEST_APIKEY";
        private string RootFolder { get; }
        private string LocalRepoFolder { get; }
        private string RepoUrl { get; }
        private string ApiKey { get; }
        private IServiceProvider ServiceProvider { get; }
        private ILoggerFactory LoggerFactory { get; }

        public NpmRepositoryTest(ITestOutputHelper testOutputHelper)
        {
            RootFolder = Environment.CurrentDirectory;
            LocalRepoFolder = Path.Combine(RootFolder, "local.npm");
            RepoUrl = Environment.GetEnvironmentVariable(EnvVar_Url);
            ApiKey = Environment.GetEnvironmentVariable(EnvVar_ApiKey);
            Assert.True(!string.IsNullOrEmpty(RepoUrl), $"No environment variable found for {EnvVar_Url}");
            Assert.True(!string.IsNullOrEmpty(ApiKey), $"No environment variable found for {EnvVar_ApiKey}");

            ServiceProvider = new ServiceCollection()
                .AddLogging(loggingBuilder => { loggingBuilder.AddDebug(); })
                .BuildServiceProvider();

            LoggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
        }

        [IgnoreUnlessIntegrationTest]
        public void Test_NpmRepositoryFactory()
        {
            var appSettings = new Settings.ApplicationSettings();
            var factory = new NpmRepositoryFactory(appSettings, ServiceProvider, LoggerFactory);
            var feed = new Settings.Feed
            {
                Name = "Dummy Feed",
                Type = "Npm"
            };

            var sut = factory.Create(feed);

            Assert.Equal("Npm", sut.RepositoryType);
        }

        [IgnoreUnlessIntegrationTest]
        public void Test_NpmRepository_Uri_Doesnt_Exist()
        {
            var repoFeedConfig = new Settings.Feed
            {
                Name = "npm.notfound"
            };

            repoFeedConfig.Settings.Add("Uri", "http://www.somedummywebsiteurl.com/");

            var sourceRepo = new NpmRepository(repoFeedConfig, LoggerFactory);

            Assert.Throws<WebException>(() => sourceRepo.Fetch(p => p.Id == ""));
        }

        [IgnoreUnlessIntegrationTest]
        public void Test_NpmRepository_Feed_Access_Forbidden()
        {
            var repoFeedConfig = new Settings.Feed
            {
                Name = "npm.test"
            };

            repoFeedConfig.Settings.Add("Uri", RepoUrl);

            var sourceRepo = new NpmRepository(repoFeedConfig, LoggerFactory);

            Assert.Throws<WebException>(() => sourceRepo.Fetch(p => p.Title == ""));
        }

        [IgnoreUnlessIntegrationTest]
        public void Test_NpmRepository_Feed_Doesnt_Exist()
        {
            var repoFeedConfig = new Settings.Feed
            {
                Name = "npm.notfound"
            };

            repoFeedConfig.Settings.Add("Uri", RepoUrl);
            repoFeedConfig.Settings.Add("ApiKey", ApiKey);

            var sourceRepo = new NpmRepository(repoFeedConfig, LoggerFactory);
            Assert.Throws<WebException>(() => sourceRepo.Fetch(p => p.Title == ""));
        }

        [IgnoreUnlessIntegrationTest]
        public void Test_NpmRepository_Fetch_Package_Not_Found()
        {
            var sourceRepoFeedConfig = new Settings.Feed
            {
                Name = "npm.test"
            };
            sourceRepoFeedConfig.Settings.Add("Uri", RepoUrl);
            sourceRepoFeedConfig.Settings.Add("ApiKey", ApiKey);

            var sourceRepo = new NpmRepository(sourceRepoFeedConfig, LoggerFactory);
            var package = new Package { Id = "PackageJunkName@1.0.0", Title = "PackageJunkName", Version = "1.0.0" };
            Assert.Null(sourceRepo.Fetch(package));
        }

        [IgnoreUnlessIntegrationTest]
        public void Test_NpmRepository_Fetch_Package_Found()
        {
            var sourceRepoFeedConfig = new Settings.Feed
            {
                Name = "npm.test"
            };
            sourceRepoFeedConfig.Settings.Add("Uri", RepoUrl);
            sourceRepoFeedConfig.Settings.Add("ApiKey", ApiKey);

            var sourceRepo = new NpmRepository(sourceRepoFeedConfig, LoggerFactory);
            var package = new Package { Id = "vandehey-test@0.0.1", Title = "vandehey-test", Version = "0.0.1" };
            var fetchedPackage = sourceRepo.Fetch(package);
            Assert.NotNull(fetchedPackage);
            Assert.Equal(package.Id, fetchedPackage.Id);
            Assert.Equal(package.Title, fetchedPackage.Title);
            Assert.Equal(package.Version, fetchedPackage.Version);
        }

        [IgnoreUnlessIntegrationTest]
        public void Test_NpmRepository_Fetch_Packages()
        {
            var sourceRepoFeedConfig = new Settings.Feed
            {
                Name = "npm.test",
            };
            sourceRepoFeedConfig.Settings.Add("Uri", RepoUrl);
            sourceRepoFeedConfig.Settings.Add("ApiKey", ApiKey);

            var sourceRepo = new NpmRepository(sourceRepoFeedConfig, LoggerFactory);
            var packages = sourceRepo.Fetch(p => true);
            Assert.NotNull(packages);
            Assert.True(packages.Any());
        }

        [IgnoreUnlessIntegrationTest]
        public void Test_NpmRepository_Copy_And_Delete_Packages()
        {
            var sourceRepoFeedConfig = new Settings.Feed
            {
                Name = "npm.test-source",
            };
            sourceRepoFeedConfig.Settings.Add("Uri", RepoUrl + "-source");
            sourceRepoFeedConfig.Settings.Add("ApiKey", ApiKey);

            var targetRepoFeedConfig = new Settings.Feed
            {
                Name = "npm.test",
            };
            targetRepoFeedConfig.Settings.Add("Uri", RepoUrl);
            targetRepoFeedConfig.Settings.Add("ApiKey", ApiKey);

            var sourceRepo = new NpmRepository(sourceRepoFeedConfig, LoggerFactory);
            var targetRepo = new NpmRepository(targetRepoFeedConfig, LoggerFactory);

            var sourcePackages = sourceRepo.Fetch(p => true).ToArray();
            var targetPackagesCountBefore = targetRepo.Fetch(t => true).Count();

            foreach (var package in sourcePackages)
            {
                var p = sourceRepo.Fetch(package);
                targetRepo.Add(p);
            }

            var targetPackages = targetRepo.Fetch(t => true).ToArray();

            Assert.True(targetPackages.Length > targetPackagesCountBefore);

            // Only delete the packages that were added from the source
            foreach (var targetPackage in sourcePackages)
            {
                targetRepo.Delete(targetPackage);
            }

            targetPackages = targetRepo.Fetch(t => true).ToArray();
            Assert.Equal(targetPackages.Length, targetPackagesCountBefore);
        }
    }
}
