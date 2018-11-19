#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="DummyClasses.cs">
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
using Microsoft.Extensions.DependencyInjection;
using SynchroFeed.Library.Action;
using SynchroFeed.Library.Action.Observer;
using SynchroFeed.Library.Command;
using SynchroFeed.Library.Factory;
using SynchroFeed.Library.Model;
using SynchroFeed.Library.Processor;
using SynchroFeed.Library.Repository;
using SynchroFeed.Library.Settings;

namespace SynchroFeed.Library.Test
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class DummyActionFactory : IActionFactory
    {
        public const string ActionType = "DummyAction";
        public string Type { get; set; } = ActionType;
        public IAction Create(Library.Settings.Action actionSettings)
        {
            return new DummyAction();
        }
    }

    public class DummyAction : IAction
    {

        public static Func<Package, PackageEvent, bool> ProcessPackageFunc = ProcessPackageMethod;

        public Library.Settings.Action ActionSettings { get; set; }
        public string ActionType { get; set; } = "DummyAction";
        public ApplicationSettings ApplicationSettings { get; set; }
        public bool Enabled { get; set; }
        public IActionObserverManager ObserverManager { get; set; } = new DummyActionObserverManager();
        public IServiceProvider ServiceProvider { get; set; }
        public IRepository<Package> SourceRepository { get; set; }
        public void Run()
        {
            ProcessPackage(new Package(), PackageEvent.Added);
        }

        public bool ProcessPackage(Package package, PackageEvent packageEvent)
        {
            return ProcessPackageFunc(package, packageEvent);
        }

        public static bool ProcessPackageMethod(Package package, PackageEvent packageEvent)
        {
            return true;
        }
    }
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class DummyActionObserverManager : IActionObserverManager
    {
        public void NotifyObservers(IActionEvent actionEvent)
        {
        }
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class DummyActionProcessor : IActionProcessor
    {
        public void Execute(List<string> actions = null)
        {
            throw new NotImplementedException();
        }

        public IAction CreateAction(IServiceScope scope, Library.Settings.Action actionSettings)
        {
            throw new NotImplementedException();
        }
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class DummyRepositoryFactory : IRepositoryFactory
    {
        public string Type { get; set; }
        public IRepository<Package> Create(Feed feedSettings)
        {
            throw new NotImplementedException();
        }
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class DummyCommandFactory : ICommandFactory
    {
        public string Type { get; set; }
        public ICommand Create(IAction action, Library.Settings.Command commandSettings)
        {
            throw new NotImplementedException();
        }
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class DummyActionObserverFactory : IActionObserverFactory
    {
        public string Type { get; set; }
        public IActionObserver Create(Observer observer)
        {
            throw new NotImplementedException();
        }
    }
}
