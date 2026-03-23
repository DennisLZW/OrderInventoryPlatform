using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderInventoryPlatform.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddInventoryMovementReason : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Reason",
            table: "InventoryMovements",
            type: "character varying(500)",
            maxLength: 500,
            nullable: false,
            defaultValue: "");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Reason",
            table: "InventoryMovements");
    }
}
