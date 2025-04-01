using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MurderBot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReaddJobTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReAddJobTrigger",
                schema: "Murder",
                columns: table => new
                {
                    ReAddJobTriggerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TokenGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "sysdatetimeoffset()"),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "sysdatetimeoffset()"),
                    JobStartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    JobCompleteDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    StartMessageId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SuccessMessageId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FailureMessageId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReAddJobTrigger", x => x.ReAddJobTriggerId);
                    table.ForeignKey(
                        name: "FK_ReAddJobTrigger_AutoReAddToken_TokenGuid",
                        column: x => x.TokenGuid,
                        principalSchema: "Murder",
                        principalTable: "AutoReAddToken",
                        principalColumn: "TokenGuid");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReAddJobTrigger_TokenGuid",
                schema: "Murder",
                table: "ReAddJobTrigger",
                column: "TokenGuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReAddJobTrigger",
                schema: "Murder");
        }
    }
}
