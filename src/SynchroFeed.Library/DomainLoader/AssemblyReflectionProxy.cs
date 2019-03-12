#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="AssemblyReflectionProxy.cs">
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
using System.Reflection;

namespace SynchroFeed.Library.DomainLoader
{
    /// <summary>
    /// The AssemblyReflectionProxy class is used to proxy assembly information
    /// from one domain to another.
    /// </summary>
    /// <seealso cref="System.MarshalByRefObject" />
    public class AssemblyReflectionProxy : MarshalByRefObject
    {
        /// <summary>
        /// Gets the proxied assembly.
        /// </summary>
        /// <value>The proxied assembly.</value>
        public Assembly Assembly { get; private set; }

        /// <summary>
        /// Gets the name of the proxied assembly.
        /// </summary>
        /// <value>The name of the proxied assembly.</value>
        public AssemblyName AssemblyName { get; private set; }

        /// <summary>Validate the assembly by calling the specifier of type T.</summary>
        /// <typeparam name="T">Returns true if the validator is able to successfully validate the assembly.</typeparam>
        public bool ValidateAssembly<T>()
            where T : IAssemblyValidator
        {
            var validator = (T)Activator.CreateInstance(typeof(T), new object[] { });
            return validator.Validate(Assembly);
        }

        /// <summary>
        /// An internal method that loads the assembly into the proxied domain.
        /// </summary>
        /// <param name="assemblyBytes">The assembly bytes to load.</param>
        internal void LoadAssembly(byte[] assemblyBytes)
        {
            Assembly = Assembly.ReflectionOnlyLoad(assemblyBytes);
            AssemblyName = Assembly.GetName();
        }
    }
}