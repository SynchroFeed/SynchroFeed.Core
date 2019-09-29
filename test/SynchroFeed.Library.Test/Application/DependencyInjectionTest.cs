#region header

// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="DependencyInjectionTest.cs">
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
using Moq;
using SynchroFeed.Library.Action.Observer;
using SynchroFeed.Library.DependencyInjection;
using SynchroFeed.Library.DomainLoader;
using SynchroFeed.Library.Settings;
using System.Collections.Generic;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace SynchroFeed.Library.Test.Application
{
    public class DependencyInjectionTest
    {
        private readonly ITestOutputHelper testOutputHelper;

        public DependencyInjectionTest(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Test_AddSynchroFeed_Extension_Adds_Types_To_ServiceCollection()
        {
            var sdCollection = new List<ServiceDescriptor>();
            AssemblyLoader.AssemblyLoaderFunc = pattern => new List<Assembly> { Assembly.GetExecutingAssembly() };
            var mock = new Mock<IServiceCollection>();
            mock.Setup(a => a.Add(It.IsAny<ServiceDescriptor>()))
                .Callback((ServiceDescriptor sd) =>
                {
                    sdCollection.Add(sd);
                    testOutputHelper.WriteLine($"Service Descriptor: {sd.ServiceType.AssemblyQualifiedName}");
                });
            var sut = mock.Object;

            sut.AddSynchroFeed();

            Assert.Contains(sdCollection, sd => sd.ServiceType == typeof(ApplicationSettings));
            Assert.Contains(sdCollection, sd => sd.ServiceType == typeof(ActionObserverManager));
            Assert.Contains(sdCollection, sd => sd.ImplementationType == typeof(DummyActionProcessor));
            Assert.Contains(sdCollection, sd => sd.ImplementationType == typeof(DummyRepositoryFactory));
            Assert.Contains(sdCollection, sd => sd.ImplementationType == typeof(DummyActionFactory));
            Assert.Contains(sdCollection, sd => sd.ImplementationType == typeof(DummyCommandFactory));
            Assert.Contains(sdCollection, sd => sd.ImplementationType == typeof(DummyActionObserverFactory));
        }
    }
}