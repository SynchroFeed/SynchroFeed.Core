using System;

namespace SynchroFeed.Command.Catalog
{
    [Serializable]
    internal class AssemblyName
    {
        public string FullName { get; set; }
        public string Name { get; set; }
        public Version Version { get; set; }
    }
}