using Microsoft.Extensions.DependencyInjection;
using SynchroFeed.Library.Action;
using SynchroFeed.Library.Processor;
using System;
using System.Collections.Generic;

namespace SynchroFeed.Library.TestFramework
{
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
}