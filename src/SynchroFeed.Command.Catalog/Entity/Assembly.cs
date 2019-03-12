#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="Assembly.cs">
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
    /// <summary>The Assembly class is an Entity Framework model class for an assembly contained within a package.</summary>
    public class Assembly
    {
        /// <summary>Initializes a new instance of the <see cref="T:SynchroFeed.Command.Catalog.Entity.Assembly"/> class.</summary>
        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
        public Assembly()
        {
            AssemblyVersions = new HashSet<AssemblyVersion>();
            CreatedUtcDateTime = DateTimeOffset.UtcNow;
        }

        /// <summary>The identifier assigned to the assembly in the database.</summary>
        /// <value>The assembly identifier.</value>
        [Key]
        public int AssemblyId { get; set; }

        /// <summary>Gets or sets the name of the assembly.</summary>
        /// <value>The name.</value>
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>Gets or sets the versions associated with the assembly.</summary>
        /// <value>The versions associated with the assembly.</value>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AssemblyVersion> AssemblyVersions { get; set; }

        /// <summary>Gets or sets the date and time the database row was created.</summary>
        /// <value>The date and time the database row was created.</value>
        public DateTimeOffset CreatedUtcDateTime { get; set; }
    }
}
