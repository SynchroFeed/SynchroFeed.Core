#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="PackageModelContext.cs">
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
using System.Data.Entity;
using SynchroFeed.Command.Catalog.Migrations;

namespace SynchroFeed.Command.Catalog.Entity
{
    /// <summary>The PackageModelContext is the Entity Framework context for persisting the Packages into the database.</summary>
    public class PackageModelContext : DbContext
    {
        /// <summary>Initializes a new instance of the <see cref="T:SynchroFeed.Command.Catalog.Entity.PackageModelContext"/>
        /// class.</summary>
        public PackageModelContext()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:SynchroFeed.Command.Catalog.Entity.PackageModelContext"/> class.</summary>
        /// <param name="connectionString">The connection string to initialize the database context.</param>
        public PackageModelContext(string connectionString)
            : base(connectionString)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<PackageModelContext, Configuration>(true));
        }

        /// <summary>Gets or sets the assemblies database set.</summary>
        /// <value>The assemblies database set.</value>
        public virtual DbSet<Assembly> Assemblies { get; set; }
        /// <summary>Gets or sets the assembly versions database set.</summary>
        /// <value>The assembly versions database set.</value>
        public virtual DbSet<AssemblyVersion> AssemblyVersions { get; set; }
        /// <summary>Gets or sets the packages database set.</summary>
        /// <value>The packages database set.</value>
        public virtual DbSet<Package> Packages { get; set; }
        /// <summary>Gets or sets the package versions database set.</summary>
        /// <value>The package versions database set.</value>
        public virtual DbSet<PackageVersion> PackageVersions { get; set; }
        /// <summary>Gets or sets the package version environments database set.</summary>
        /// <value>The package version environments database set.</value>
        public virtual DbSet<PackageVersionEnvironment> PackageEnvironments { get; set; }

        //public virtual DbSet<AssemblyVersionsView> AssemblyVersionsViews { get; set; }
        //public virtual DbSet<MaxPackageVersion> MaxPackageVersions { get; set; }
        //public virtual DbSet<PackageVersionAssembliesView> PackageVersionAssembliesViews { get; set; }

        /// <summary>
        /// This method is called when the model for a derived context has been initialized, but
        /// before the model has been locked down and used to initialize the context.  The default
        /// implementation of this method does nothing, but it can be overridden in a derived class
        /// such that the model can be further configured before it is locked down.
        /// </summary>
        /// <param name="modelBuilder">The builder that defines the model for the context being created.</param>
        /// <remarks>
        /// Typically, this method is called only once when the first instance of a derived context
        /// is created.  The model for that context is then cached and is for all further instances of
        /// the context in the app domain.  This caching can be disabled by setting the ModelCaching
        /// property on the given ModelBuidler, but note that this can seriously degrade performance.
        /// More control over caching is provided through use of the DbModelBuilder and DbContextFactory
        /// classes directly.
        /// </remarks>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Added to eliminate System.InvalidOperationException due to not finding the EF type for SQL Server
            // ReSharper disable once UnusedVariable
            var instance = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
            modelBuilder.Entity<Assembly>()
                .ToTable("Assemblies")
                .HasMany(e => e.AssemblyVersions)
                .WithRequired(e => e.Assembly)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AssemblyVersion>()
                .HasMany(e => e.PackageVersions)
                .WithMany(e => e.AssemblyVersions)
                .Map(m => m.ToTable("PackageVersionAssemblies").MapLeftKey("AssemblyVersionId").MapRightKey("PackageVersionId"));

            modelBuilder.Entity<Package>()
                .HasMany(e => e.PackageVersions)
                .WithRequired(e => e.Package)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<PackageVersion>()
                .HasMany(e => e.PackageEnvironments)
                .WithRequired(e => e.PackageVersion)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<PackageVersionEnvironment>()
                .HasKey(e => new { e.PackageVersionId, e.Name });
        }
    }
}
