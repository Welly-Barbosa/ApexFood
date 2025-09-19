using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApexFood.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanoAssinaturaToTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Plano",
                table: "Tenants",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Plano",
                table: "Tenants");
        }
    }
}
