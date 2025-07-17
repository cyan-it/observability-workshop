using FluentMigrator;

namespace Migrator.Migrations.V1;

[Migration(202505151131)]
// ReSharper disable once InconsistentNaming
public class M202505151131_Demo : Migration
{
    public override void Up()
    {
        Create.Table("Person2");
    }

    public override void Down()
    {
        Delete.Table("Person2");
    }
}