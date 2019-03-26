using SynchroFeed.Library.Action.Observer;
using SynchroFeed.Library.Factory;
using SynchroFeed.Library.Settings;
using System;

namespace SynchroFeed.Library.TestFramework
{
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