// --------------------------------------------------------------------------------------------------------------------
// <copyright file="V0BaseMigration.cs" company="CoMetrics">
//   Copyright 2015 CoMetrics
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Spritely.Test.FluentMigratorSqlDatabase.Test
{
    using FluentMigrator;
    using System;

    [Migration(0)]
    public class TestMigration : Migration
    {
        public override void Down()
        {
            throw new NotSupportedException("Downgrading the base migration is not supported.");
        }

        public override void Up()
        {
            this.Create.Table("Person")
                .WithColumn("Id").AsInt32().PrimaryKey()
                .WithColumn("FirstName").AsString()
                .WithColumn("LastName").AsString();
        }
    }
}
