using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.Logging;
using SynchroFeed.Library.Action;
using SynchroFeed.Library.Command;
using SynchroFeed.Library.DomainLoader;
using SynchroFeed.Library.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Settings = SynchroFeed.Library.Settings;

namespace SynchroFeed.Command.VersioningCheck
{
    /// <summary>
    /// The VersioningCheckCommand class is a <see cref="Command"/> that validates that all binaries within the
    /// package have the same version as the package.
    /// </summary>
    public class VersioningCheckCommand : BaseCommand
    {
        private const string Setting_PackageIdRegex = "PackageIdRegex";
        private const string Setting_FileRegex = "FileRegex";
        private const string FileRegexPlaceHolder = "~PackageId~";

        /// <summary>
        /// Initializes a new instance of the <see cref="VersioningCheckCommand" /> class.
        /// </summary>
        /// <param name="action">The action running with this command.</param>
        /// <param name="commandSettings">The command settings associated with this command.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <exception cref="ArgumentNullException">loggerFactory</exception>
        public VersioningCheckCommand(IAction action, Settings.Command commandSettings, ILoggerFactory loggerFactory)
            : base(action, commandSettings, loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            Logger = loggerFactory.CreateLogger<VersioningCheckCommand>();
        }

        public override string Type => "VersioningCheck";

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private ILogger Logger { get; }

        /// <summary>
        /// Validates that the specified package has consistent versioning with the package.
        /// </summary>
        /// <param name="package">The package to examine.</param>
        /// <param name="packageEvent">The event associated with the package.</param>
        /// <returns>Returns <c>true</c> if the packages is properly versioned, otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">package</exception>
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

            if (this.Settings.Settings.TryGetValue(Setting_PackageIdRegex, out var packageIdRegex) && !string.IsNullOrWhiteSpace(packageIdRegex))
            {
                if (!Regex.IsMatch(package.Id, packageIdRegex, RegexOptions.IgnoreCase))
                {
                    Logger.LogTrace($"{package} skipped due to package filtering.");
                    return new CommandResult(this, true, $"{package} package versioning check skipped.");
                }
            }
            else
            {
                Logger.LogTrace($"Command does not have a '{Setting_PackageIdRegex}' setting, defaulting to all packages.");
            }

            var binariesWithDifferentVersions = GetBinariesWithDifferentVersions(package);

            if (binariesWithDifferentVersions.Count > 0)
            {
                Logger.LogWarning($"{package} has binaries with versions that differ from the package version.");
                return new CommandResult(this, false, $"{package} contains the following binaries with a different version from the package: {string.Join(", ", binariesWithDifferentVersions)}");
            }

            Logger.LogTrace($"{package} contains no differences between package and binary versions.");
            return new CommandResult(this, true, $"{package} contains no differences between package and binary versions.");
        }

        private List<string> GetBinariesWithDifferentVersions(Package package)
        {
            var packageVersion = GetPackageVersion(package.Version);
            var binariesWithDifferentVersions = new HashSet<string>();

            if (this.Settings.Settings.TryGetValue(Setting_FileRegex, out var fileRegex) && !string.IsNullOrWhiteSpace(fileRegex))
                fileRegex = fileRegex.Replace(FileRegexPlaceHolder, package.Id);
            else
                fileRegex = @"\.(dll|exe)$";

            if (package.Content == null)
            {
                Logger.LogWarning($"Contents are empty for package {package.Id}");
                return new List<string>();
            }

            using (var byteStream = new MemoryStream(package.Content))
            using (var zipFile = new ZipFile(byteStream))
            {
                foreach (ZipEntry zipEntry in zipFile)
                {
                    if (!zipEntry.IsFile || (zipEntry.Name == null))
                        continue;

                    var fileName = Path.GetFileName(zipEntry.Name);

                    if (!Regex.IsMatch(fileName, fileRegex, RegexOptions.IgnoreCase))
                        continue;

                    var binaryVersion = GetBinaryVersion(zipFile, zipEntry);

                    if (binaryVersion == null)
                        continue;

                    if (!IsSameVersion(packageVersion, binaryVersion))
                    {
                        binariesWithDifferentVersions.Add(fileName);
                    }
                }
            }

            return binariesWithDifferentVersions.OrderBy(x => x).ToList();
        }

        private static Version GetPackageVersion(string strVersion)
        {
            var versionParts = strVersion.Split('-').FirstOrDefault();

            if (versionParts != null)
            {
                Version.TryParse(versionParts, out var version);
                return version;
            }

            return new Version();
        }

        private Version GetBinaryVersion(ZipFile zipFile, ZipEntry zipEntry)
        {
            using (var entryStream = zipFile.GetInputStream(zipEntry))
            using (var memoryStream = new MemoryStream((int)zipEntry.Size))
            {
                entryStream.CopyTo(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                using (var arm = new AssemblyReflectionManager())
                {
                    var proxy = arm.LoadAssembly(memoryStream.ToArray());
                    if (proxy == null)
                    {
                        Logger.LogDebug($"Ignoring non-.NET assembly - {zipEntry.Name}");
                        return null;
                    }

                    return proxy.AssemblyName.Version;
                }
            }
        }

        private static bool IsSameVersion(Version packageVersion, Version binaryVersion)
        {
            return ((packageVersion != null)
                    && (packageVersion.Major == binaryVersion.Major)
                    && ((packageVersion.Minor < 0) || (packageVersion.Minor == binaryVersion.Minor))
                    && ((packageVersion.Build < 0) || (packageVersion.Build == binaryVersion.Build))
                    && ((packageVersion.Revision < 0) || (packageVersion.Revision == binaryVersion.Revision)));
        }
    }
}