// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FluentMigratorRunnerTest.cs">
//   Copyright (c) 2015. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Spritely.Test.FluentMigratorSqlDatabase.Test
{
    using System.Reflection;
    using NUnit.Framework;

    [TestFixture]
    public class FluentMigratorRunnerTest
    {
        [Test]
        public void Create_creates_runner_capable_of_populating_database()
        {
            using (var testDatabase = new TestDatabase("Create_creates_runner_capable_of_populating_database.mdf"))
            {
                testDatabase.Create();

                // Use TestMigration class in this assembly
                var runner = FluentMigratorRunnerFactory.Create(Assembly.GetExecutingAssembly(), testDatabase.ConnectionString);
                runner.MigrateUp(0);
                testDatabase.ExecuteCommand(
                    c =>
                    {
                        c.CommandText = @"
insert into Person (Id, FirstName, LastName) values (1, 'George', 'Washington');
insert into Person (Id, FirstName, LastName) values (2, 'John', 'Adams');";
                        c.ExecuteNonQuery();
                    });

                testDatabase.ExecuteCommand(
                    c =>
                    {
                        c.CommandText = "select Id, FirstName, LastName from Person where Id = 1";
                        using (var dataReader = c.ExecuteReader())
                        {
                            dataReader.Read();
                            Assert.AreEqual("George", dataReader.GetString(1));
                        }
                    });
            }
        }
    }
}
