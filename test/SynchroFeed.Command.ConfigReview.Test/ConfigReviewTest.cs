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

namespace SynchroFeed.Command.ConfigReview.Test
{
    public class ConfigReviewTest
    {
        private ILoggerFactory LoggerFactory { get; }
        private TestLogger Logger { get; } = new TestLogger();
        private string RootFolder { get; }

        public ConfigReviewTest()
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
            serviceCollection.AddTransient(typeof(ICommandFactory), typeof(ConfigReviewCommandFactory));

            var serviceProvider = serviceCollection
                .BuildServiceProvider();

            var sut = serviceProvider.GetRequiredService<ICommandFactory>();

            Assert.NotNull(sut);
            Assert.IsType<ConfigReviewCommandFactory>(sut);
            Assert.IsAssignableFrom<ICommandFactory>(sut);
        }

        [Theory]
        [InlineData("^Test.*", false)]
        [InlineData("^Package.*", true)]
        public void PackageId_Regex_Test(string packageIdRegex, bool expectedResult)
        {
            var commandResult = CommandTestWrapper.Execute
            (
                (action, command, loggerFactory) => new ConfigReviewCommand(action, command, loggerFactory),
                LoggerFactory,
                Path.Combine(RootFolder, "local.choco", "Test.Package.1.0.0.nupkg"),
                new Library.Settings.Command()
                {
                    Settings =
                    {
                        { "PackageIdRegex", packageIdRegex },
                        { "Enabled", "true" },
                    }
                },
                PackageEvent.Added
            );

            Assert.Equal(expectedResult, commandResult.ResultValid);
        }

        [Theory]
        [InlineData("NoExe.Package.1.0.0.nupkg")]
        [InlineData("NoConfig.Package.1.0.0.nupkg")]
        public void No_Matching_Pair_Test(string packageFileName)
        {
            var commandResult = CommandTestWrapper.Execute
            (
                (action, command, loggerFactory) => new ConfigReviewCommand(action, command, loggerFactory),
                LoggerFactory,
                Path.Combine(RootFolder, "local.choco", packageFileName),
                new Library.Settings.Command(),
                PackageEvent.Added
            );

            Assert.True(commandResult.ResultValid);
        }

        [Fact]
        public void Invalid_Config_Test()
        {
            var commandResult = CommandTestWrapper.Execute
            (
                (action, command, loggerFactory) => new ConfigReviewCommand(action, command, loggerFactory),
                LoggerFactory,
                Path.Combine(RootFolder, "local.choco", "InvalidConfig.Package.1.0.0.nupkg"),
                new Library.Settings.Command(),
                PackageEvent.Added
            );

            Assert.False(commandResult.ResultValid);
        }

        [Theory]
        [InlineData("Enabled", "true", PackageEvent.Added, false)]
        [InlineData("Enabled", "false", PackageEvent.Added, true)]
        [InlineData("Disabled", "^F.*E$", PackageEvent.Added, false)]
        [InlineData("+Enabled", "false", PackageEvent.Added, true)]
        [InlineData("+NotPresent", ".*", PackageEvent.Added, false)]
        [InlineData("Enabled", "true", PackageEvent.Deleted, true)]
        public void General_Test(string key, string value, PackageEvent packageEvent, bool expectedResult)
        {
            var commandResult = CommandTestWrapper.Execute
            (
                (action, command, loggerFactory) => new ConfigReviewCommand(action, command, loggerFactory),
                LoggerFactory,
                Path.Combine(RootFolder, "local.choco", "Test.Package.1.0.0.nupkg"),
                new Library.Settings.Command()
                {
                    Settings =
                    {
                        { key, value },
                    }
                },
                packageEvent
            );

            Assert.Equal(expectedResult, commandResult.ResultValid);
        }
    }
}