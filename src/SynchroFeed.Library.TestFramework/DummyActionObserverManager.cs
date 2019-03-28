using SynchroFeed.Library.Action.Observer;

namespace SynchroFeed.Library.TestFramework
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class DummyActionObserverManager : IActionObserverManager
    {
        public void NotifyObservers(IActionEvent actionEvent)
        {
        }
    }
}