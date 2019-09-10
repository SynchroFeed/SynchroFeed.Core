namespace SynchroFeed.Command.Catalog.Migrations
{
    using System.Data.Entity.Migrations;

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
                "dbo.PackageVersionAssemblies",
                c => new
                {
                    AssemblyVersionId = c.Int(nullable: false),
                    PackageVersionId = c.Int(nullable: false),
                    ReferenceIncluded = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => new { t.AssemblyVersionId, t.PackageVersionId })
                .ForeignKey("dbo.AssemblyVersions", t => t.AssemblyVersionId, cascadeDelete: true)
                .ForeignKey("dbo.PackageVersions", t => t.PackageVersionId, cascadeDelete: true)
                .Index(t => t.AssemblyVersionId)
                .Index(t => t.PackageVersionId);

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

            this.Sql(@"
CREATE VIEW [dbo].[AssemblyVersionsView]
AS
SELECT
    [A].[Name],
    [AV].[Version],
    [AV].[MajorVersion],
    [AV].[MinorVersion],
    [AV].[BuildVersion],
    [AV].[RevisionVersion],
    [A].[AssemblyId],
    [AV].[AssemblyVersionId]
FROM
    [dbo].[Assemblies] [A]
    INNER JOIN [dbo].[AssemblyVersions] [AV]
        ON [A].[AssemblyId] = [AV].[AssemblyId]
");

            this.Sql(@"
CREATE VIEW [dbo].[MaxAssemblyVersion]
AS
WITH MaxAssemblyVersion([AssemblyId], [Name], [MaxVersion]) AS
(
    SELECT
        [AssemblyId],
        [Name],
        (
            SELECT TOP 1
                [AV].[Version]
            FROM
                [dbo].[Assemblies] [AA] 
                INNER JOIN [dbo].[AssemblyVersions] [AV]
                    ON [AV].[AssemblyId] = [AA].[AssemblyId]
            WHERE
                [A].[AssemblyId] = [AA].[AssemblyId]
            ORDER BY
                [A].[Name],
                [AV].[MajorVersion] DESC,
                [AV].[MinorVersion] DESC,
                [AV].[BuildVersion] DESC,
                [AV].[RevisionVersion] DESC
        ) [Version]
    FROM
        [dbo].[Assemblies] [A]
)
SELECT
    [AssemblyId],
    [Name],
    [MaxVersion]
FROM
    [MaxAssemblyVersion]
");

            this.Sql(@"
CREATE VIEW [dbo].[MaxPackageVersion]
AS
WITH MaxPackageVersion([PackageId], [Name], [MaxVersion]) AS
(
    SELECT
        [PackageId],
        [Name],
        (
            SELECT TOP 1
                [PV].[Version]
            FROM
                [dbo].[Packages] [PP]
                INNER JOIN [dbo].[PackageVersions] [PV]
                    ON [PV].[PackageId] = [PP].[PackageId]
            WHERE
                [P].[PackageId] = [PP].[PackageId]
            ORDER BY
                [PP].[Name],
                [PV].[MajorVersion] DESC,
                [PV].[MinorVersion] DESC,
                [PV].[BuildVersion] DESC,
                [PV].[RevisionVersion] DESC
        ) [Version]
    FROM
        [dbo].[Packages] [P]
)
SELECT
    [PackageId],
    [Name],
    [MaxVersion]
FROM
    [MaxPackageVersion]
");
            this.Sql(@"
CREATE VIEW [dbo].[PackageVersionAssembliesView]
AS
SELECT
	[P].[PackageId],
	[P].[Name] [PackageName],
	[PV].[PackageVersionId],
	[PV].[Version] [PackageVersion],
	[PV].[MajorVersion] [PackageMajorVersion],
	[PV].[MinorVersion] [PackageMinorVersion], 
    [PV].[RevisionVersion] [PackageRevisionVersion],
	[A].[AssemblyId],
	[A].[Name] [AssemblyName],
	[PVA].[ReferenceIncluded],
	[AV].[AssemblyVersionId],
	[AV].[Version] [AssemblyVersion],
	[AV].[MajorVersion] [AssemblyMajorVersion], 
    [AV].[MinorVersion] [AssemblyMinorVersion],
	[AV].[BuildVersion] [AssemblyBuildVersion],
	[AV].[RevisionVersion] [AssemblyRevisionVersion]
FROM
	[dbo].[PackageVersionAssemblies] [PVA] 
	INNER JOIN [dbo].[PackageVersions] [PV]
		ON [PV].[PackageVersionId] = [PVA].[PackageVersionId]
	INNER JOIN [dbo].[AssemblyVersions] [AV]
		ON [AV].[AssemblyVersionId] = [PVA].[AssemblyVersionId]
	INNER JOIN [dbo].[Assemblies] [A]
		ON [A].[AssemblyId] = [AV].[AssemblyId]
	INNER JOIN [dbo].[Packages] [P]
		ON [P].[PackageId] = [PV].[PackageId]
");
            this.Sql(@"
CREATE VIEW [dbo].[PackageVersionsView]
AS
SELECT
    [P].[Name],
    [P].[Title],
    [PV].[Version],
    [PV].[MajorVersion],
    [PV].[MinorVersion],
    [PV].[BuildVersion],
    [PV].[RevisionVersion]
FROM
    [dbo].[Packages] [P]
    INNER JOIN [dbo].[PackageVersions] [PV]
        ON [P].[PackageId] = [PV].[PackageId]
");
        }

        public override void Down()
        {
            Sql("DROP VIEW [dbo].[PackageVersionsView]");
            Sql("DROP VIEW [dbo].[PackageVersionAssembliesView]");
            Sql("DROP VIEW [dbo].[MaxPackageVersion]");
            Sql("DROP VIEW [dbo].[MaxAssemblyVersion]");
            Sql("DROP VIEW [dbo].[AssemblyVersionsView]");

            DropForeignKey("dbo.AssemblyVersions", "AssemblyId", "dbo.Assemblies");
            DropForeignKey("dbo.PackageVersionAssemblies", "PackageVersionId", "dbo.PackageVersions");
            DropForeignKey("dbo.PackageVersionEnvironments", "PackageVersionId", "dbo.PackageVersions");
            DropForeignKey("dbo.PackageVersions", "PackageId", "dbo.Packages");
            DropForeignKey("dbo.PackageVersionAssemblies", "AssemblyVersionId", "dbo.AssemblyVersions");
            DropIndex("dbo.PackageVersionEnvironments", new[] { "PackageVersionId" });
            DropIndex("dbo.PackageVersions", new[] { "Version" });
            DropIndex("dbo.PackageVersions", new[] { "PackageId" });
            DropIndex("dbo.PackageVersionAssemblies", new[] { "PackageVersionId" });
            DropIndex("dbo.PackageVersionAssemblies", new[] { "AssemblyVersionId" });
            DropIndex("dbo.AssemblyVersions", new[] { "Version" });
            DropIndex("dbo.AssemblyVersions", new[] { "AssemblyId" });
            DropTable("dbo.PackageVersionEnvironments");
            DropTable("dbo.PackageVersionAssemblies");
            DropTable("dbo.PackageVersions");
            DropTable("dbo.Packages");
            DropTable("dbo.AssemblyVersions");
            DropTable("dbo.Assemblies");
        }
    }
}