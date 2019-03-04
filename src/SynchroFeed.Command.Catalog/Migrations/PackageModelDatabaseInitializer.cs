using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynchroFeed.Command.Catalog.Entity;

namespace SynchroFeed.Command.Catalog.Migrations
{
    internal class PackageModelDatabaseInitializer : IDatabaseInitializer<PackageModelContext>
    {
        private string connectionString;

        /// <summary>
        /// Creates an instance of the PackageModelDatabaseInitializer with the specified connection string.
        /// </summary>
        /// <param name="connectionString">The connection string to use to initialize the database.</param>
        public PackageModelDatabaseInitializer(string connectionString)
        {
            this.connectionString = connectionString;
        }
        public void InitializeDatabase(PackageModelContext context)
        {
            var dbInitializer = new MigrateDatabaseToLatestVersion<PackageModelContext, Configuration>(connectionString);
            dbInitializer.InitializeDatabase(context);
        }
    }
}
