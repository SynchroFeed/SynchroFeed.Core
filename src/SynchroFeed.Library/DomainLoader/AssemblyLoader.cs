#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="AssemblyLoader.cs">
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
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

namespace SynchroFeed.Library.DomainLoader
{
    /// <summary>
    /// The AssemblyLoader class scans the folders for assemblies.
    /// </summary>
    public static class AssemblyLoader
    {
        internal static Func<string, IEnumerable<Assembly>> AssemblyLoaderFunc = AssemblyLoader.Load;

        /// <summary>
        /// Loads the assemblies that match the specified search pattern.
        /// </summary>
        /// <param name="searchPattern">The search pattern to match files.
        /// The pattern can have multiple patterns separated with a pipe (|).</param>
        /// <returns>IEnumerable&lt;Assembly&gt;.</returns>
        public static IEnumerable<Assembly> Load(string searchPattern)
        {
            return Load(Path.GetDirectoryName(new Uri(GetAssemblyCodeBasePath()).LocalPath), searchPattern);
        }

        /// <summary>
        /// Loads the assemblies that match the specified search pattern from the specified directory.
        /// </summary>
        /// <param name="directory">The directory to load the assemblies from.</param>
        /// <param name="searchPattern">The search pattern to match files.
        /// The pattern can have multiple patterns separated with a pipe (|).</param>
        /// <returns>IEnumerable&lt;Assembly&gt;.</returns>
        public static IEnumerable<Assembly> Load(string directory, string searchPattern)
        {
            var assemblies = new List<Assembly>();
            if (directory == null)
                return assemblies;

            var files = searchPattern.Split('|').SelectMany(sp => Directory.GetFiles(directory, sp, SearchOption.AllDirectories));
            foreach (var file in files)
            {
                var assembly = LoadAssembly(file);
                if (assembly != null)
                    assemblies.Add(assembly);
            }

            return assemblies;
        }

        private static string GetAssemblyCodeBasePath()
        {
            return typeof(AssemblyLoader).Assembly.Location;
        }

        private static Assembly LoadAssembly(string filename)
        {
            try
            {
                return Assembly.LoadFrom(filename);
            }
            catch (BadImageFormatException)
            {
                // Ignore since it is a a non .NET assembly
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }
    }
}