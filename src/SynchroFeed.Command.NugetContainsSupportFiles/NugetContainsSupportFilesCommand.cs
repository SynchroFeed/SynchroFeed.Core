using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.Logging;
using SynchroFeed.Library.Action;
using SynchroFeed.Library.Command;
using SynchroFeed.Library.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Settings = SynchroFeed.Library.Settings;

namespace SynchroFeed.Command.NugetContainsSupportFiles
{
    /// <summary>
    /// The NugetContainsSupportFilesCommand class is a <see cref="Command"/> that validates
    /// that the Package contains all the necessary files for a nuget package (i.e. pdb's and xml comment files).
    /// </summary>
    public class NugetContainsSupportFilesCommand : BaseCommand
    {
        private const string Setting_PackageIdRegex = "PackageIdRegex";
        private const string Setting_FileRegex = "FileRegex";
        private const string Setting_CheckForXml = "CheckForXml";
        private const string Setting_CheckForPdb = "CheckForPdb";

        private const string FileRegexPlaceHolder = "~PackageId~";

        /// <summary>
        /// Initializes a new instance of the <see cref="NugetContainsSupportFilesCommand" /> class.
        /// </summary>
        /// <param name="action">The action running with this command.</param>
        /// <param name="commandSettings">The command settings associated with this command.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <exception cref="ArgumentNullException">loggerFactory</exception>
        public NugetContainsSupportFilesCommand(
            IAction action,
            Settings.Command commandSettings,
            ILoggerFactory loggerFactory)
            : base(action, commandSettings, loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            Logger = loggerFactory.CreateLogger<NugetContainsSupportFilesCommand>();
        }

        public override string Type => "NugetContainsSupportFiles";

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private ILogger Logger { get; }

        /// <summary>
        /// Validates that the specified package has additional support files.
        /// </summary>
        /// <param name="package">The package to examine.</param>
        /// <param name="packageEvent">The event associated with the package.</param>
        /// <returns>Returns <c>true</c> if the packages contains the necessary support files, otherwise <c>false</c>.</returns>
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
                    Logger.LogTrace($"{package.Id} (v{package.Version}) skipped due to package filtering.");
                    return new CommandResult(this, true, $"{package} package check skipped.");
                }
            }
            else
            {
                Logger.LogTrace($"Command does not have a '{Setting_PackageIdRegex}' setting, defaulting to all packages.");
            }

            var assembliesMissingSupportFiles = GetAssembliesMissingSupportFiles(package);

            if (assembliesMissingSupportFiles.Count < 1)
            {
                var message = "All support files detected.";

                Logger.LogTrace(message);

                return new CommandResult(this, true, message);
            }
            else
            {
                var sb = new StringBuilder();

                sb.AppendLine($"Missing support files detected:");
                sb.AppendLine();

                foreach (var issue in assembliesMissingSupportFiles)
                {
                    sb.Append(" * ");
                    sb.AppendLine(issue);
                }

                var message = sb.ToString();

                Logger.LogWarning(message);

                return new CommandResult(this, false, message);
            }
        }

        private List<string> GetAssembliesMissingSupportFiles(Package package)
        {
            var assembliesMissingSupportFiles = new HashSet<string>();

            if (this.Settings.Settings.TryGetValue(Setting_FileRegex, out var fileRegex) && !string.IsNullOrWhiteSpace(fileRegex))
                fileRegex = fileRegex.Replace(FileRegexPlaceHolder, package.Id);
            else
                fileRegex = @".*";

            var checkForXml = true;
            var checkForPdb = true;

            if (this.Settings.Settings.TryGetValue(Setting_CheckForXml, out var checkForXmlValue))
                bool.TryParse(checkForXmlValue, out checkForXml);
            if (this.Settings.Settings.TryGetValue(Setting_CheckForPdb, out var checkForPdbValue))
                bool.TryParse(checkForPdbValue, out checkForPdb);

            if (!checkForXml && !checkForPdb)
            {
                Logger.LogWarning("Command disabled, bypassing checks.");
            }
            else
            {
                using (var byteStream = new MemoryStream(package.Content))
                using (var zipFile = new ZipFile(byteStream))
                {
                    foreach (ZipEntry zipEntry in zipFile)
                    {
                        if (!zipEntry.IsFile || (zipEntry.Name == null) || !zipEntry.Name.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
                            continue;

                        var fileName = Path.GetFileNameWithoutExtension(zipEntry.Name);

                        if (!Regex.IsMatch(fileName, fileRegex, RegexOptions.IgnoreCase))
                            continue;

                        var hasMissingSupportFiles = false;

                        if (checkForXml)
                        {
                            var xmlFileName = Path.ChangeExtension(zipEntry.Name, "xml");

                            hasMissingSupportFiles |= (zipFile.GetEntry(xmlFileName) == null);
                        }

                        if (checkForPdb)
                        {
                            var pdbFileName = Path.ChangeExtension(zipEntry.Name, "pdb");

                            hasMissingSupportFiles |= (zipFile.GetEntry(pdbFileName) == null);
                        }

                        if (hasMissingSupportFiles)
                        {
                            assembliesMissingSupportFiles.Add(fileName);
                        }
                    }
                }
            }

            return assembliesMissingSupportFiles.OrderBy(x => x).ToList();
        }
    }
}