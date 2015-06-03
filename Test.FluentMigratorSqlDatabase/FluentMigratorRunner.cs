// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FluentMigratorRunner.cs">
//   Copyright (c) 2015. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Spritely.Test.FluentMigratorSqlDatabase
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using FluentMigrator;
    using FluentMigrator.Runner;
    using FluentMigrator.Runner.Announcers;
    using FluentMigrator.Runner.Initialization;
    using FluentMigrator.Runner.Processors.SqlServer;

    /// <summary>
    ///     Factory for creating Fluent Migrator MigrationRunner instances.
    /// </summary>
    public static class FluentMigratorRunnerFactory
    {
        /// <summary>
        ///     Creates a migration runner for the specified migration assembly against the given connection string.
        /// </summary>
        /// <param name="assembly">The assembly with migrations.</param>
        /// <param name="connectionString">The connection string of the database to migrate.</param>
        /// <param name="outputWriter">
        ///     The writer where output messages should be written (optional - defaults to
        ///     System.Diagnostics.Debug output).
        /// </param>
        /// <returns>A Fluent MigrationRunner instance.</returns>
        public static MigrationRunner Create(Assembly assembly, string connectionString, Action<string> outputWriter = null)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            if (connectionString == null)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (outputWriter == null)
            {
                outputWriter = s => Debug.WriteLine(s);
            }

            var options = new MigrationOptions { PreviewOnly = false };
            var announcer = new TextWriterAnnouncer(outputWriter);
            var migrationContext = new RunnerContext(announcer);

            var processorFactory = new SqlServerProcessorFactory();
            var processor = processorFactory.Create(connectionString, announcer, options);

            var runner = new MigrationRunner(assembly, migrationContext, processor);
            return runner;
        }

        private class MigrationOptions : IMigrationProcessorOptions
        {
            public bool PreviewOnly { get; set; }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
                Justification = "This is used to get auto-property behavior")]
            public string ProviderSwitches { get; set; }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
                Justification = "This is used to get auto-property behavior")]
            public int Timeout { get; set; }
        }
    }
}
