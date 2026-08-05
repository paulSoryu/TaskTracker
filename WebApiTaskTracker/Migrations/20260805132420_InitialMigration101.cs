using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApiTaskTracker.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration101 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Tasks_UserId_Position",
                table: "Tasks",
                columns: new[] { "UserId", "Position" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_UserId_Position",
                table: "Tasks");
        }
    }
}
