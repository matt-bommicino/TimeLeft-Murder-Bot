using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MurderBot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReadCountInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastReadCount",
                schema: "Murder",
                table: "GroupCheckIn",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastReadCountCompleted",
                schema: "Murder",
                table: "GroupCheckIn",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastReadCount",
                schema: "Murder",
                table: "GroupCheckIn");

            migrationBuilder.DropColumn(
                name: "LastReadCountCompleted",
                schema: "Murder",
                table: "GroupCheckIn");
        }
    }
}
