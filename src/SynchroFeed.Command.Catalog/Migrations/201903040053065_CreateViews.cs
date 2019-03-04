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
using System;
using System.Data.Entity.Migrations;

namespace SynchroFeed.Command.Catalog.Migrations
{
    public partial class CreateViews : DbMigration
    {
        public override void Up()
        {
            this.Sql(@"
CREATE VIEW [dbo].[AssemblyVersionsView]
AS
SELECT dbo.Assemblies.Name, dbo.AssemblyVersions.Version, dbo.AssemblyVersions.MajorVersion, dbo.AssemblyVersions.MinorVersion, dbo.AssemblyVersions.BuildVersion, dbo.AssemblyVersions.RevisionVersion, 
       dbo.Assemblies.AssemblyId, dbo.AssemblyVersions.AssemblyVersionId
FROM dbo.Assemblies 
INNER JOIN dbo.AssemblyVersions ON dbo.Assemblies.AssemblyId = dbo.AssemblyVersions.AssemblyId
");

            this.Sql(@"
CREATE VIEW [dbo].[MaxAssemblyVersion]
AS
WITH MaxAssemblyVersion(Name, MaxVersion) AS (
    SELECT Name,
        (SELECT TOP (1) av.Version
            FROM  dbo.Assemblies AS aa 
            INNER JOIN dbo.AssemblyVersions AS av ON av.AssemblyId = aa.AssemblyId
            WHERE (a.AssemblyId = aa.AssemblyId)
            ORDER BY a.Name, av.MajorVersion DESC, av.MinorVersion DESC, av.BuildVersion DESC, av.RevisionVersion DESC) AS Version
    FROM dbo.Assemblies AS a)
    SELECT Name, MaxVersion
    FROM MaxAssemblyVersion
");

            this.Sql(@"
CREATE VIEW [dbo].[MaxPackageVersion]
AS
WITH MaxPackageVersion(PackageId, Name, MaxVersion) AS (
    SELECT PackageId, Name,
        (SELECT TOP (1) pv.Version
        FROM dbo.Packages AS pp 
        INNER JOIN dbo.PackageVersions AS pv ON pv.PackageId = pp.PackageId
        WHERE (p.PackageId = pp.PackageId)
        ORDER BY pp.Name, pv.MajorVersion DESC, pv.MinorVersion DESC, pv.BuildVersion DESC, pv.RevisionVersion DESC) AS Version
    FROM dbo.Packages AS p)
    SELECT PackageId, Name, MaxVersion
    FROM MaxPackageVersion
");
            this.Sql(@"
CREATE VIEW [dbo].[PackageVersionAssembliesView]
AS
SELECT p.PackageId, p.Name AS PackageName, pv.PackageVersionId, pv.Version AS PackageVersion, pv.MajorVersion AS PackageMajorVersion, pv.MinorVersion AS PackageMinorVersion, 
       pv.RevisionVersion AS PackageRevisionVersion, a.AssemblyId, a.Name AS AssemblyName, av.AssemblyVersionId, av.Version AS AssemblyVersion, av.MajorVersion AS AssemblyMajorVersion, 
       av.MinorVersion AS AssemblyMinorVersion, av.BuildVersion AS AssemblyBuildVersion, av.RevisionVersion AS AssemblyRevisionVersion
FROM dbo.PackageVersionAssemblies AS pva 
INNER JOIN dbo.PackageVersions AS pv ON pv.PackageVersionId = pva.PackageVersionId 
INNER JOIN dbo.AssemblyVersions AS av ON av.AssemblyVersionId = pva.AssemblyVersionId 
INNER JOIN dbo.Assemblies AS a ON a.AssemblyId = av.AssemblyId 
INNER JOIN dbo.Packages AS p ON p.PackageId = pv.PackageId
");
            this.Sql(@"
CREATE VIEW [dbo].[PackageVersionsView]
AS
SELECT dbo.Packages.Name, dbo.Packages.Title, dbo.PackageVersions.Version, dbo.PackageVersions.MajorVersion, dbo.PackageVersions.MinorVersion, dbo.PackageVersions.BuildVersion, dbo.PackageVersions.RevisionVersion
FROM dbo.Packages 
INNER JOIN dbo.PackageVersions ON dbo.Packages.PackageId = dbo.PackageVersions.PackageId
");
        }

        public override void Down()
        {
            Sql("DROP VIEW dbo.PackageVersionsView");
            Sql("DROP VIEW dbo.PackageVersionAssembliesView");
            Sql("DROP VIEW dbo.MaxPackageVersion");
            Sql("DROP VIEW dbo.MaxAssemblyVersion");
            Sql("DROP VIEW dbo.AssemblyVersionsView");
        }
    }
}
