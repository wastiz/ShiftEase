using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationIdToShiftType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShiftTypes_Organizations_OrganizationId",
                table: "ShiftTypes");

            // Populate OrganizationId from the parent Group for all existing rows
            migrationBuilder.Sql(@"
                UPDATE ""ShiftTypes""
                SET ""OrganizationId"" = g.""OrganizationId""
                FROM ""Groups"" g
                WHERE ""ShiftTypes"".""GroupId"" = g.""Id""
                  AND (""ShiftTypes"".""OrganizationId"" IS NULL OR ""ShiftTypes"".""OrganizationId"" = 0);
            ");

            migrationBuilder.AlterColumn<int>(
                name: "OrganizationId",
                table: "ShiftTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ShiftTypes_Organizations_OrganizationId",
                table: "ShiftTypes",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShiftTypes_Organizations_OrganizationId",
                table: "ShiftTypes");

            migrationBuilder.AlterColumn<int>(
                name: "OrganizationId",
                table: "ShiftTypes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_ShiftTypes_Organizations_OrganizationId",
                table: "ShiftTypes",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");
        }
    }
}
