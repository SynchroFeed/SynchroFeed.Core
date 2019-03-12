#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="ApplicationIs64bitTest.cs">
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SynchroFeed.Library.Command;
using SynchroFeed.Library.Factory;
using SynchroFeed.Library.Model;
using SynchroFeed.Library.Settings;
using SynchroFeed.Repository.Directory;
using Xunit;

namespace SynchroFeed.Command.ApplicationIs64bit.Tests
{
    public class ApplicationIs64bitTest
    {
        private IServiceProvider ServiceProvider { get; }
        private ILoggerFactory LoggerFactory { get; }
        private TestLogger Logger { get; } = new TestLogger();
        private string RootFolder { get; }
        private string LocalRepoFolder { get; }

        public ApplicationIs64bitTest()
        {
            RootFolder = Environment.CurrentDirectory;
            LocalRepoFolder = Path.Combine(RootFolder, "local.choco");

            var serviceCollection = new ServiceCollection();

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(factory => factory.CreateLogger(It.IsAny<string>()))
                .Returns(Logger);
            LoggerFactory = mockLoggerFactory.Object;
            serviceCollection.AddSingleton(LoggerFactory);
            serviceCollection.AddSingleton(new ApplicationSettings());
            serviceCollection.AddTransient(typeof(ICommandFactory), typeof(ApplicationIs64BitCommandFactory));
            ServiceProvider = serviceCollection
                .BuildServiceProvider();
        }

        [Fact]
        public void TestApplicationIs64BitCommandFactory()
        {
            var sut = ServiceProvider.GetRequiredService<ICommandFactory>();

            Assert.NotNull(sut);
            Assert.IsType<ApplicationIs64BitCommandFactory>(sut);
            Assert.IsAssignableFrom<ICommandFactory>(sut);
        }

        [Fact]
        public void TestApplicationIs64BitCommand()
        {
            var command = new Library.Settings.Command();
            var action = new DummyAction();
            var localRepoFeedConfig = new Feed
            {
                Name = "local.choco",
            };
            localRepoFeedConfig.Settings.Add("Uri", LocalRepoFolder);
            action.SourceRepository = new DirectoryRepository(localRepoFeedConfig, LoggerFactory);
            var packages = action.SourceRepository.Fetch((p) => true);

            var sut = new ApplicationIs64BitCommand(action, command, LoggerFactory);
            var results = new Dictionary<Package, CommandResult>();
            foreach (var package in packages)
            {
                results.Add(package, sut.Execute(package, PackageEvent.Added));
            }

            Assert.True(results.Count == 4);
            Assert.True(results[new Package {Id = "Test.Choco.32bit-NoPrefer32-bit", Version = "1.0.0"}].ResultValid);
            Assert.False(results[new Package { Id = "Test.Choco.32bit-Prefer32-bit", Version = "1.0.0" }].ResultValid);
            Assert.True(results[new Package { Id = "Test.Choco.32bit-x64", Version = "1.0.0" }].ResultValid);
            Assert.False(results[new Package { Id = "Test.Choco.32bit-x86", Version = "1.0.0" }].ResultValid);
        }

        [Fact]
        public void TestApplicationIs64BitCommandPackageDeleted()
        {
            var command = new Library.Settings.Command();
            var action = new DummyAction();
            var localRepoFeedConfig = new Feed
            {
                Name = "local.choco",
            };
            localRepoFeedConfig.Settings.Add("Uri", LocalRepoFolder);
            action.SourceRepository = new DirectoryRepository(localRepoFeedConfig, LoggerFactory);
            var packages = action.SourceRepository.Fetch((p) => true);

            var sut = new ApplicationIs64BitCommand(action, command, LoggerFactory);
            var results = new Dictionary<Package, CommandResult>();
            foreach (var package in packages)
            {
                results.Add(package, sut.Execute(package, PackageEvent.Deleted));
            }

            Assert.True(results.Count == 4);
            Assert.True(results[new Package { Id = "Test.Choco.32bit-NoPrefer32-bit", Version = "1.0.0" }].ResultValid);
            Assert.True(results[new Package { Id = "Test.Choco.32bit-Prefer32-bit", Version = "1.0.0" }].ResultValid);
            Assert.True(results[new Package { Id = "Test.Choco.32bit-x64", Version = "1.0.0" }].ResultValid);
            Assert.True(results[new Package { Id = "Test.Choco.32bit-x86", Version = "1.0.0" }].ResultValid);
        }
    }
}
