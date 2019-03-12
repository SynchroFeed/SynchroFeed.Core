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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace SynchroFeed.Command.Catalog.Entity
{
    /// <summary>The Package class is an Entity Framework model class for a package.</summary>
    public class Package
    {
        /// <summary>Initializes a new instance of the <see cref="T:SynchroFeed.Command.Catalog.Entity.Package"/> class.</summary>
        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
        public Package()
        {
            PackageVersions = new HashSet<PackageVersion>();
            CreatedUtcDateTime = DateTimeOffset.UtcNow;
        }

        /// <summary>Gets or sets the database identifier associated with this package.</summary>
        /// <value>The database identifier associated with this package.</value>
        [Key]
        public int PackageId { get; set; }

        /// <summary>Gets or sets the name of the package.</summary>
        /// <value>The name of the package.</value>
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>Gets or sets the title of the package.</summary>
        /// <value>The title of the package.</value>
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        /// <summary>Gets or sets the versions associated with the package.</summary>
        /// <value>The versions associated with the package.</value>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PackageVersion> PackageVersions { get; set; }

        /// <summary>Gets or sets the date and time the database row was created.</summary>
        /// <value>The date and time the database row was created.</value>
        [Required]
        public DateTimeOffset CreatedUtcDateTime { get; set; }
    }
}
