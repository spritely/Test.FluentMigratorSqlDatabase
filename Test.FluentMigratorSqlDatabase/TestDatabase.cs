// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestDatabase.cs">
//   Copyright (c) 2015. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Spritely.Test.FluentMigratorSqlDatabase
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;

    /// <summary>
    ///     A local test database.
    /// </summary>
    public sealed class TestDatabase : IDisposable
    {
        private const string MasterConnectionString =
            "Server=(local)\\Sql2014;Integrated Security=true";

        private const string InstanceConnectionStringFormat =
            "Server=(local)\\Sql2014;Integrated Security=true;Initial Catalog={0}";

        private const string CreateDatabaseCommandFormat = @"
if db_id('{0}') is not null begin
    drop database [{0}];
end
create database [{0}];";
        private const string DropDatabaseCommandFormat = @"
if db_id('{0}') is not null begin
    alter database [{0}] set single_user with rollback immediate;
    drop database [{0}];
end";

        /// <summary>
        ///     Initializes a new instance of the <see cref="TestDatabase" /> class.
        /// </summary>
        /// <param name="databaseFilePath">The database file path.</param>
        public TestDatabase(string databaseFilePath)
        {
            this.DatabaseFilePath = Path.GetFullPath(databaseFilePath);
            this.DatabaseName = Path.GetFileNameWithoutExtension(this.DatabaseFilePath);
        }

        /// <summary>
        ///     Gets the connection string for this database.
        /// </summary>
        /// <value>
        ///     The connection string for this database.
        /// </value>
        public string ConnectionString
        {
            get { return string.Format(CultureInfo.InvariantCulture, InstanceConnectionStringFormat, this.DatabaseName); }
        }

        /// <summary>
        ///     Gets the database file path.
        /// </summary>
        /// <value>
        ///     The database file path.
        /// </value>
        public string DatabaseFilePath { get; private set; }

        /// <summary>
        ///     Gets the name of the database.
        /// </summary>
        /// <value>
        ///     The name of the database.
        /// </value>
        public string DatabaseName { get; private set; }

        private string CreateDatabaseCommand
        {
            get
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    CreateDatabaseCommandFormat,
                    this.DatabaseName);
            }
        }

        private string DropDatabaseCommand
        {
            get { return string.Format(CultureInfo.InvariantCulture, DropDatabaseCommandFormat, this.DatabaseName); }
        }

        /// <summary>
        ///     Creates the database.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities",
            Justification = "This is a safe query as value is privately sourced.")]
        public void Create()
        {
            using (var connection = new SqlConnection(MasterConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = this.CreateDatabaseCommand;
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        ///     Executes the given command against the database inside a newly opened connection which is closed and disposed of on
        ///     completion.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        public void ExecuteCommand(Action<IDbCommand> command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            using (var connection = new SqlConnection(this.ConnectionString))
            {
                connection.Open();
                using (var executeCommand = connection.CreateCommand())
                {
                    command(executeCommand);
                }
            }
        }

        /// <summary>
        ///     Disposes of this instances by dropping the database and deleting the file.
        /// </summary>
        public void Dispose()
        {
            this.DropDatabase();
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities",
            Justification = "This is a safe query as value is privately sourced.")]
        private void DropDatabase()
        {
            using (var connection = new SqlConnection(MasterConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = this.DropDatabaseCommand;
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
