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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SynchroFeed.Library.Model;
using SynchroFeed.Repository.Proget;
using Xunit;
using Settings=SynchroFeed.Library.Settings;

namespace SynchroFeed.Repository.Directory.Test
{
    public class ProgetRepositoryTest
    {
        const string EnvVarUrl = "NUGETTEST_URL";
        const string EnvVarApiKey = "NUGETTEST_APIKEY";
        const string NotepadPlusPlusPackageId = "notepadplusplus";
        const int NotepadPlusPlusPackageCount = 17;
        private string RootFolder { get; }
        private string LocalRepoFolder { get; }
        private string RepoUrl { get; }
        private string ApiKey { get; }
        private IServiceProvider ServiceProvider { get; }
        private ILoggerFactory LoggerFactory { get; }

        public ProgetRepositoryTest()
        {
            RootFolder = Environment.CurrentDirectory;
            LocalRepoFolder = Path.Combine(RootFolder, "local.choco");
            RepoUrl = Environment.GetEnvironmentVariable(EnvVarUrl);
            ApiKey = Environment.GetEnvironmentVariable(EnvVarApiKey);
            Assert.True(!string.IsNullOrEmpty(RepoUrl), $"No environment variable found for {EnvVarUrl}");
            Assert.True(!string.IsNullOrEmpty(ApiKey), $"No environment variable found for {EnvVarApiKey}");

            ServiceProvider = new ServiceCollection()
                .AddLogging(loggingBuilder => { loggingBuilder.AddDebug(); })
                .BuildServiceProvider();

            LoggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
        }

#if DEBUG
        [Fact]
#else
        [Fact(Skip="Integration testing only.")]
#endif
        public void Test_ProgetRepositoryFactory()
        {
            var appSettings = new Settings.ApplicationSettings();
            var factory = new ProgetRepositoryFactory(appSettings, ServiceProvider, LoggerFactory);
            var feed = new Settings.Feed
            {
                Name = "",
                Type = "Proget"
            };

            var sut = factory.Create(feed);

            Assert.Equal("Proget", sut.RepositoryType);
        }

#if DEBUG
        [Fact]
#else
        [Fact(Skip = "Integration testing only.")]
#endif
        public void Test_ProgetRepository_Uri_Doesnt_Exist()
        {
            var repoFeedConfig = new Settings.Feed
            {
                Name = "choco.notfound",
            };

            repoFeedConfig.Settings.Add("Uri", RepoUrl + "-not-found");

            var sourceRepo = new ProgetRepository(repoFeedConfig, LoggerFactory);

            Assert.Throws<WebException>(() => sourceRepo.Fetch(t => t.Id == NotepadPlusPlusPackageId && !t.IsPrerelease));
        }

#if DEBUG
        [Fact]
#else
        [Fact(Skip = "Integration testing only.")]
#endif
        public void Test_ProgetRepository_Fetch_Package_Not_Found()
        {
            var sourceRepoFeedConfig = new Settings.Feed
            {
                Name = "nuget.test",
            };
            sourceRepoFeedConfig.Settings.Add("Uri", RepoUrl);
            sourceRepoFeedConfig.Settings.Add("ApiKey", ApiKey);

            var sourceRepo = new ProgetRepository(sourceRepoFeedConfig, LoggerFactory);
            var package = new Package { Id = "PackageJunkName", Version = "1.0.0" };
            try
            {
                Assert.Null(sourceRepo.Fetch(package));
            }
            catch (Exception)
            {
                // Expected
            }
        }

#if DEBUG
        [Fact]
#else
        [Fact(Skip = "Integration testing only.")]
#endif
        public void Test_ProgetRepository_Copy_And_Delete_Packages()
        {
            var localRepoFeedConfig = new Settings.Feed
            {
                Name = "local.choco",
            };

            localRepoFeedConfig.Settings.Add("Uri", LocalRepoFolder);

            var targetRepoFeedConfig = new Settings.Feed
            {
                Name = "nuget.test",
            };
            targetRepoFeedConfig.Settings.Add("Uri", RepoUrl);
            targetRepoFeedConfig.Settings.Add("ApiKey", ApiKey);

            var sourceRepo = new DirectoryRepository(localRepoFeedConfig, LoggerFactory);
            var targetRepo = new ProgetRepository(targetRepoFeedConfig, LoggerFactory);

            var sourcePackages = sourceRepo.Fetch(t => t.Id == NotepadPlusPlusPackageId && !t.IsPrerelease).ToArray();

            foreach (var package in sourcePackages)
            {
                var p = sourceRepo.Fetch(package);
                Assert.NotNull(p.Content);
                targetRepo.Add(p);
            }

            var targetPackages = targetRepo.Fetch(t => t.Id == NotepadPlusPlusPackageId && !t.IsPrerelease).ToArray();

            Assert.Equal(NotepadPlusPlusPackageCount, sourcePackages.Length);
            Assert.Equal(NotepadPlusPlusPackageCount, targetPackages.Length);

            foreach (var targetPackage in targetPackages)
            {
                var targetPackageWithContent = targetRepo.Fetch(targetPackage);
                Assert.NotNull(targetPackageWithContent.Content);
                targetRepo.Delete(targetPackage);
            }

            targetPackages = targetRepo.Fetch(t => t.Id == NotepadPlusPlusPackageId && !t.IsPrerelease).ToArray();
            Assert.Empty(targetPackages);
        }
    }
}
