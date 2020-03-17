using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using SynchroFeed.Library.Action;
using SynchroFeed.Library.Command;
using SynchroFeed.Library.Model;
using SynchroFeed.Library.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Settings = SynchroFeed.Library.Settings;

namespace SynchroFeed.Command.ConfigReview
{
    /// <summary>
    /// The ConfigReviewCommand class is a <see cref="Command"/> that validates
    /// that the Package has valid configuration.
    /// </summary>
    public class ConfigReviewCommand : BaseCommand
    {
        private const string Setting_PackageIdRegex = "PackageIdRegex";

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigReviewCommand" /> class.
        /// </summary>
        /// <param name="action">The action running with this command.</param>
        /// <param name="commandSettings">The command settings associated with this command.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <exception cref="ArgumentNullException">loggerFactory</exception>
        public ConfigReviewCommand(
            IAction action,
            Settings.Command commandSettings,
            ILoggerFactory loggerFactory)
            : base(action, commandSettings, loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            Logger = loggerFactory.CreateLogger<ConfigReviewCommand>();
        }

        public override string Type => "ConfigReview";

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
        /// <returns>Returns <c>true</c> if the packages has valid configuration, otherwise <c>false</c>.</returns>
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

            var issues = new List<string>();

            using (var byteStream = new MemoryStream(package.Content))
            using (var archive = ArchiveFactory.Open(byteStream))
            {
                foreach (var archiveEntry in archive.Entries)
                {
                    if (archiveEntry.IsDirectory || !archiveEntry.Key.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    Logger.LogTrace($"Executable found: {archiveEntry.Key}");

                    var configFileName = archiveEntry.Key + ".config";
                    var configArchiveEntry = archive.Entries.FirstOrDefault(x => x.Key.Equals(configFileName, StringComparison.InvariantCultureIgnoreCase));

                    if (configArchiveEntry == null)
                    {
                        Logger.LogDebug($"Skipping, no config file found for : {configFileName}");
                        continue;
                    }

                    try
                    {
                        using (var configStream = configArchiveEntry.ExtractToStream())
                        {
                            var doc = XDocument.Load(configStream);

                            ValidateConfig(configFileName, doc, issues);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogInformation(e, "Unable to parse: {0}", configFileName);
                    }
                }
            }

            if (issues.Count < 1)
            {
                var message = "Valid configuration.";

                Logger.LogTrace(message);

                return new CommandResult(this, true, message);
            }
            else
            {
                var sb = new StringBuilder();

                sb.AppendLine($"Invalid configuration detected:");
                sb.AppendLine();

                foreach (var issue in issues)
                {
                    sb.Append(" * ");
                    sb.AppendLine(issue);
                }

                var message = sb.ToString();

                Logger.LogWarning(message);

                return new CommandResult(this, false, message);
            }
        }

        private void ValidateConfig(string fileName, XDocument doc, List<string> issues)
        {
            Dictionary<string, string> configuredSettings;

            try
            {
                configuredSettings = doc
                    .XPathSelectElements("/configuration/appSettings/add")
                    .Where(x => (x.Attribute("key") != null) && (x.Attribute("value") != null))
                    .ToDictionary(x => x.Attribute("key")?.Value, x => x.Attribute("value")?.Value);
            }
            catch (ArgumentException)
            {
                issues.Add($"{fileName}: Unable to parse configuration.");
                return;
            }

            foreach (var key in GetSettingsToCheck())
            {
                var mustBePresent = false;
                var settingName = key;

                if (settingName.StartsWith("+"))
                {
                    mustBePresent = true;
                    settingName = settingName.Substring(1);
                }

                if (this.Settings.Settings.TryGetValue(key, out var regexToCheck) && !string.IsNullOrWhiteSpace(regexToCheck))
                {
                    if (configuredSettings.TryGetValue(settingName, out var settingValue))
                    {
                        if (Regex.IsMatch(settingValue, regexToCheck, RegexOptions.IgnoreCase))
                        {
                            issues.Add($"{fileName}: '{settingName}' of '{settingValue} is invalid.");
                        }
                    }
                    else if (mustBePresent)
                    {
                        issues.Add($"{fileName}: '{settingName}' was not configured.");
                    }
                }
            }
        }

        private List<string> GetSettingsToCheck()
        {
            return this.Settings.Settings.Keys
                .Where(key => !string.IsNullOrWhiteSpace(key)
                    && !string.Equals(key, Setting_PackageIdRegex, StringComparison.InvariantCultureIgnoreCase))
                .ToList();
        }
    }
}