using FluentMigrator;

namespace Migrator.Migrations.V1;

[Migration(202505151130)]
// ReSharper disable once InconsistentNaming
public class M202505151130_Demo : Migration
{
    public override void Up()
    {
        Create.Table("Person");
    }

    public override void Down()
    {
        Delete.Table("Person");
    }
}