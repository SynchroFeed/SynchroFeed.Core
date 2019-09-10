#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="AssemblyVersion.cs">
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
    /// <summary>The AssemblyVersion class is an Entity Framework model class for an assembly version associated with a specific assembly.</summary>
    public class AssemblyVersion
    {
        /// <summary>Initializes a new instance of the <see cref="T:SynchroFeed.Command.Catalog.Entity.AssemblyVersion"/> class.</summary>
        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
        public AssemblyVersion()
        {
            PackageVersionAssemblies = new HashSet<PackageVersionAssembly>();
            CreatedUtcDateTime = DateTimeOffset.UtcNow;
        }

        /// <summary>Gets or sets the database identifier associated this assembly version.</summary>
        /// <value>The identifier associated with this assembly version.</value>
        [Key]
        public int AssemblyVersionId { get; set; }

        /// <summary>Gets or sets database identifier for the assembly associated with this assembly version..</summary>
        /// <value>The database identifier associated with this assembly version.</value>
        [ForeignKey("Assembly")]
        public int AssemblyId { get; set; }

        /// <summary>Gets or sets the full name of this assembly.</summary>
        /// <value>The full name of this assembly including the version number,</value>
        [StringLength(200)]
        public string FullName { get; set; }

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

        /// <summary>Gets or sets the framework version.</summary>
        /// <value>The framework version.</value>
        [StringLength(100)]
        public string FrameworkVersion { get; set; }

        /// <summary>Gets or sets the assembly entity associated with this assembly version entity.</summary>
        /// <value>The assembly entity associaged with this assembly version entity.</value>
        public virtual Assembly Assembly { get; set; }

        /// <summary>Gets or sets the package versions that contain this assembly.</summary>
        /// <value>The package versions that contain this assembly.</value>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PackageVersionAssembly> PackageVersionAssemblies { get; set; }

        /// <summary>Gets or sets the date and time this entity was added to the database.</summary>
        /// <value>The date and time this entity was added to the database.</value>
        public DateTimeOffset CreatedUtcDateTime { get; set; }
    }
}
