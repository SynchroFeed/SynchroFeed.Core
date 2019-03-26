using SynchroFeed.Library.Action;
using SynchroFeed.Library.Action.Observer;
using SynchroFeed.Library.Model;
using SynchroFeed.Library.Repository;
using SynchroFeed.Library.Settings;
using System;

namespace SynchroFeed.Library.TestFramework
{
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
}