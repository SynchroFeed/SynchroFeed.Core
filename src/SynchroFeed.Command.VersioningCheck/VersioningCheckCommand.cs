using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.Logging;
using SynchroFeed.Library.Action;
using SynchroFeed.Library.Command;
using SynchroFeed.Library.Model;
using SynchroFeed.Library.Reflection;
using SynchroFeed.Library.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

            if (binariesWithDifferentVersions.Count < 1)
            {
                var message = "No version issues detected.";

                Logger.LogTrace(message);

                return new CommandResult(this, true, message);
            }
            else
            {
                var sb = new StringBuilder();

                sb.AppendLine($"Binaries with versions different from the package detected:");
                sb.AppendLine();

                foreach (var issue in binariesWithDifferentVersions)
                {
                    sb.Append(" * ");
                    sb.AppendLine(issue);
                }

                var message = sb.ToString();

                Logger.LogWarning(message);

                return new CommandResult(this, false, message);
            }
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

            var coreAssembly = typeof(object).Assembly;

            using (var byteStream = new MemoryStream(package.Content))
            using (var zipFile = new ZipFile(byteStream))
            using (var lc = new MetadataLoadContext(new ZipAssemblyResolver(zipFile, coreAssembly), coreAssembly.FullName))
            {
                foreach (ZipEntry zipEntry in zipFile)
                {
                    if (!zipEntry.IsFile || (zipEntry.Name == null))
                        continue;

                    var fileName = Path.GetFileName(zipEntry.Name);

                    if (!Regex.IsMatch(fileName, fileRegex, RegexOptions.IgnoreCase))
                        continue;

                    var binaryVersion = GetBinaryVersion(lc, zipFile, zipEntry);

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

        private Version GetBinaryVersion(MetadataLoadContext metaDataLoadContext, ZipFile zipFile, ZipEntry zipEntry)
        {
            using (var memoryStream = ZipUtility.ReadFromZip(zipFile, zipEntry))
            {
                var assembly = metaDataLoadContext.LoadFromStream(memoryStream);

                return assembly.GetName().Version;
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