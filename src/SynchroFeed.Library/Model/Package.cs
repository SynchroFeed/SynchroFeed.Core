#region header

// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="Package.cs">
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

using SharpCompress.Archives;
using SynchroFeed.Library.Zip;
using System;
using System.Data.Services.Common;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SynchroFeed.Library.Model
{
    /// <summary>
    /// The Package class defines the properties for the package entity from a Nuget repository.
    /// </summary>
    [EntityPropertyMapping("Summary", SyndicationItemProperty.Summary, SyndicationTextContentKind.Plaintext, false)]
    [EntityPropertyMapping("LastUpdated", SyndicationItemProperty.Updated, SyndicationTextContentKind.Plaintext, false)]
    [HasStream]
    [EntityPropertyMapping("Id", SyndicationItemProperty.Title, SyndicationTextContentKind.Plaintext, false)]
    [EntityPropertyMapping("Authors", SyndicationItemProperty.AuthorName, SyndicationTextContentKind.Plaintext, false)]
    [DataServiceKey("Id", "Version")]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class Package : IEquatable<Package>
    {
        /// <summary>Gets or sets the authors.</summary>
        /// <value>The authors.</value>
        public string Authors { get; set; }

        /// <summary>Gets or sets the package contents.</summary>
        /// <value>The contents of the package.</value>
        public byte[] Content { get; set; }

        /// <summary>Gets or sets the copyright.</summary>
        /// <value>The copyright.</value>
        public string Copyright { get; set; }

        /// <summary>Gets or sets the created date.</summary>
        /// <value>The date the package was created.</value>
        public DateTime? Created { get; set; }

        /// <summary>Gets or sets the dependencies of the package.</summary>
        /// <value>The dependencies associagted with this package.</value>
        public string Dependencies { get; set; }

        /// <summary>Gets or sets the description of the package.</summary>
        /// <value>The description of the package.</value>
        public string Description { get; set; }

        /// <summary>Gets or sets the download count.</summary>
        /// <value>The count of how many times the package was downloaded.</value>
        public int DownloadCount { get; set; }

        /// <summary>Gets or sets the gallery details URL.</summary>
        /// <value>The URL to the gallery details for the package.</value>
        public string GalleryDetailsUrl { get; set; }

        /// <summary>Gets or sets a value indicating whether this package has source code.</summary>
        /// <value>
        ///   <c>true</c> if this package has source code; otherwise, <c>false</c>.</value>
        public bool HasSource { get; set; }

        /// <summary>Gets or sets a value indicating whether this package has symbols.</summary>
        /// <value>
        ///   <c>true</c> if this package has symbols; otherwise, <c>false</c>.</value>
        public bool HasSymbols { get; set; }

        /// <summary>Gets or sets the icon URL.</summary>
        /// <value>The icon URL associaged with this package.</value>
        public string IconUrl { get; set; }

        /// <summary>Gets or sets the identifier associaged with this package.</summary>
        /// <value>The identifier associaged with this package.</value>
        public string Id { get; set; }

        /// <summary>Gets or sets a value indicating whether this package is the absolute latest version.</summary>
        /// <value>
        ///   <c>true</c> if this package is absolute latest version; otherwise, <c>false</c>.</value>
        public bool IsAbsoluteLatestVersion { get; set; }

        /// <summary>Gets or sets a value indicating whether this package is cached.</summary>
        /// <value>
        ///   <c>true</c> if this package is cached; otherwise, <c>false</c>.</value>
        public bool IsCached { get; set; }

        /// <summary>Gets or sets a value indicating whether this package is the latest version.</summary>
        /// <value>
        ///   <c>true</c> if this package is latest version; otherwise, <c>false</c>.</value>
        public bool IsLatestVersion { get; set; }

        /// <summary>Gets or sets a value indicating whether this package is a local package.</summary>
        /// <value>
        ///   <c>true</c> if this package is a local package; otherwise, <c>false</c>.</value>
        public bool IsLocalPackage { get; set; }

        /// <summary>Gets or sets a value indicating whether this package is a prerelease package.</summary>
        /// <value>
        ///   <c>true</c> if this package is a prerelease package; otherwise, <c>false</c>.</value>
        public bool IsPrerelease { get; set; }

        /// <summary>Gets or sets a value indicating whether this package is hosted on Proget.</summary>
        /// <value>
        ///   <c>true</c> if this package is hosted on Proget; otherwise, <c>false</c>.</value>
        public bool IsProGetHosted { get; set; }

        /// <summary>Gets or sets the language of the package.</summary>
        /// <value>The language of the package.</value>
        public string Language { get; set; }

        /// <summary>Gets or sets the last edited date</summary>
        /// <value>The last time the package was edited.</value>
        public DateTime? LastEdited { get; set; }

        /// <summary>Gets or sets the last updated date</summary>
        /// <value>The last time the package was updated.</value>
        public DateTime? LastUpdated { get; set; }

        /// <summary>Gets or sets the name of the license.</summary>
        /// <value>The name of the license associated with this package.</value>
        public string LicenseNames { get; set; }

        /// <summary>Gets or sets the license report URL.</summary>
        /// <value>The license report URL.</value>
        public string LicenseReportUrl { get; set; }

        /// <summary>Gets or sets the license URL.</summary>
        /// <value>The license URL.</value>
        public string LicenseUrl { get; set; }

        /// <summary>Gets or sets a value indicating whether this <see cref="T:SynchroFeed.Library.Model.Package"/> is listed.</summary>
        /// <value>
        ///   <c>true</c> if listed; otherwise, <c>false</c>.</value>
        public bool Listed { get; set; }

        /// <summary>Gets or sets the minimum client version.</summary>
        /// <value>The minimum client version.</value>
        public string MinClientVersion { get; set; }

        /// <summary>Gets or sets the normalized version.</summary>
        /// <value>The normalized version.</value>
        public string NormalizedVersion { get; set; }

        /// <summary>Gets or sets the owners of the package.</summary>
        /// <value>The owners of the package.</value>
        public string Owners { get; set; }

        /// <summary>Gets or sets the package download URL.</summary>
        /// <value>The package download URL.</value>
        public string PackageDownloadUrl { get; set; }

        /// <summary>Gets or sets the package hash.</summary>
        /// <value>The package hash.</value>
        public string PackageHash { get; set; }

        /// <summary>Gets or sets the package hash algorithm.</summary>
        /// <value>The package hash algorithm.</value>
        public string PackageHashAlgorithm { get; set; }

        /// <summary>Gets or sets the size of the package.</summary>
        /// <value>The size of the package.</value>
        public long PackageSize { get; set; }

        /// <summary>Gets or sets the package URL.</summary>
        /// <value>The package URL.</value>
        public string PackageUrl { get; set; }

        /// <summary>Gets or sets the project URL.</summary>
        /// <value>The project URL.</value>
        public string ProjectUrl { get; set; }

        /// <summary>Gets or sets the date the package was published.</summary>
        /// <value>The published date of the package.</value>
        public DateTime? Published { get; set; }

        /// <summary>Gets or sets the release notes associated with this package.</summary>
        /// <value>The release notes for this package.</value>
        public string ReleaseNotes { get; set; }

        /// <summary>Gets or sets the report abuse URL.</summary>
        /// <value>The report abuse URL.</value>
        public string ReportAbuseUrl { get; set; }

        /// <summary>Gets or sets a value indicating whether license acceptance is required to install the package.</summary>
        /// <value>
        ///   <c>true</c> if license acceptance is required to install this package; otherwise, <c>false</c>.</value>
        public bool RequireLicenseAcceptance { get; set; }

        /// <summary>Gets or sets the package summary.</summary>
        /// <value>The package summary.</value>
        public string Summary { get; set; }

        /// <summary>Gets or sets the tags associated with the package.</summary>
        /// <value>The tags associated with the package.</value>
        public string Tags { get; set; }

        /// <summary>Gets or sets the title associated with the package.</summary>
        /// <value>The title associated with the package.</value>
        public string Title { get; set; }

        /// <summary>Gets or sets the version associated with this package.</summary>
        /// <value>The version associated with this package.</value>
        public string Version { get; set; }

        /// <summary>Gets or sets the version download count.</summary>
        /// <value>The count of how manu times this version of the package has been downloaded.</value>
        public int VersionDownloadCount { get; set; }

        /// <summary>
        /// Creates a package from a file.
        /// </summary>
        /// <param name="filename">The filename to create the package from.</param>
        /// <returns>Returns an instance of a Package from the filename.</returns>
        public static Package CreateFromFile(string filename)
        {
            var package = new Package();

            using (var archive = ArchiveFactory.Open(filename))
            {
                foreach (var archiveEntry in archive.Entries)
                {
                    if (archiveEntry.Key.EndsWith(".nuspec", StringComparison.CurrentCultureIgnoreCase))
                    {
                        using (var stream = archiveEntry.ExtractToStream())
                        using (var reader = new StreamReader(stream, true))
                        {
                            var xdoc = XElement.Load(reader);
                            package.Id = GetXmlValue<string>("id");
                            package.Version = GetXmlValue<string>("version");
                            package.Authors = GetXmlValue<string>("authors");
                            package.Owners = GetXmlValue<string>("owners");
                            package.LicenseUrl = GetXmlValue<string>("licenseUrl");
                            package.ProjectUrl = GetXmlValue<string>("projectUrl");
                            package.IconUrl = GetXmlValue<string>("iconUrl");
                            package.RequireLicenseAcceptance = GetXmlValue<bool>("requireLicenseAcceptance");
                            package.Description = GetXmlValue<string>("description");
                            package.ReleaseNotes = GetXmlValue<string>("releaseNotes");
                            package.Copyright = GetXmlValue<string>("copyright");
                            package.Tags = GetXmlValue<string>("tags");
                            package.IsPrerelease = package.Version.IsPrerelease();

                            // Dependencies should have the following format:
                            // Common.Logging:3.3.1|Common.Logging.Core:3.3.1|Newtonsoft.Json:8.0.3
                            if (xdoc.Descendants().Any(e => e.Name.LocalName == "dependency"))
                            {
                                package.Dependencies = xdoc.Descendants().Where(e => e.Name.LocalName == "dependency")
                                    .Select(a => $"{a.Attribute("id")?.Value}:{a.Attribute("version")?.Value}")
                                    .Aggregate((current, next) => current + "|" + next);
                            }

                            T GetXmlValue<T>(string elementName)
                            {
                                var value = xdoc.Descendants().FirstOrDefault(e => e.Name.LocalName == elementName)?.Value;
                                if (value == null)
                                    return default(T);
                                return (T)Convert.ChangeType(value, typeof(T));
                            }
                        }

                        break;
                    }
                }

                package.Content = File.ReadAllBytes(filename);
            }

            return package;
        }

        /// <summary>
        /// Clones the package.
        /// </summary>
        /// <returns>Returns a new Package that is a clone of this instance.</returns>
        public Package ClonePackage()
        {
            var clone = new Package
            {
                Id = this.Id,
                Version = this.Version,
                NormalizedVersion = this.NormalizedVersion,
                Title = this.Title,
                Authors = this.Authors,
                IconUrl = this.IconUrl,
                LicenseUrl = this.LicenseUrl,
                ProjectUrl = this.ProjectUrl,
                ReportAbuseUrl = this.ReportAbuseUrl,
                GalleryDetailsUrl = this.GalleryDetailsUrl,
                DownloadCount = this.DownloadCount,
                VersionDownloadCount = this.VersionDownloadCount,
                RequireLicenseAcceptance = this.RequireLicenseAcceptance,
                Description = this.Description,
                Language = this.Language,
                Summary = this.Summary,
                ReleaseNotes = this.ReleaseNotes,
                Published = this.Published,
                Created = this.Created,
                LastUpdated = this.LastUpdated,
                LastEdited = this.LastEdited,
                Dependencies = this.Dependencies,
                PackageHash = this.PackageHash,
                PackageSize = this.PackageSize,
                PackageHashAlgorithm = this.PackageHashAlgorithm,
                Copyright = this.Copyright,
                Tags = this.Tags,
                IsAbsoluteLatestVersion = this.IsAbsoluteLatestVersion,
                IsLatestVersion = this.IsLatestVersion,
                IsPrerelease = this.IsPrerelease,
                Listed = this.Listed,
                IsLocalPackage = this.IsLocalPackage,
                IsProGetHosted = this.IsProGetHosted,
                IsCached = this.IsCached,
                HasSymbols = this.HasSymbols,
                HasSource = this.HasSource,
                MinClientVersion = this.MinClientVersion,
                LicenseNames = this.LicenseNames,
                LicenseReportUrl = this.LicenseReportUrl,
                Content = this.Content
            };

            return clone;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Package);
        }

        /// <summary>
        /// Equalses the specified package.
        /// </summary>
        /// <param name="package">The package to compare equality.</param>
        /// <returns><c>true</c> if the package ID and version of the two packages match, <c>false</c> otherwise.</returns>
        public bool Equals(Package package)
        {
            if (package == null)
                return false;
            return this.Id.Equals(package.Id, StringComparison.CurrentCultureIgnoreCase) && this.Version.Equals(package.Version);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            return this.ToString().ToLower().GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that returns the ID of this package.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents ID of this package.</returns>
        public override string ToString()
        {
            return $"{this.Id} v{this.Version}";
        }
    }
}