using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Churn.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPredictionLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PredictionLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PredictedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModelVersion = table.Column<int>(type: "integer", nullable: false),
                    Probability = table.Column<float>(type: "real", nullable: false),
                    AtRisk = table.Column<bool>(type: "boolean", nullable: false),
                    DaysSinceLastEvent = table.Column<float>(type: "real", nullable: false),
                    SessionsLast7Days = table.Column<float>(type: "real", nullable: false),
                    SessionsLast30Days = table.Column<float>(type: "real", nullable: false),
                    ActivityTrend = table.Column<float>(type: "real", nullable: false),
                    PurchasesLast30Days = table.Column<float>(type: "real", nullable: false),
                    PageViewsPerSession = table.Column<float>(type: "real", nullable: false),
                    AccountAgeDays = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PredictionLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PredictionLogs_PredictedAtUtc",
                table: "PredictionLogs",
                column: "PredictedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PredictionLogs");
        }
    }
}
