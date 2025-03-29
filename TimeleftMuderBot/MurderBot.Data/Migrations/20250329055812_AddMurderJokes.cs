using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MurderBot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMurderJokes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MessageSendStageMaxRetries",
                schema: "Murder",
                table: "Group",
                type: "int",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<int>(
                name: "ReminderCheckinMessages",
                schema: "Murder",
                table: "Group",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RemovalStageMaxRetries",
                schema: "Murder",
                table: "Group",
                type: "int",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.CreateTable(
                name: "MurderJoke",
                schema: "Murder",
                columns: table => new
                {
                    MurderJokeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimesTold = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    JokeText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "sysdatetimeoffset()"),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "sysdatetimeoffset()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MurderJoke", x => x.MurderJokeId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MurderJoke",
                schema: "Murder");

            migrationBuilder.DropColumn(
                name: "MessageSendStageMaxRetries",
                schema: "Murder",
                table: "Group");

            migrationBuilder.DropColumn(
                name: "ReminderCheckinMessages",
                schema: "Murder",
                table: "Group");

            migrationBuilder.DropColumn(
                name: "RemovalStageMaxRetries",
                schema: "Murder",
                table: "Group");
        }
    }
}
