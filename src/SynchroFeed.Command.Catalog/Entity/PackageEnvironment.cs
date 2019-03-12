#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="PackageEnvironment.cs">
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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SynchroFeed.Command.Catalog.Entity
{
    /// <summary>The PackageVersionEnvironment class is an Entity Framework model class for
    /// the environment associated with a package version.</summary>
    public class PackageVersionEnvironment
    {
        /// <summary>Initializes a new instance of the <see cref="T:SynchroFeed.Command.Catalog.Entity.PackageVersionEnvironment"/> class.</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PackageVersionEnvironment()
        {
            CreatedUtcDateTime = DateTimeOffset.UtcNow;
        }

        /// <summary>Gets or sets the database identifier associated with this package version environment.</summary>
        /// <value>The database identifier associated with this package version environment.</value>
        [Key, Column(Order = 0)]
        [ForeignKey("PackageVersion")]
        public int PackageVersionId { get; set; }

        /// <summary>Gets or sets the name of the environment</summary>
        /// <value>The name of the environment.</value>
        [Key, Column(Order = 1)]
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>Gets or sets the versions associated with the package environment.</summary>
        /// <value>The versions associated with the package environment.</value>
        public virtual PackageVersion PackageVersion { get; set; }

        /// <summary>Gets or sets the date and time the database row was created.</summary>
        /// <value>The date and time the database row was created.</value>
        public DateTimeOffset CreatedUtcDateTime { get; set; }
    }
}
