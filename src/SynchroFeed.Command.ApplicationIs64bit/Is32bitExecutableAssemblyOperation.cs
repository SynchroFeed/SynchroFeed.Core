#region header

// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="Is32bitExecutableAssemblyValidator.cs">
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

#endregion header

using SynchroFeed.Library.DomainLoader;
using System.Reflection;

namespace SynchroFeed.Command.ApplicationIs64bit
{
    /// <summary>
    /// The Is32bitExecutableAssemblyOperation class takes an assembly and checks if it is a 32-bit assembly.
    /// </summary>
    /// <seealso cref="IAssemblyOperation" />
    public class Is32bitExecutableAssemblyOperation : IAssemblyOperation
    {
        /// <summary>
        /// Operates on the assembly and, optionally, returning an object.
        /// </summary>
        /// <param name="assembly">The assembly to operate on.</param>
        /// <returns>The resulting serializable object returned by the operation.</returns>
        public object Operation(Assembly assembly)
        {
            foreach (var module in assembly.GetModules())
            {
                module.GetPEKind(out var peKind, out _);
                if (peKind.HasFlag(PortableExecutableKinds.Preferred32Bit) || peKind.HasFlag(PortableExecutableKinds.Required32Bit))
                    return true;
            }

            return false;
        }
    }
}