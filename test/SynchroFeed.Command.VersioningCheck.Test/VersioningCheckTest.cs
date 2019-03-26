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

namespace SynchroFeed.Command.VersioningCheck.Test
{
    public class VersioningCheckTest
    {
        private ILoggerFactory LoggerFactory { get; }
        private TestLogger Logger { get; } = new TestLogger();
        private string RootFolder { get; }

        public VersioningCheckTest()
        {
            RootFolder = Environment.CurrentDirectory;

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory
                .Setup(factory => factory.CreateLogger(It.IsAny<string>()))
                .Returns(Logger);

            LoggerFactory = mockLoggerFactory.Object;
        }

        [Fact]
        public void Factory_Test()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(LoggerFactory);
            serviceCollection.AddSingleton(new ApplicationSettings());
            serviceCollection.AddTransient(typeof(ICommandFactory), typeof(VersioningCheckCommandFactory));

            var serviceProvider = serviceCollection
                .BuildServiceProvider();

            var sut = serviceProvider.GetRequiredService<ICommandFactory>();

            Assert.NotNull(sut);
            Assert.IsType<VersioningCheckCommandFactory>(sut);
            Assert.IsAssignableFrom<ICommandFactory>(sut);
        }

        [Theory]
        [InlineData("^Same.*", true)]
        [InlineData("^Different.*", false)]
        public void PackageId_Regex_Test(string packageIdRegex, bool expectedResult)
        {
            var commandResult = CommandTestWrapper.Execute
            (
                (action, command, loggerFactory) => new VersioningCheckCommand(action, command, loggerFactory),
                LoggerFactory,
                Path.Combine(RootFolder, "local.choco", "Different.Version.From.Package.1.0.0.nupkg"),
                new Library.Settings.Command()
                {
                    Settings =
                    {
                        { "PackageIdRegex", packageIdRegex }
                    }

                },
                PackageEvent.Added
            );

            Assert.Equal(expectedResult, commandResult.ResultValid);
        }

        [Theory]
        [InlineData("Some.Different.Versions.2.0.0.nupkg", @"^Shelf\.(dll|exe)", true)] // Match only the 'valid' items.
        [InlineData("Different.Version.From.Package.1.0.0.nupkg", @".*Computer.*\.(dll|exe)", true)] // Remove all items and nothing breaks.
        [InlineData("Files.With.Package.Name.4.1.0.nupkg", @"^~PackageId~.*\.(dll|exe)", true)] // Use the packageId replacement token.
        public void File_Regex_Test(string fileName, string fileRegex, bool expectedResult)
        {
            var commandResult = CommandTestWrapper.Execute
            (
                 (action, command, loggerFactory) => new VersioningCheckCommand(action, command, loggerFactory),
                 LoggerFactory,
                 Path.Combine(RootFolder, "local.choco", fileName),
                 new Library.Settings.Command()
                 {
                     Settings =
                     {
                         { "FileRegex", fileRegex }
                     }

                 },
                 PackageEvent.Added
            );

            Assert.Equal(expectedResult, commandResult.ResultValid);
        }

        [Theory]
        [InlineData("Different.Version.From.Package.1.0.0.nupkg", PackageEvent.Added, false)]
        [InlineData("Same.Versions.1.0.0.nupkg", PackageEvent.Added, true)]
        [InlineData("Same.Versions.Less.Precision.3.0.0.nupkg", PackageEvent.Added, true)]
        [InlineData("Some.Different.Versions.2.0.0.nupkg", PackageEvent.Added, false)]
        [InlineData("Files.With.Package.Name.4.1.0.nupkg", PackageEvent.Added, false)]
        [InlineData("Different.Version.From.Package.1.0.0.nupkg", PackageEvent.Deleted, true)]
        [InlineData("Same.Versions.1.0.0.nupkg", PackageEvent.Deleted, true)]
        [InlineData("Same.Versions.Less.Precision.3.0.0.nupkg", PackageEvent.Deleted, true)]
        [InlineData("Some.Different.Versions.2.0.0.nupkg", PackageEvent.Deleted, true)]
        [InlineData("Files.With.Package.Name.4.1.0.nupkg", PackageEvent.Deleted, true)]
        public void General_Test(string packageFileName, PackageEvent packageEvent, bool expectedResult)
        {
            var commandResult = CommandTestWrapper.Execute
            (
                (action, command, loggerFactory) => new VersioningCheckCommand(action, command, loggerFactory),
                LoggerFactory,
                Path.Combine(RootFolder, "local.choco", packageFileName),
                new Library.Settings.Command(),
                packageEvent
            );

            Assert.Equal(expectedResult, commandResult.ResultValid);
        }
    }
}