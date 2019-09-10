#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="AssemblyReflectionManager.cs">
// MIT License
// 
// Copyright(c) 2018 Robert Vandehey
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
#endregion
using System;
using System.Security.Policy;

namespace SynchroFeed.Library.DomainLoader
{
    /// <summary>
    /// The AssemblyReflectionManager class is used to load .NET assemblies
    /// into another domain for reflection and still have the ability to unload.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class AssemblyReflectionManager : IDisposable
    {
        private readonly AppDomain _appDomain;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyReflectionManager"/> class.
        /// </summary>
        public AssemblyReflectionManager()
        {
            _appDomain = CreateChildDomain(AppDomain.CurrentDomain, "assemblyLoadDomain");
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AssemblyReflectionManager"/> class.
        /// </summary>
        ~AssemblyReflectionManager()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Loads the assembly bytes into the appdomain.
        /// </summary>
        /// <param name="assemblyBytes">The assembly bytes.</param>
        /// <returns>AssemblyReflectionProxy.</returns>
        public AssemblyReflectionProxy LoadAssembly(byte[] assemblyBytes)
        {
            AssemblyReflectionProxy proxy;

            // load the assembly in the specified app domain
            var proxyType = typeof(AssemblyReflectionProxy);
            if (proxyType.FullName == null)
                return null;
            try
            {
                proxy = (AssemblyReflectionProxy)_appDomain.CreateInstanceFromAndUnwrap(proxyType.Assembly.Location, proxyType.FullName);
                proxy.LoadAssembly(assemblyBytes);
            }
            catch (BadImageFormatException)
            {
                // Ignoring non-.NET assembly
                proxy = null;
            }

            return proxy;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                AppDomain.Unload(_appDomain);
            }
        }

        /// <summary>
        /// Creates the child domain.
        /// </summary>
        /// <param name="parentDomain">The parent domain.</param>
        /// <param name="domainName">Name of the domain to create.</param>
        /// <returns>Returns the AppDomain instance</returns>
        private AppDomain CreateChildDomain(AppDomain parentDomain, string domainName)
        {
            var evidence = new Evidence(parentDomain.Evidence);
            var setup = parentDomain.SetupInformation;
            return AppDomain.CreateDomain(domainName, evidence, setup);
        }
    }
}