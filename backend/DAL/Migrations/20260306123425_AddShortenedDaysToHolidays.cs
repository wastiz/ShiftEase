using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddShortenedDaysToHolidays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "OrganizationHolidays",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsShortenedDay",
                table: "OrganizationHolidays",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "OrganizationHolidays",
                type: "interval",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "OrganizationHolidays");

            migrationBuilder.DropColumn(
                name: "IsShortenedDay",
                table: "OrganizationHolidays");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "OrganizationHolidays");
        }
    }
}
