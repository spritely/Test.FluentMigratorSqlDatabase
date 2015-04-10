// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestDatabaseTest.cs">
//   Copyright (c) 2015. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Spritely.Test.FluentMigratorSqlDatabase.Test
{
    using NUnit.Framework;

    [TestFixture]
    public class TestDatabaseTest
    {
        [Test]
        public void Create_creates_on_disk_database()
        {
            using (var testDatabase = new TestDatabase("Test.mdf"))
            {
                testDatabase.Create();

                testDatabase.ExecuteCommand(
                    c =>
                    {
                        c.CommandText = @"
create table Person
(
    Id int,
    FirstName nvarchar(255),
    LastName nvarchar(255)
);

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
