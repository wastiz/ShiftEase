using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class BindShiftTypesToGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            // Clear existing shift types — they were tied to organizations,
            // but now belong to groups. Existing rows have no valid GroupId.
            migrationBuilder.Sql("DELETE FROM \"ShiftTypes\";");

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "ShiftTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ShiftTypes_GroupId",
                table: "ShiftTypes",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShiftTypes_Groups_GroupId",
                table: "ShiftTypes",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShiftTypes_Organizations_OrganizationId",
                table: "ShiftTypes",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShiftTypes_Groups_GroupId",
                table: "ShiftTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ShiftTypes_Organizations_OrganizationId",
                table: "ShiftTypes");

            migrationBuilder.DropIndex(
                name: "IX_ShiftTypes_GroupId",
                table: "ShiftTypes");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "ShiftTypes");

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
    }
}
