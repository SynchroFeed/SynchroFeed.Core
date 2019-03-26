using SynchroFeed.Library.Action;
using SynchroFeed.Library.Factory;

namespace SynchroFeed.Library.TestFramework
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
}