#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="201806240422267_CreateViews.cs">
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
namespace SynchroFeed.Command.Catalog.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    #pragma warning disable 1591

    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Assemblies",
                c => new
                    {
                        AssemblyId = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 100),
                        CreatedUtcDateTime = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.AssemblyId);
            
            CreateTable(
                "dbo.AssemblyVersions",
                c => new
                    {
                        AssemblyVersionId = c.Int(nullable: false, identity: true),
                        AssemblyId = c.Int(nullable: false),
                        FullName = c.String(maxLength: 200),
                        Version = c.String(nullable: false, maxLength: 20),
                        MajorVersion = c.Int(nullable: false),
                        MinorVersion = c.Int(nullable: false),
                        BuildVersion = c.Int(nullable: false),
                        RevisionVersion = c.Int(nullable: false),
                        FrameworkVersion = c.String(maxLength: 100),
                        CreatedUtcDateTime = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.AssemblyVersionId)
                .ForeignKey("dbo.Assemblies", t => t.AssemblyId)
                .Index(t => t.AssemblyId)
                .Index(t => t.Version);
            
            CreateTable(
                "dbo.PackageVersions",
                c => new
                    {
                        PackageVersionId = c.Int(nullable: false, identity: true),
                        PackageId = c.Int(nullable: false),
                        Version = c.String(nullable: false, maxLength: 20),
                        MajorVersion = c.Int(nullable: false),
                        MinorVersion = c.Int(nullable: false),
                        BuildVersion = c.Int(nullable: false),
                        RevisionVersion = c.Int(nullable: false),
                        IsPrerelease = c.Boolean(nullable: false),
                        CreatedUtcDateTime = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.PackageVersionId)
                .ForeignKey("dbo.Packages", t => t.PackageId, cascadeDelete: true)
                .Index(t => t.PackageId)
                .Index(t => t.Version);
            
            CreateTable(
                "dbo.Packages",
                c => new
                    {
                        PackageId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Title = c.String(nullable: false, maxLength: 200),
                        CreatedUtcDateTime = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.PackageId);
            
            CreateTable(
                "dbo.PackageVersionEnvironments",
                c => new
                    {
                        PackageVersionId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        CreatedUtcDateTime = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => new { t.PackageVersionId, t.Name })
                .ForeignKey("dbo.PackageVersions", t => t.PackageVersionId, cascadeDelete: true)
                .Index(t => t.PackageVersionId);
            
            CreateTable(
                "dbo.PackageVersionAssemblies",
                c => new
                    {
                        AssemblyVersionId = c.Int(nullable: false),
                        PackageVersionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.AssemblyVersionId, t.PackageVersionId })
                .ForeignKey("dbo.AssemblyVersions", t => t.AssemblyVersionId, cascadeDelete: true)
                .ForeignKey("dbo.PackageVersions", t => t.PackageVersionId, cascadeDelete: true)
                .Index(t => t.AssemblyVersionId)
                .Index(t => t.PackageVersionId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AssemblyVersions", "AssemblyId", "dbo.Assemblies");
            DropForeignKey("dbo.PackageVersionAssemblies", "PackageVersionId", "dbo.PackageVersions");
            DropForeignKey("dbo.PackageVersionAssemblies", "AssemblyVersionId", "dbo.AssemblyVersions");
            DropForeignKey("dbo.PackageVersionEnvironments", "PackageVersionId", "dbo.PackageVersions");
            DropForeignKey("dbo.PackageVersions", "PackageId", "dbo.Packages");
            DropIndex("dbo.PackageVersionAssemblies", new[] { "PackageVersionId" });
            DropIndex("dbo.PackageVersionAssemblies", new[] { "AssemblyVersionId" });
            DropIndex("dbo.PackageVersionEnvironments", new[] { "PackageVersionId" });
            DropIndex("dbo.PackageVersions", new[] { "Version" });
            DropIndex("dbo.PackageVersions", new[] { "PackageId" });
            DropIndex("dbo.AssemblyVersions", new[] { "Version" });
            DropIndex("dbo.AssemblyVersions", new[] { "AssemblyId" });
            DropTable("dbo.PackageVersionAssemblies");
            DropTable("dbo.PackageVersionEnvironments");
            DropTable("dbo.Packages");
            DropTable("dbo.PackageVersions");
            DropTable("dbo.AssemblyVersions");
            DropTable("dbo.Assemblies");
        }
    }
}
