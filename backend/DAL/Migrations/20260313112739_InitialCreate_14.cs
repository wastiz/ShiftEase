using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_ShiftTypes_ShiftTypeId",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_ShiftTypeId",
                table: "Shifts");

            migrationBuilder.AddColumn<int>(
                name: "ShiftTemplateId",
                table: "Shifts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"UPDATE ""Shifts"" SET ""ShiftTemplateId"" = ""ShiftTypeId""");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_ShiftTemplateId",
                table: "Shifts",
                column: "ShiftTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_ShiftTypes_ShiftTemplateId",
                table: "Shifts",
                column: "ShiftTemplateId",
                principalTable: "ShiftTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_ShiftTypes_ShiftTemplateId",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_ShiftTemplateId",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "ShiftTemplateId",
                table: "Shifts");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_ShiftTypeId",
                table: "Shifts",
                column: "ShiftTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_ShiftTypes_ShiftTypeId",
                table: "Shifts",
                column: "ShiftTypeId",
                principalTable: "ShiftTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
