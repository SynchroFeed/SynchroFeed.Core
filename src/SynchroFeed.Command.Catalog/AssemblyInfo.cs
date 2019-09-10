using System;
using System.Collections.Generic;

namespace SynchroFeed.Command.Catalog
{
    [Serializable]
    internal class AssemblyInfo
    {
        public AssemblyInfo()
        {
            ReferencedAssemblies = new List<AssemblyName>();
        }

        public AssemblyName AssemblyName { get; set; }
        public List<AssemblyName> ReferencedAssemblies { get; }
    }
}