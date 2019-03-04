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
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SynchroFeed.Library.Model;
using Xunit;
using Settings=SynchroFeed.Library.Settings;

namespace SynchroFeed.Repository.Directory.Test
{
    public class DirectoryRepositoryTest : IDisposable
    {
        const string NotepadPlusPlusPackageId = "notepadplusplus";
        const int NotepadPlusPlusPackageCount = 17;
        const int NotepadPlusPlusPrereleasePackageCount = 1;
        private string RootFolder { get; }
        private string LocalRepoFolder { get; }
        private string TempRepoFolder { get; }
        private IServiceProvider ServiceProvider { get; }
        private ILoggerFactory LoggerFactory { get; }

        public DirectoryRepositoryTest()
        {
            RootFolder = Environment.CurrentDirectory;
            LocalRepoFolder = Path.Combine(RootFolder, "local.choco");
            TempRepoFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            ServiceProvider = new ServiceCollection()
                .AddLogging(loggingBuilder => { loggingBuilder.AddDebug(); })
                .BuildServiceProvider();

            LoggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
        }

        [Fact]
        public void Test_DirectoryRepositoryFactory()
        {
            var appSettings = new Settings.ApplicationSettings();
            var factory = new DirectoryRepositoryFactory(appSettings, ServiceProvider, LoggerFactory);
            var feed = new Settings.Feed { Name = "", Type = "Directory"};

            var sut = factory.Create(feed);

            Assert.Equal("Directory", sut.RepositoryType);
        }

        [Fact]
        public void Test_DirectoryRepository_Uri_Doesnt_Exist()
        {
            var localRepoFeedConfig = new Settings.Feed
            {
                Name = "local.choco.notfound",
            };

            localRepoFeedConfig.Settings.Add("Uri", LocalRepoFolder + "-not-found");

            var sourceRepo = new DirectoryRepository(localRepoFeedConfig, LoggerFactory);

            var packages = sourceRepo.Fetch(t => t.Id == NotepadPlusPlusPackageId && !t.IsPrerelease);

            Assert.Empty(packages);
        }

        [Fact]
        public void Test_DirectoryRepository_Fetch_Package_Not_Found()
        {
            var localRepoFeedConfig = new Settings.Feed
            {
                Name = "local.choco",
            };

            localRepoFeedConfig.Settings.Add("Uri", LocalRepoFolder);

            var sourceRepo = new DirectoryRepository(localRepoFeedConfig, LoggerFactory);
            var package = new Package { Id = "PackageJunkName", Version = "1.0.0"};
            Assert.Throws<ObjectNotFoundException>(() => sourceRepo.Fetch(package));
        }

        [Fact]
        public void Test_DirectoryRepository_Fetch_With_Expression()
        {
            var localRepoFeedConfig = new Settings.Feed
            {
                Name = "local.choco",
            };

            localRepoFeedConfig.Settings.Add("Uri", LocalRepoFolder);

            var sourceRepo = new DirectoryRepository(localRepoFeedConfig,LoggerFactory);

            var packages = sourceRepo.Fetch(t => t.Id == NotepadPlusPlusPackageId && !t.IsPrerelease);
            var packagesPrerelease = sourceRepo.Fetch(t => t.Id == NotepadPlusPlusPackageId && t.IsPrerelease);
            var packagesIncludingPrerelease = sourceRepo.Fetch(t => t.Id == NotepadPlusPlusPackageId);

            Assert.Equal(NotepadPlusPlusPackageCount, packages.Count());
            Assert.Equal(NotepadPlusPlusPrereleasePackageCount, (int) packagesPrerelease.Count());
            Assert.Equal(NotepadPlusPlusPackageCount + NotepadPlusPlusPrereleasePackageCount, packagesIncludingPrerelease.Count());
        }

        [Fact]
        public void Test_DirectoryRepository_Copy_And_Delete_Packages()
        {
            var localRepoFeedConfig = new Settings.Feed
            {
                Name = "local.choco",
            };

            localRepoFeedConfig.Settings.Add("Uri", LocalRepoFolder);

            var tempRepoFeedConfig = new Settings.Feed
            {
                Name = "local.temp.choco",
            };
            tempRepoFeedConfig.Settings.Add("Uri", TempRepoFolder);

            var sourceRepo = new DirectoryRepository(localRepoFeedConfig, LoggerFactory);
            var targetRepo = new DirectoryRepository(tempRepoFeedConfig, LoggerFactory);

            var sourcePackages = sourceRepo.Fetch(t => t.Id == "notepadplusplus" && !t.IsPrerelease).ToArray();

            foreach (var package in sourcePackages)
            {
                var p = sourceRepo.Fetch(package);
                targetRepo.Add(p);
            }

            var targetPackages = targetRepo.Fetch(t => t.Id == "notepadplusplus" && !t.IsPrerelease).ToArray();

            Assert.Equal(NotepadPlusPlusPackageCount, sourcePackages.Length);
            Assert.Equal(NotepadPlusPlusPackageCount, targetPackages.Length);

            foreach (var targetPackage in targetPackages)
            {
                targetRepo.Delete(targetPackage);
            }

            targetPackages = targetRepo.Fetch(t => t.Id == "notepadplusplus" && !t.IsPrerelease).ToArray();
            Assert.Empty(targetPackages);
    }

        public void Dispose()
        {
            if (System.IO.Directory.Exists(TempRepoFolder))
            {
                System.IO.Directory.Delete(TempRepoFolder, true);
            }
        }
    }
}
