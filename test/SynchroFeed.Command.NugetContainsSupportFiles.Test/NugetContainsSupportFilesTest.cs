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

namespace SynchroFeed.Command.NugetContainsSupportFiles.Test
{
    public class NugetContainsSupportFilesTest
    {
        private ILoggerFactory LoggerFactory { get; }
        private TestLogger Logger { get; } = new TestLogger();
        private string RootFolder { get; }

        public NugetContainsSupportFilesTest()
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
            serviceCollection.AddTransient(typeof(ICommandFactory), typeof(NugetContainsSupportFilesCommandFactory));

            var serviceProvider = serviceCollection
                .BuildServiceProvider();

            var sut = serviceProvider.GetRequiredService<ICommandFactory>();

            Assert.NotNull(sut);
            Assert.IsType<NugetContainsSupportFilesCommandFactory>(sut);
            Assert.IsAssignableFrom<ICommandFactory>(sut);
        }

        [Theory]
        [InlineData("^Has.Support.*", true)]
        [InlineData("^Has.Some.*", false)]
        public void PackageId_Regex_Test(string packageIdRegex, bool expectedResult)
        {
            var commandResult = CommandTestWrapper.Execute
            (
                (action, command, loggerFactory) => new NugetContainsSupportFilesCommand(action, command, loggerFactory),
                LoggerFactory,
                Path.Combine(RootFolder, "local.choco", "Has.Some.Support.Files.1.0.0.nupkg"),
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
        [InlineData("Has.Some.Support.Files.1.0.0.nupkg", null, false, false, true)] // Match only the 'valid' items.
        [InlineData("Has.Some.Support.Files.1.0.0.nupkg", "(Book|Shelf)", true, false, true)] // Look at only the items with Xml files.
        [InlineData("Has.Some.Support.Files.1.0.0.nupkg", "(Book|Library)", false, true, true)] // Look at only the items with Pdb files.
        [InlineData("Files.With.Package.Name.4.1.0.nupkg", @"^~PackageId~.*", true, true, true)] // Use the packageId replacement token.
        public void File_Regex_Test(string packageFileName, string fileRegex, bool checkForXml, bool checkForPdb, bool expectedResult)
        {
            var commandResult = CommandTestWrapper.Execute
            (
                 (action, command, loggerFactory) => new NugetContainsSupportFilesCommand(action, command, loggerFactory),
                 LoggerFactory,
                 Path.Combine(RootFolder, "local.choco", packageFileName),
                 new Library.Settings.Command()
                 {
                     Settings =
                     {
                         { "FileRegex", fileRegex },
                         { "CheckForXml", checkForXml.ToString() },
                         { "CheckForPdb", checkForPdb.ToString() }
                     }
                 },
                 PackageEvent.Added
            );

            Assert.Equal(expectedResult, commandResult.ResultValid);
        }

        [Theory]
        [InlineData("Has.Some.Support.Files.1.0.0.nupkg", PackageEvent.Added, false)]
        [InlineData("Has.Support.Files.1.0.0.nupkg", PackageEvent.Added, true)]
        [InlineData("Has.Some.Support.Files.1.0.0.nupkg", PackageEvent.Deleted, true)]
        [InlineData("Has.Support.Files.1.0.0.nupkg", PackageEvent.Deleted, true)]
        public void General_Test(string packageFileName, PackageEvent packageEvent, bool expectedResult)
        {
            var commandResult = CommandTestWrapper.Execute
            (
                (action, command, loggerFactory) => new NugetContainsSupportFilesCommand(action, command, loggerFactory),
                LoggerFactory,
                Path.Combine(RootFolder, "local.choco", packageFileName),
                new Library.Settings.Command(),
                packageEvent
            );

            Assert.Equal(expectedResult, commandResult.ResultValid);
        }
    }
}