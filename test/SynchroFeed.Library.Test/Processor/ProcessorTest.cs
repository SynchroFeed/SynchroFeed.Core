#region header

// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="ProcessorTest.cs">
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

using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SynchroFeed.Library.Factory;
using SynchroFeed.Library.Processor;
using SynchroFeed.Library.Settings;
using SynchroFeed.Library.TestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SynchroFeed.Library.Test.Processor
{
    public class ProcessorTest
    {
        private Fixture Fixture { get; } = new Fixture();
        private IServiceProvider ServiceProvider { get; }
        private ILoggerFactory LoggerFactory { get; }
        private TestLogger Logger { get; } = new TestLogger();

        public ProcessorTest()
        {
            var serviceCollection = new ServiceCollection();

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(factory => factory.CreateLogger(It.IsAny<string>()))
                .Returns(Logger);
            LoggerFactory = mockLoggerFactory.Object;
            serviceCollection.AddSingleton(LoggerFactory);
            serviceCollection.AddTransient<IActionFactory>(sp => new DummyActionFactory());

            ServiceProvider = serviceCollection
                .BuildServiceProvider();
        }

        [Fact]
        public void Test_ActionProcessor_No_Actions()
        {
            var appSettings = new ApplicationSettings();
            var sut = new ActionProcessor(ServiceProvider, LoggerFactory, appSettings);

            sut.Execute();

            Assert.True(Logger.LoggedMessages.Count == 1);
            Assert.Equal(LogLevel.Error, Logger.LoggedMessages[0].LogLevel);
        }

        [Fact]
        public void Test_ActionProcessor_Specific_Action_Not_Found()
        {
            var appSettings = new ApplicationSettings();
            Fixture.AddManyTo(appSettings.Actions, 25);

            var sut = new ActionProcessor(ServiceProvider, LoggerFactory, appSettings);

            sut.Execute(new List<string>
            {
                Fixture.Create("Action"),
                Fixture.Create("Action")
            });

            Assert.True(Logger.LoggedMessages.Count == 1);
            Assert.Equal(LogLevel.Error, Logger.LoggedMessages[0].LogLevel);
        }

        [Fact]
        public void Test_ActionProcessor_Specific_Action_Found_Warn_Others_Not_Found()
        {
            var appSettings = new ApplicationSettings();
            Fixture.AddManyTo(appSettings.Actions, 25);

            var sut = new ActionProcessor(ServiceProvider, LoggerFactory, appSettings);

            sut.Execute(new List<string>
            {
                appSettings.Actions.First().Name,
                Fixture.Create("Action")
            });

            Assert.Equal(1, Logger.LoggedMessages.Count(l => l.LogLevel == LogLevel.Warning));
        }

        [Fact]
        public void Test_ActionProcessor_Action_None_Enabled()
        {
            var appSettings = new ApplicationSettings();
            Fixture.AddManyTo(appSettings.Actions, 23);
            appSettings.Actions.ForEach(action => action.Enabled = false);
            var sut = new ActionProcessor(ServiceProvider, LoggerFactory, appSettings);

            sut.Execute();

            Assert.True(Logger.LoggedMessages.Count == 1);
            Assert.Equal(LogLevel.Error, Logger.LoggedMessages[0].LogLevel);
        }

        [Fact]
        public void Test_ActionProcessor_Action_Run_Enabled_No_Service_Found()
        {
            const int actionCount = 8;
            var appSettings = new ApplicationSettings();
            Fixture.AddManyTo(appSettings.Actions, actionCount);
            var sut = new ActionProcessor(ServiceProvider, LoggerFactory, appSettings);

            sut.Execute();

            Assert.Equal(actionCount, Logger.LoggedMessages.Count(l => l.LogLevel == LogLevel.Error));
        }

        [Fact]
        public void Test_ActionProcessor_Action_Run()
        {
            const int actionCount = 8;
            bool processPackageCalled = false;
            var appSettings = new ApplicationSettings();
            Fixture.AddManyTo(appSettings.Actions, actionCount);
            appSettings.Actions.ForEach(action => action.Enabled = false);
            appSettings.Actions[4].Enabled = true;
            appSettings.Actions[4].Type = DummyActionFactory.ActionType;
            DummyAction.ProcessPackageFunc = (package, packageEvent) =>
            {
                processPackageCalled = true;
                return true;
            };
            var sut = new ActionProcessor(ServiceProvider, LoggerFactory, appSettings);

            sut.Execute();

            Assert.True(processPackageCalled, "Action never ran ProcessPackage");
        }
    }
}