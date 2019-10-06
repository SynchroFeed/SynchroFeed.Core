using Microsoft.Extensions.Logging;
using Moq;
using SynchroFeed.Library.Action;
using SynchroFeed.Library.Command;
using SynchroFeed.Library.Model;
using SynchroFeed.Library.Repository;
using System;

namespace SynchroFeed.Library.TestFramework
{
    public static class CommandTestWrapper
    {
        public static CommandResult Execute<TCommand>(Func<IAction, Settings.Command, ILoggerFactory, TCommand> commandFactory, ILoggerFactory loggerFactory, string packageFileName, Settings.Command commandSettings, PackageEvent packageEvent)
            where TCommand : BaseCommand
        {
            var mockRepository = new Mock<IRepository<Package>>();
            mockRepository
                .Setup(m => m.Name)
                .Returns("MockRepository");
            mockRepository
                .Setup(m => m.Fetch(It.IsAny<Package>()))
                .Returns<Package>(p => p);

            var mockAction = new Mock<IAction>();
            mockAction
                .Setup(a => a.SourceRepository)
                .Returns(mockRepository.Object);

            var commandUnderTest = commandFactory(mockAction.Object, commandSettings, loggerFactory);
            var package = Package.CreateFromFile(packageFileName);

            return commandUnderTest.Execute(package, packageEvent);
        }
    }
}