using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AshproSAASERP.Repo.Migrations
{
    /// <inheritdoc />
    public partial class untracked : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AccountSubGroup_Code",
                table: "AccountSubGroup",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountMaster_Code",
                table: "AccountMaster",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AccountSubGroup_Code",
                table: "AccountSubGroup");

            migrationBuilder.DropIndex(
                name: "IX_AccountMaster_Code",
                table: "AccountMaster");
        }
    }
}
