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
#endregion
using System;
using System.Linq;
using System.Data.Services.Common;
using System.IO;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Zip;

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
        public string Authors { get; set; }

        public byte[] Content { get; set; }

        public string Copyright { get; set; }

        public DateTime? Created { get; set; }

        public string Dependencies { get; set; }

        public string Description { get; set; }

        public int DownloadCount { get; set; }

        public string GalleryDetailsUrl { get; set; }

        public bool HasSource { get; set; }

        public bool HasSymbols { get; set; }

        public string IconUrl { get; set; }

        public string Id { get; set; }

        public bool IsAbsoluteLatestVersion { get; set; }

        public bool IsCached { get; set; }

        public bool IsLatestVersion { get; set; }

        public bool IsLocalPackage { get; set; }

        public bool IsPrerelease { get; set; }

        public bool IsProGetHosted { get; set; }

        public string Language { get; set; }

        public DateTime? LastEdited { get; set; }

        public DateTime? LastUpdated { get; set; }

        public string LicenseNames { get; set; }

        public string LicenseReportUrl { get; set; }

        public string LicenseUrl { get; set; }

        public bool Listed { get; set; }

        public string MinClientVersion { get; set; }

        public string NormalizedVersion { get; set; }

        public string Owners { get; set; }

        public string PackageDownloadUrl { get; set; }

        public string PackageHash { get; set; }

        public string PackageHashAlgorithm { get; set; }

        public long PackageSize { get; set; }

        public string PackageUrl { get; set; }

        public string ProjectUrl { get; set; }

        public DateTime? Published { get; set; }

        public string ReleaseNotes { get; set; }

        public string ReportAbuseUrl { get; set; }

        public bool RequireLicenseAcceptance { get; set; }

        public string Summary { get; set; }

        public string Tags { get; set; }

        public string Title { get; set; }

        public string Version { get; set; }

        public int VersionDownloadCount { get; set; }

        /// <summary>
        /// Creates a package from a file.
        /// </summary>
        /// <param name="filename">The filename to create the package from.</param>
        /// <param name="loadContent">if set to <c>true</c> the binary contents should be loaded.</param>
        /// <returns>Returns an instance of a Package from the filename.</returns>
        public static Package CreateFromFile(string filename, bool loadContent = false)
        {
            var package = new Package();

            using (var inStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var contentBytes = new byte[inStream.Length];
                using (var memoryStream = new MemoryStream(contentBytes))
                {
                    inStream.CopyTo(memoryStream);
                    using (var zipFile = new ZipFile(memoryStream))
                    {
                        foreach (ZipEntry zipEntry in zipFile)
                        {
                            if (zipEntry.Name.EndsWith(".nuspec", StringComparison.CurrentCultureIgnoreCase))
                            {
                                using (var reader = new StreamReader(zipFile.GetInputStream(zipEntry), true))
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
                    }
                }

                if (loadContent)
                {
                    package.Content = contentBytes;
                }
            }

            return package;
        }

        /// <summary>
        /// Clones the package.
        /// </summary>
        /// <returns>Returns a new Package that is a clone of this instance.</returns>
        public Package ClonePackage()
        {
            var clone = new Package();
            clone.Id = this.Id;
            clone.Version = this.Version;
            clone.NormalizedVersion = this.NormalizedVersion;
            clone.Title = this.Title;
            clone.Authors = this.Authors;
            clone.IconUrl = this.IconUrl;
            clone.LicenseUrl = this.LicenseUrl;
            clone.ProjectUrl = this.ProjectUrl;
            clone.ReportAbuseUrl = this.ReportAbuseUrl;
            clone.GalleryDetailsUrl = this.GalleryDetailsUrl;
            clone.DownloadCount = this.DownloadCount;
            clone.VersionDownloadCount = this.VersionDownloadCount;
            clone.RequireLicenseAcceptance = this.RequireLicenseAcceptance;
            clone.Description = this.Description;
            clone.Language = this.Language;
            clone.Summary = this.Summary;
            clone.ReleaseNotes = this.ReleaseNotes;
            clone.Published = this.Published;
            clone.Created = this.Created;
            clone.LastUpdated = this.LastUpdated;
            clone.LastEdited = this.LastEdited;
            clone.Dependencies = this.Dependencies;
            clone.PackageHash = this.PackageHash;
            clone.PackageSize = this.PackageSize;
            clone.PackageHashAlgorithm = this.PackageHashAlgorithm;
            clone.Copyright = this.Copyright;
            clone.Tags = this.Tags;
            clone.IsAbsoluteLatestVersion = this.IsAbsoluteLatestVersion;
            clone.IsLatestVersion = this.IsLatestVersion;
            clone.IsPrerelease = this.IsPrerelease;
            clone.Listed = this.Listed;
            clone.IsLocalPackage = this.IsLocalPackage;
            clone.IsProGetHosted = this.IsProGetHosted;
            clone.IsCached = this.IsCached;
            clone.HasSymbols = this.HasSymbols;
            clone.HasSource = this.HasSource;
            clone.MinClientVersion = this.MinClientVersion;
            clone.LicenseNames = this.LicenseNames;
            clone.LicenseReportUrl = this.LicenseReportUrl;
            clone.Content = this.Content;

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
            return $"{this.Id.ToLower()}{this.Version}".GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that returns the ID of this package.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents ID of this package.</returns>
        public override string ToString()
        {
            return this.Id;
        }
    }
}