#region header

// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="ApplicationIs64BitCommand.cs">
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

using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.Logging;
using SynchroFeed.Library.Action;
using SynchroFeed.Library.Command;
using SynchroFeed.Library.Model;
using SynchroFeed.Library.Reflection;
using SynchroFeed.Library.Zip;
using System;
using System.IO;
using System.Reflection;
using Settings = SynchroFeed.Library.Settings;

namespace SynchroFeed.Command.ApplicationIs64bit
{
    /// <summary>
    /// The ApplicationIs64BitCommand class is a <see cref="Command"/> that validates
    /// that the Package doesn't contain a 32-bit application. This is useful since .NET configuration is
    /// separated between 32 and 64-bit. Having everything run as 64 bit means
    /// we only need to worry about the 64 bit .NET machine.config configuration.
    /// </summary>
    public class ApplicationIs64BitCommand : BaseCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationIs64BitCommand" /> class.
        /// </summary>
        /// <param name="action">The action running with this command.</param>
        /// <param name="commandSettings">The command settings associated with this command.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <exception cref="ArgumentNullException">loggerFactory</exception>
        public ApplicationIs64BitCommand(
            IAction action,
            Settings.Command commandSettings,
            ILoggerFactory loggerFactory)
            : base(action, commandSettings, loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            Logger = loggerFactory.CreateLogger<ApplicationIs64BitCommand>();
        }

        /// <summary>
        /// Gets the action type of this action.
        /// </summary>
        /// <value>The action type of this action.</value>
        public override string Type => "ApplicationIs64Bit";

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private ILogger Logger { get; }

        /// <summary>Validates that the specified package is a 64 bit application.</summary>
        /// <param name="package">The package to examine.</param>
        /// <param name="packageEvent">The event associated with the package.</param>
        /// <returns>Returns <c>true</c> if the packages does not contain a 32-bit application, otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">package</exception>
        /// <remarks>The more correct description is to validate that the package
        /// does not contain a 32-bit application.</remarks>
        public override CommandResult Execute(Package package, PackageEvent packageEvent)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));

            if (packageEvent != PackageEvent.Added)
            {
                Logger.LogTrace($"Ignoring {package.Id} due to the event being a delete");
                return new CommandResult(this);
            }

            if (package.Content == null)
            {
                Logger.LogWarning($"Contents are empty for package {package}.");
                return new CommandResult(this);
            }

            var result = DoesPackageContain32bitExecutable(package);
            if (result.contains32BitExecutable)
            {
                Logger.LogWarning($"{package} contains a 32-bit application:{result.assemblyName}");
                return new CommandResult(this, false, $"{package.Id} contains a 32-bit application:{result.assemblyName}");
            }

            Logger.LogTrace($"{package.Id} does not contain a 32-bit application:{result.assemblyName}");
            return new CommandResult(this, true, $"{package.Id} does not contain a 32-bit application:{result.assemblyName}");
        }

        /// <summary>
        /// Determines if the package contains a 32-bit executable.
        /// </summary>
        /// <param name="repository">The repository containing the package.</param>
        /// <param name="package">The package to examine if it contains a 32-bit executable.</param>
        /// <returns><c>true</c> if the package contains a 32-bit executable, <c>false</c> otherwise.</returns>
        private (bool contains32BitExecutable, string assemblyName) DoesPackageContain32bitExecutable(Package package)
        {
            var coreAssembly = typeof(object).Assembly;

            using (var byteStream = new MemoryStream(package.Content))
            using (var zipFile = new ZipFile(byteStream))
            using (var lc = new MetadataLoadContext(new ZipAssemblyResolver(zipFile, coreAssembly), coreAssembly.FullName))
            {
                foreach (ZipEntry zipEntry in zipFile)
                {
                    if (!zipEntry.IsFile)
                        continue;

                    if (zipEntry.Name.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
                    {
                        using (var memoryStream = ZipUtility.ReadFromZip(zipFile, zipEntry))
                        {
                            var assembly = lc.LoadFromStream(memoryStream);

                            foreach (var module in assembly.GetModules())
                            {
                                module.GetPEKind(out var peKind, out _);

                                if (peKind.HasFlag(PortableExecutableKinds.Preferred32Bit) || peKind.HasFlag(PortableExecutableKinds.Required32Bit))
                                {
                                    return (true, zipEntry.Name);
                                }
                            }
                        }
                    }
                }
            }

            return (false, string.Empty);
        }
    }
}