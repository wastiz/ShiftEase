using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedbackResponse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeedbackResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchedulingMethodBefore = table.Column<string>(type: "text", nullable: false),
                    ReferralSource = table.Column<int>(type: "integer", nullable: false),
                    TodaysPurpose = table.Column<string>(type: "text", nullable: false),
                    TaskCompleted = table.Column<int>(type: "integer", nullable: false),
                    Obstacle = table.Column<string>(type: "text", nullable: true),
                    BiggestChallenge = table.Column<string>(type: "text", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    RatingReason = table.Column<string>(type: "text", nullable: false),
                    AdditionalComments = table.Column<string>(type: "text", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackResponses", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedbackResponses");
        }
    }
}
