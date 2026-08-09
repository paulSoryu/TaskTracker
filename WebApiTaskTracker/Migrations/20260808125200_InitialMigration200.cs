using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApiTaskTracker.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration200 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "Categories",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position",
                table: "Categories");
        }
    }
}
