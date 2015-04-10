﻿// --------------------------------------------------------------------------------------------------------------------
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
    using System.Globalization;
    using System.IO;

    /// <summary>
    ///     A local test database.
    /// </summary>
    public class TestDatabase : IDisposable
    {
        private const string MasterConnectionString =
            "Server=(LocalDB)\\MSSQLLocalDB;Integrated Security=true";

        private const string InstanceConnectionStringFormat =
            "Server=(LocalDB)\\MSSQLLocalDB;Integrated Security=true;Initial Catalog={0}";

        private const string CreateDatabaseCommandFormat = "create database {0} on primary (name={0}, filename='{1}')";
        private const string DropDatabaseCommandFormat = "alter database {0} set single_user with rollback immediate;drop database {0}";

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
                    this.DatabaseName,
                    this.DatabaseFilePath);
            }
        }

        private string DropDatabaseCommand
        {
            get { return string.Format(CultureInfo.InvariantCulture, DropDatabaseCommandFormat, this.DatabaseName); }
        }

        /// <summary>
        ///     Creates the database.
        /// </summary>
        public void Create()
        {
            using (var connection = new SqlConnection(MasterConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = this.CreateDatabaseCommand;
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Message !=
                            string.Format("Database '{0}' already exists. Choose a different database name.", this.DatabaseName))
                        {
                            throw;
                        }

                        // Last time the database didn't get disposed of, often because a test
                        // didn't complete and Dispose wasn't properly called
                        this.DropDatabase();

                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        ///     Executes the given command against the database inside a newly opened connection which is closed and disposed of on
        ///     completion.
        /// </summary>
        /// <param name="executeCommand">The execute command.</param>
        public void ExecuteCommand(Action<IDbCommand> executeCommand)
        {
            using (var connection = new SqlConnection(this.ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    executeCommand(command);
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