using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KiraathaneTactics.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchRooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DarkSeatToken",
                table: "Matches",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DarkTimePenaltySeconds",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LightSeatToken",
                table: "Matches",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LightTimePenaltySeconds",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RoomCode",
                table: "Matches",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartedAt",
                table: "Matches",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TurnDeadlineUtc",
                table: "Matches",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_RoomCode",
                table: "Matches",
                column: "RoomCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Matches_RoomCode",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "DarkSeatToken",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "DarkTimePenaltySeconds",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "LightSeatToken",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "LightTimePenaltySeconds",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "RoomCode",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TurnDeadlineUtc",
                table: "Matches");
        }
    }
}
