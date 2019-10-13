namespace SynchroFeed.Command.Catalog.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class UpdateViews : DbMigration
    {
        public override void Up()
        {
            Sql("DROP VIEW [dbo].[MaxPackageVersion]");
            Sql("DROP VIEW [dbo].[MaxAssemblyVersion]");

            this.Sql(@"
CREATE VIEW [dbo].[MaxAssemblyVersion]
AS
SELECT
	[A].[AssemblyId],
	[A].[Name],
	[AV].[Version] [MaxVersion],
	[AV].[AssemblyVersionId]
FROM
	[dbo].[Assemblies] [A]
	INNER JOIN [dbo].[AssemblyVersions] [AV]
		ON [A].[AssemblyId] = [AV].[AssemblyId]
	LEFT JOIN [dbo].[AssemblyVersions] [AV2]
		ON [AV].[AssemblyId] = [AV2].[AssemblyId]
		AND (([AV].[MajorVersion] < [AV2].[MajorVersion])
			OR (([AV].[MajorVersion] = [AV2].[MajorVersion]) AND ([AV].[MinorVersion] < [AV2].[MinorVersion]))
			OR (([AV].[MajorVersion] = [AV2].[MajorVersion]) AND ([AV].[MinorVersion] = [AV2].[MinorVersion]) AND ([AV].[BuildVersion] < [AV2].[BuildVersion]))
			OR (([AV].[MajorVersion] = [AV2].[MajorVersion]) AND ([AV].[MinorVersion] = [AV2].[MinorVersion]) AND ([AV].[BuildVersion] = [AV2].[BuildVersion]) AND ([AV].[RevisionVersion] < [AV2].[RevisionVersion])))
WHERE
	[AV2].[AssemblyId] IS NULL
");

            this.Sql(@"
CREATE VIEW [dbo].[MaxPackageVersion]
AS
SELECT
	[P].[PackageId],
	[P].[Name],
	[PV].[Version] [MaxVersion],
	[PV].[PackageVersionId]
FROM
	[dbo].[Packages] [P]
	INNER JOIN [dbo].[PackageVersions] [PV]
		ON [P].[PackageId] = [PV].[PackageId]
	LEFT JOIN [dbo].[PackageVersions] [PV2]
		ON [PV].[PackageId] = [PV2].[PackageId]
		AND (([PV].[MajorVersion] < [PV2].[MajorVersion])
			OR (([PV].[MajorVersion] = [PV2].[MajorVersion]) AND ([PV].[MinorVersion] < [PV2].[MinorVersion]))
			OR (([PV].[MajorVersion] = [PV2].[MajorVersion]) AND ([PV].[MinorVersion] = [PV2].[MinorVersion]) AND ([PV].[BuildVersion] < [PV2].[BuildVersion]))
			OR (([PV].[MajorVersion] = [PV2].[MajorVersion]) AND ([PV].[MinorVersion] = [PV2].[MinorVersion]) AND ([PV].[BuildVersion] = [PV2].[BuildVersion]) AND ([PV].[RevisionVersion] < [PV2].[RevisionVersion])))
WHERE
	[PV2].[PackageId] IS NULL
");
        }

        public override void Down()
        {
            Sql("DROP VIEW [dbo].[MaxPackageVersion]");
            Sql("DROP VIEW [dbo].[MaxAssemblyVersion]");

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
        }
    }
}