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

#endregion header

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SynchroFeed.Library.Factory;
using SynchroFeed.Library.Model;
using SynchroFeed.Library.Settings;
using SynchroFeed.Library.TestFramework;
using System;
using System.IO;
using Xunit;

namespace SynchroFeed.Command.ApplicationIs64bit.Tests
{
    public class ApplicationIs64bitTest
    {
        private ILoggerFactory LoggerFactory { get; }
        private TestLogger Logger { get; } = new TestLogger();
        private string RootFolder { get; }

        public ApplicationIs64bitTest()
        {
            RootFolder = Environment.CurrentDirectory;

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory
                .Setup(factory => factory.CreateLogger(It.IsAny<string>()))
                .Returns(Logger);

            LoggerFactory = mockLoggerFactory.Object;
        }

        [Fact]
        public void TestApplicationIs64BitCommandFactory()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(LoggerFactory);
            serviceCollection.AddSingleton(new ApplicationSettings());
            serviceCollection.AddTransient(typeof(ICommandFactory), typeof(ApplicationIs64BitCommandFactory));

            var serviceProvider = serviceCollection
                .BuildServiceProvider();

            var sut = serviceProvider.GetRequiredService<ICommandFactory>();

            Assert.NotNull(sut);
            Assert.IsType<ApplicationIs64BitCommandFactory>(sut);
            Assert.IsAssignableFrom<ICommandFactory>(sut);
        }

        [Theory]
        [InlineData("Test.Choco.32bit-NoPrefer32-bit.1.0.0.nupkg", PackageEvent.Added, true)]
        [InlineData("Test.Choco.32bit-Prefer32-bit.1.0.0.nupkg", PackageEvent.Added, false)]
        [InlineData("Test.Choco.32bit-x64.1.0.0.nupkg", PackageEvent.Added, true)]
        [InlineData("Test.Choco.32bit-x86.1.0.0.nupkg", PackageEvent.Added, false)]
        [InlineData("Test.Choco.32bit-NoPrefer32-bit.1.0.0.nupkg", PackageEvent.Deleted, true)]
        [InlineData("Test.Choco.32bit-Prefer32-bit.1.0.0.nupkg", PackageEvent.Deleted, true)]
        [InlineData("Test.Choco.32bit-x64.1.0.0.nupkg", PackageEvent.Deleted, true)]
        [InlineData("Test.Choco.32bit-x86.1.0.0.nupkg", PackageEvent.Deleted, true)]
        public void TestApplicationIs64BitCommand(string packageFileName, PackageEvent packageEvent, bool expectedResult)
        {
            var commandResult = CommandTestWrapper.Execute
            (
                (action, command, loggerFactory) => new ApplicationIs64BitCommand(action, command, loggerFactory),
                LoggerFactory,
                Path.Combine(RootFolder, "local.choco", packageFileName),
                new Library.Settings.Command(),
                packageEvent
            );

            Assert.Equal(expectedResult, commandResult.ResultValid);
        }
    }
}