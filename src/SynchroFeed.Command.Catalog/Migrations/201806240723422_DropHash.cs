#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="201806240723422_DropHash.cs">
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
    public partial class DropHash : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.AssemblyVersions", new[] { "Version" });
            DropIndex("dbo.PackageVersions", new[] { "Version" });
            AlterColumn("dbo.AssemblyVersions", "Version", c => c.String(nullable: false, maxLength: 20));
            AlterColumn("dbo.PackageVersions", "Version", c => c.String(nullable: false, maxLength: 20));
            CreateIndex("dbo.AssemblyVersions", "Version");
            CreateIndex("dbo.PackageVersions", "Version");
            DropColumn("dbo.AssemblyVersions", "AssemblyVersionHash");
            DropColumn("dbo.PackageVersions", "PackageVersionHash");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PackageVersions", "PackageVersionHash", c => c.String(nullable: false, maxLength: 64, unicode: false));
            AddColumn("dbo.AssemblyVersions", "AssemblyVersionHash", c => c.String(nullable: false, maxLength: 64, unicode: false));
            DropIndex("dbo.PackageVersions", new[] { "Version" });
            DropIndex("dbo.AssemblyVersions", new[] { "Version" });
            AlterColumn("dbo.PackageVersions", "Version", c => c.String(maxLength: 20));
            AlterColumn("dbo.AssemblyVersions", "Version", c => c.String(maxLength: 20));
            CreateIndex("dbo.PackageVersions", "Version");
            CreateIndex("dbo.AssemblyVersions", "Version");
        }
    }
}
