using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZakiSAASERP.PSQL.Migrations.TenantDB
{
    /// <inheritdoc />
    public partial class UpdateTenantModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PermanentSettings_SettingKey",
                table: "PermanentSettings");

            migrationBuilder.DropIndex(
                name: "IX_MenuDetails_Name",
                table: "MenuDetails");

            migrationBuilder.AddColumn<string>(
                name: "InvoiceQRCode",
                table: "SalesInvoiceHdr",
                type: "text",
                nullable: true);

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

            migrationBuilder.DropColumn(
                name: "InvoiceQRCode",
                table: "SalesInvoiceHdr");

            migrationBuilder.CreateIndex(
                name: "IX_PermanentSettings_SettingKey",
                table: "PermanentSettings",
                column: "SettingKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuDetails_Name",
                table: "MenuDetails",
                column: "Name",
                unique: true);
        }
    }
}
