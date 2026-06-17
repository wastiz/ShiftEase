using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentScheduleConditions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultSchedulePattern",
                table: "Departments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int[]>(
                name: "WorkingDays",
                table: "Departments",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultSchedulePattern",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "WorkingDays",
                table: "Departments");
        }
    }
}
