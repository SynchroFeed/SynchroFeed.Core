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

namespace SynchroFeed.Command.Log4netReview
{
    /// <summary>
    /// The Log4netReviewCommand class is a <see cref="Command"/> that validates
    /// that the Package has valid log4net configuration.
    /// </summary>
    public class Log4netReviewCommand : BaseCommand
    {
        private const string Setting_PackageIdRegex = "PackageIdRegex";
        private const string Setting_ConversionPattern = "ConversionPattern";

        private enum LogLevel
        {
            Off = 0,
            Fatal,
            Error,
            Warn,
            Info,
            Debug,
            Trace,
            All,
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4netReviewCommand" /> class.
        /// </summary>
        /// <param name="action">The action running with this command.</param>
        /// <param name="commandSettings">The command settings associated with this command.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <exception cref="ArgumentNullException">loggerFactory</exception>
        public Log4netReviewCommand(
            IAction action,
            Settings.Command commandSettings,
            ILoggerFactory loggerFactory)
            : base(action, commandSettings, loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            Logger = loggerFactory.CreateLogger<Log4netReviewCommand>();
        }

        public override string Type => "Log4netReview";

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
        /// <returns>Returns <c>true</c> if the packages has valid log4net configuration, otherwise <c>false</c>.</returns>
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
                    if (archiveEntry.IsDirectory || !archiveEntry.Key.EndsWith(".config"))
                        continue;

                    try
                    {
                        using (var entryStream = archiveEntry.ExtractToStream())
                        {
                            var doc = XDocument.Load(entryStream);

                            ParseConfig(archiveEntry.Key, doc, issues);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogInformation(e, "Unable to parse: {0}", archiveEntry.Key);
                    }
                }
            }

            if (issues.Count < 1)
            {
                var message = "Valid log4net configuration.";

                Logger.LogTrace(message);

                return new CommandResult(this, true, message);
            }
            else
            {
                var sb = new StringBuilder();

                sb.AppendLine($"Invalid log4net configuration detected:");
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

        private void ParseConfig(string fileName, XDocument doc, List<string> issues)
        {
            // Verify the file is a log4net config file.
            if (doc.XPathSelectElement(@"/configuration/log4net") == null)
                return;

            foreach (var xpath in GetXPathSettings())
            {
                if (this.Settings.Settings.TryGetValue(xpath, out var logLevelValue) && !string.IsNullOrWhiteSpace(logLevelValue))
                {
                    var elements = FindElementsFromXpath(doc, xpath);

                    if (elements.Count > 0)
                    {
                        var availableLogLevels = GetAvailableLogLevels(logLevelValue);

                        foreach (var element in elements)
                        {
                            var currentLogLevel = ParseLogLevel(element?.Attribute("value")?.Value);

                            if ((currentLogLevel != null) && !availableLogLevels.Contains(currentLogLevel.Value))
                            {
                                var elementName = GetLoggerName(element);

                                issues.Add($"{fileName}: '{currentLogLevel}' is not a valid for {elementName}");
                            }
                        }
                    }
                }
            }

            /**************************************/

            if (this.Settings.Settings.TryGetValue(Setting_ConversionPattern, out var expectedConversionPattern) && !string.IsNullOrWhiteSpace(expectedConversionPattern))
            {
                Logger.LogDebug("Checking for conversion pattern: {0}", expectedConversionPattern);

                var conversionPatterns = doc.XPathSelectElements(@"/configuration/log4net/appender/layout/conversionPattern[@value]");

                foreach (var conversionPattern in conversionPatterns)
                {
                    var pattern = conversionPattern.Attribute("value")?.Value.Trim();

                    if (!string.Equals(pattern, expectedConversionPattern, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Logger.LogInformation($"{fileName} failed ConversionPattern Test with: {pattern}", fileName, pattern);
                        issues.Add($"{fileName}: 'conversionPattern' does not match expected pattern");
                    }
                }
            }
            else
            {
                Logger.LogDebug("{0} test skipped, due to configuration not being set.", Setting_ConversionPattern);
            }
        }

        private List<string> GetXPathSettings()
        {
            return this.Settings.Settings.Keys
                .Where(key => !string.IsNullOrWhiteSpace(key)
                    && !string.Equals(key, Setting_ConversionPattern, StringComparison.InvariantCultureIgnoreCase)
                    && !string.Equals(key, Setting_PackageIdRegex, StringComparison.InvariantCultureIgnoreCase))
                .ToList();
        }

        private List<XElement> FindElementsFromXpath(XDocument doc, string xpath)
        {
            var finalXPath = $"{xpath.TrimEnd('/')}/level[@value]";

            return doc
                .XPathSelectElements(finalXPath)
                .ToList();
        }

        private static List<LogLevel> GetAvailableLogLevels(string logLevelValue)
        {
            return logLevelValue
                .Split('|')
                .Select(ParseLogLevel)
                .Where(level => level != null)
                .Select(level => level.Value)
                .ToList();
        }

        private static LogLevel? ParseLogLevel(string value)
        {
            if ((value == null) || !Enum.TryParse<LogLevel>(value, true, out var level))
                return null;

            return level;
        }

        private static string GetLoggerName(XElement element)
        {
            if (element.Name.LocalName.Equals("level", StringComparison.InvariantCultureIgnoreCase))
                element = element.Parent;

            if (element != null && element.Name.LocalName.Equals("logger", StringComparison.InvariantCultureIgnoreCase))
            {
                var name = element.Attribute("name")?.Value;

                if (!string.IsNullOrWhiteSpace(name))
                    return name;
            }
            else if (element != null && element.Name.LocalName.Equals("root", StringComparison.InvariantCultureIgnoreCase))
            {
                return "root";
            }

            return "Unknown element";
        }
    }
}