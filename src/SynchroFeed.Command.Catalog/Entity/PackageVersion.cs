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
    public class PackageVersion
    {
        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
        public PackageVersion()
        {
            AssemblyVersions = new HashSet<AssemblyVersion>();
            PackageEnvironments = new HashSet<PackageVersionEnvironment>();
            CreatedUtcDateTime = DateTimeOffset.UtcNow;
        }

        [Key]
        public int PackageVersionId { get; set; }

        [ForeignKey("Package")]
        public int PackageId { get; set; }

        [Required]
        [Index]
        [StringLength(20)]
        public string Version { get; set; }

        public int MajorVersion { get; set; }

        public int MinorVersion { get; set; }

        public int BuildVersion { get; set; }

        public int RevisionVersion { get; set; }

        public bool IsPrerelease { get; set; }

        public virtual Package Package { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PackageVersionEnvironment> PackageEnvironments { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AssemblyVersion> AssemblyVersions { get; set; }

        public DateTimeOffset CreatedUtcDateTime { get; set; }
    }
}
