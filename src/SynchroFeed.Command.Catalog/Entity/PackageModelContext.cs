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
using System.Data.Entity.Migrations;
//using SynchroFeed.Command.Catalog.Entity.View;
using SynchroFeed.Command.Catalog.Migrations;

namespace SynchroFeed.Command.Catalog.Entity
{
    public class PackageModelContext : DbContext
    {
        public PackageModelContext()
        {
        }

        public PackageModelContext(string connectionStringName, string connectionString)
            : base(connectionString)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<PackageModelContext, Configuration>(true));
        }

        public virtual DbSet<Assembly> Assemblies { get; set; }
        public virtual DbSet<AssemblyVersion> AssemblyVersions { get; set; }
        public virtual DbSet<Package> Packages { get; set; }
        public virtual DbSet<PackageVersion> PackageVersions { get; set; }
        public virtual DbSet<PackageVersionEnvironment> PackageEnvironments { get; set; }

        //public virtual DbSet<AssemblyVersionsView> AssemblyVersionsViews { get; set; }
        //public virtual DbSet<MaxPackageVersion> MaxPackageVersions { get; set; }
        //public virtual DbSet<PackageVersionAssembliesView> PackageVersionAssembliesViews { get; set; }

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
