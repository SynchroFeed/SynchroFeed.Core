using System;
using Xunit;

namespace SynchroFeed.Repository.Npm.Test
{
    public sealed class IgnoreUnlessIntegrationTestAttribute : FactAttribute
    {
        public IgnoreUnlessIntegrationTestAttribute()
        {
            if (!IsDebugVersion())
            {
                Skip = "Integration testing only";
            }
        }
        /// <summary>
        /// Determine if runtime is running a debug version
        /// Taken from http://stackoverflow.com/questions/721161
        /// </summary>
        /// <returns>True if being executed in Mono, false otherwise.</returns>
        public static bool IsDebugVersion()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}