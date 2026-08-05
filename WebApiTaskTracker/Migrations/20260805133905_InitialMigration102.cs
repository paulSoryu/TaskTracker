using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApiTaskTracker.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration102 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_UserId_Position",
                table: "Tasks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Tasks_UserId_Position",
                table: "Tasks",
                columns: new[] { "UserId", "Position" },
                unique: true);
        }
    }
}
