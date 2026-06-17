using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidatedMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Безопасное удаление constraint только если он существует
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE constraint_name = 'FK_Vacations_Employees_EmployeeId'
                        AND table_name = 'Vacations'
                    ) THEN
                        ALTER TABLE ""Vacations"" DROP CONSTRAINT ""FK_Vacations_Employees_EmployeeId"";
                    END IF;
                END $$;
            ");

            // Безопасное удаление таблицы только если она существует
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""DayOffPreferences"";");

            // Безопасное удаление индекса только если он существует
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Vacations_EmployeeId"";");

            // Безопасное удаление колонок только если они существуют
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'SickLeaves' AND column_name = 'Accepted'
                    ) THEN
                        ALTER TABLE ""SickLeaves"" DROP COLUMN ""Accepted"";
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'SickLeaves' AND column_name = 'Rejected'
                    ) THEN
                        ALTER TABLE ""SickLeaves"" DROP COLUMN ""Rejected"";
                    END IF;
                END $$;
            ");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:day_of_week", "sunday,monday,tuesday,wednesday,thursday,friday,saturday")
                .Annotation("Npgsql:Enum:schedule_pattern", "custom,two_on_two_off,five_on_two_off,three_on_three_off,four_on_four_off")
                .Annotation("Npgsql:Enum:user_role", "unknown,employer,employee")
                .OldAnnotation("Npgsql:Enum:day_of_week", "sunday,monday,tuesday,wednesday,thursday,friday,saturday")
                .OldAnnotation("Npgsql:Enum:user_role", "unknown,employer,employee");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "StartDate",
                table: "Vacations",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "EndDate",
                table: "Vacations",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateTable(
                name: "PersonalDayRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Accepted = table.Column<bool>(type: "boolean", nullable: false),
                    Rejected = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalDayRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersonalDays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalDays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SickLeaveRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Accepted = table.Column<bool>(type: "boolean", nullable: false),
                    Rejected = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SickLeaveRequests", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonalDayRequests");

            migrationBuilder.DropTable(
                name: "PersonalDays");

            migrationBuilder.DropTable(
                name: "SickLeaveRequests");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:day_of_week", "sunday,monday,tuesday,wednesday,thursday,friday,saturday")
                .Annotation("Npgsql:Enum:user_role", "unknown,employer,employee")
                .OldAnnotation("Npgsql:Enum:day_of_week", "sunday,monday,tuesday,wednesday,thursday,friday,saturday")
                .OldAnnotation("Npgsql:Enum:schedule_pattern", "custom,two_on_two_off,five_on_two_off,three_on_three_off,four_on_four_off")
                .OldAnnotation("Npgsql:Enum:user_role", "unknown,employer,employee");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "Vacations",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "Vacations",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<bool>(
                name: "Accepted",
                table: "SickLeaves",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Rejected",
                table: "SickLeaves",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DayOffPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayOffPreferences", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vacations_EmployeeId",
                table: "Vacations",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vacations_Employees_EmployeeId",
                table: "Vacations",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
