using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShiftTypes_Groups_GroupId",
                table: "ShiftTypes");

            migrationBuilder.RenameTable(
                name: "Groups",
                newName: "Departments");

            migrationBuilder.RenameTable(
                name: "EmployeeInGroups",
                newName: "EmployeeInDepartments");

            migrationBuilder.RenameColumn(
                name: "GroupId",
                table: "ShiftTypes",
                newName: "DepartmentId");

            migrationBuilder.RenameColumn(
                name: "GroupId",
                table: "EmployeeInDepartments",
                newName: "DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_ShiftTypes_GroupId",
                table: "ShiftTypes",
                newName: "IX_ShiftTypes_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Groups_OrganizationId",
                table: "Departments",
                newName: "IX_Departments_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeInGroups_GroupId",
                table: "EmployeeInDepartments",
                newName: "IX_EmployeeInDepartments_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeInGroups_EmployeeId",
                table: "EmployeeInDepartments",
                newName: "IX_EmployeeInDepartments_EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShiftTypes_Departments_DepartmentId",
                table: "ShiftTypes",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShiftTypes_Departments_DepartmentId",
                table: "ShiftTypes");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeInDepartments_EmployeeId",
                table: "EmployeeInDepartments",
                newName: "IX_EmployeeInGroups_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeInDepartments_DepartmentId",
                table: "EmployeeInDepartments",
                newName: "IX_EmployeeInGroups_GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_Departments_OrganizationId",
                table: "Departments",
                newName: "IX_Groups_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_ShiftTypes_DepartmentId",
                table: "ShiftTypes",
                newName: "IX_ShiftTypes_GroupId");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "EmployeeInDepartments",
                newName: "GroupId");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "ShiftTypes",
                newName: "GroupId");

            migrationBuilder.RenameTable(
                name: "EmployeeInDepartments",
                newName: "EmployeeInGroups");

            migrationBuilder.RenameTable(
                name: "Departments",
                newName: "Groups");

            migrationBuilder.AddForeignKey(
                name: "FK_ShiftTypes_Groups_GroupId",
                table: "ShiftTypes",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
