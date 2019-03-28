using SynchroFeed.Library.Action;
using SynchroFeed.Library.Command;
using SynchroFeed.Library.Factory;
using System;

namespace SynchroFeed.Library.TestFramework
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class DummyCommandFactory : ICommandFactory
    {
        public string Type { get; set; }

        public ICommand Create(IAction action, Library.Settings.Command commandSettings)
        {
            throw new NotImplementedException();
        }
    }
}