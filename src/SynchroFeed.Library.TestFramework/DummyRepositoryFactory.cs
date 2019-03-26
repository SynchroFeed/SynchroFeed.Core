using SynchroFeed.Library.Factory;
using SynchroFeed.Library.Model;
using SynchroFeed.Library.Repository;
using SynchroFeed.Library.Settings;
using System;

namespace SynchroFeed.Library.TestFramework
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class DummyRepositoryFactory : IRepositoryFactory
    {
        public string Type { get; set; }

        public IRepository<Package> Create(Feed feedSettings)
        {
            throw new NotImplementedException();
        }
    }
}