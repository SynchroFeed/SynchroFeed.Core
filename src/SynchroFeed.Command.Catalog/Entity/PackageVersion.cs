#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="PackageVersion.cs">
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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace SynchroFeed.Command.Catalog.Entity
{
    /// <summary>The PackageVersion class is an Entity Framework model class for an package version associated with a specific package.</summary>
    public class PackageVersion
    {
        /// <summary>Initializes a new instance of the <see cref="T:SynchroFeed.Command.Catalog.Entity.PackageVersion"/> class.</summary>
        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
        public PackageVersion()
        {
            AssemblyVersions = new HashSet<AssemblyVersion>();
            PackageEnvironments = new HashSet<PackageVersionEnvironment>();
            CreatedUtcDateTime = DateTimeOffset.UtcNow;
        }

        /// <summary>Gets or sets the database identifier associated this package version.</summary>
        /// <value>The identifier associated with this pacakge version.</value>
        [Key]
        public int PackageVersionId { get; set; }

        /// <summary>Gets or sets database identifier for the package associated with this package version..</summary>
        /// <value>The database identifier associated with this package version.</value>
        [ForeignKey("Package")]
        public int PackageId { get; set; }

        /// <summary>Gets or sets the version associated with this assembly.</summary>
        /// <value>The version associated with this assembly.</value>
        [Required]
        [Index]
        [StringLength(20)]
        public string Version { get; set; }

        /// <summary>Gets or sets the major version.</summary>
        /// <value>The major version.</value>
        public int MajorVersion { get; set; }

        /// <summary>Gets or sets the minor version.</summary>
        /// <value>The minor version.</value>
        public int MinorVersion { get; set; }

        /// <summary>Gets or sets the build version.</summary>
        /// <value>The build version.</value>
        public int BuildVersion { get; set; }

        /// <summary>Gets or sets the revision version.</summary>
        /// <value>The revision version.</value>
        public int RevisionVersion { get; set; }

        /// <summary>Gets or sets a value indicating whether this package version is a prerelease.</summary>
        /// <value>
        ///   <c>true</c> if this package version is a prerelease; otherwise, <c>false</c>.</value>
        public bool IsPrerelease { get; set; }

        /// <summary>Gets or sets the package associated with this version.</summary>
        /// <value>The package associated with this version.</value>
        public virtual Package Package { get; set; }

        /// <summary>Gets or sets the environments associated with this package version.</summary>
        /// <value>The environments associated with this package version.</value>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PackageVersionEnvironment> PackageEnvironments { get; set; }

        /// <summary>Gets or sets the assembly versions associated with this package version.</summary>
        /// <value>The assembly versions associated with this package version.</value>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AssemblyVersion> AssemblyVersions { get; set; }

        /// <summary>Gets or sets the date and time this entity was added to the database.</summary>
        /// <value>The date and time this entity was added to the database.</value>
        public DateTimeOffset CreatedUtcDateTime { get; set; }
    }
}
