using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KiraathaneTactics.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CurrentTurn = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Winner = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    LightPerkCharges = table.Column<int>(type: "int", nullable: false),
                    DarkPerkCharges = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TileDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false),
                    North = table.Column<int>(type: "int", nullable: false),
                    East = table.Column<int>(type: "int", nullable: false),
                    South = table.Column<int>(type: "int", nullable: false),
                    West = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TileDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoardCells",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Row = table.Column<int>(type: "int", nullable: false),
                    Column = table.Column<int>(type: "int", nullable: false),
                    Owner = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TileDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    North = table.Column<int>(type: "int", nullable: false),
                    East = table.Column<int>(type: "int", nullable: false),
                    South = table.Column<int>(type: "int", nullable: false),
                    West = table.Column<int>(type: "int", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    IsFaceDown = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardCells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoardCells_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BoardCells_TileDefinitions_TileDefinitionId",
                        column: x => x.TileDefinitionId,
                        principalTable: "TileDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoardCells_MatchId_Row_Column",
                table: "BoardCells",
                columns: new[] { "MatchId", "Row", "Column" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BoardCells_TileDefinitionId",
                table: "BoardCells",
                column: "TileDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_TileDefinitions_Name",
                table: "TileDefinitions",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoardCells");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "TileDefinitions");
        }
    }
}
