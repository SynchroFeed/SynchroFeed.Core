using System;
using System.Collections.Generic;
using System.Linq;

namespace SynchroFeed.Library
{
    /// <summary>The IInitializable interface is used to mark a class that needs to be initialized by calling its Initialize() method.</summary>
    public interface IInitializable
    {
        /// <summary>The Initialize method is called to initialize a class before using.</summary>
        void Initialize();
    }
}
