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

namespace SynchroFeed.Command.Log4netReview.Test
{
    public class Log4netReviewTest
    {
        private ILoggerFactory LoggerFactory { get; }
        private TestLogger Logger { get; } = new TestLogger();
        private string RootFolder { get; }

        public Log4netReviewTest()
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
            serviceCollection.AddTransient(typeof(ICommandFactory), typeof(Log4netReviewCommandFactory));

            var serviceProvider = serviceCollection
                .BuildServiceProvider();

            var sut = serviceProvider.GetRequiredService<ICommandFactory>();

            Assert.NotNull(sut);
            Assert.IsType<Log4netReviewCommandFactory>(sut);
            Assert.IsAssignableFrom<ICommandFactory>(sut);
        }

        [Theory]
        [InlineData("^Test.*", false)]
        [InlineData("^Package.*", true)]
        public void PackageId_Regex_Test(string packageIdRegex, bool expectedResult)
        {
            var commandResult = CommandTestWrapper.Execute
            (
                (action, command, loggerFactory) => new Log4netReviewCommand(action, command, loggerFactory),
                LoggerFactory,
                Path.Combine(RootFolder, "local.choco", "Test.Package.1.0.0.nupkg"),
                new Library.Settings.Command()
                {
                    Settings =
                    {
                        { "PackageIdRegex", packageIdRegex },
                        { "/configuration/log4net/root", "OFF" },
                    }
                },
                PackageEvent.Added
            );

            Assert.Equal(expectedResult, commandResult.ResultValid);
        }

        [Theory]
        [InlineData("/configuration/log4net/root", "trace|warn|off", PackageEvent.Added, false)]
        [InlineData("/configuration/log4net/root", "TRACE|WARN|OFF", PackageEvent.Added, false)]
        [InlineData("/configuration/log4net/root", "trace|warn|off|info", PackageEvent.Added, true)]
        [InlineData("/configuration/log4net/root", "TRACE|WARN|OFF|INFO", PackageEvent.Added, true)]
        [InlineData("/configuration/log4net/logger[@name='LogInfo']", "TRACE|WARN|OFF|INFO", PackageEvent.Added, true)]
        [InlineData("/configuration/log4net/logger[@name='LogInfo']", "TRACE|WARN|OFF", PackageEvent.Added, false)]
        [InlineData("/configuration/log4net/logger[starts-with(@name, 'Log')]", "INFO|WARN|ERROR", PackageEvent.Added, true)]
        [InlineData("/configuration/log4net/logger[starts-with(@name, 'Log')]", "INFO|WARN", PackageEvent.Added, false)]
        [InlineData("/configuration/log4net/root", "TRACE|WARN|OFF", PackageEvent.Deleted, true)]
        public void General_Test(string key, string value, PackageEvent packageEvent, bool expectedResult)
        {
            var commandResult = CommandTestWrapper.Execute
            (
                (action, command, loggerFactory) => new Log4netReviewCommand(action, command, loggerFactory),
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

        [Theory]
        [InlineData("%date |*| %thread |*| %-5level |*| %logger |*| %message%newline", true)]
        [InlineData("%date |*| %thread |*| %-5level |*| %logger |*| %message%newline%newline", false)]
        public void ConversionPattern_Test(string conversionPattern, bool expectedResult)
        {
            var commandResult = CommandTestWrapper.Execute
            (
                 (action, command, loggerFactory) => new Log4netReviewCommand(action, command, loggerFactory),
                 LoggerFactory,
                 Path.Combine(RootFolder, "local.choco", "Test.Package.1.0.0.nupkg"),
                 new Library.Settings.Command()
                 {
                     Settings =
                     {
                         { "ConversionPattern", conversionPattern },
                     }
                 },
                 PackageEvent.Added
            );

            Assert.Equal(expectedResult, commandResult.ResultValid);
        }
    }
}