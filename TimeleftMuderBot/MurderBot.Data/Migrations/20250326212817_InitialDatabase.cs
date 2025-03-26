using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MurderBot.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Murder");

            migrationBuilder.CreateTable(
                name: "AlwaysRemoveParticipant",
                schema: "Murder",
                columns: table => new
                {
                    AlwaysRemoveParticipantId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParticipantId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GroupId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "sysdatetimeoffset()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlwaysRemoveParticipant", x => x.AlwaysRemoveParticipantId);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessage",
                schema: "Murder",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    WaId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "sysdatetimeoffset()"),
                    SendAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeliverAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ChatId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ParticipantId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    OutgoingMessage = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExemptParticipant",
                schema: "Murder",
                columns: table => new
                {
                    ExemptParticipantId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParticipantId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GroupId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExemptParticipant", x => x.ExemptParticipantId);
                });

            migrationBuilder.CreateTable(
                name: "Group",
                schema: "Murder",
                columns: table => new
                {
                    WId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "sysdatetimeoffset()"),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "sysdatetimeoffset()"),
                    LastParticipantSync = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsBotAdmin = table.Column<bool>(type: "bit", nullable: false),
                    DoMurders = table.Column<bool>(type: "bit", nullable: false),
                    Ignore = table.Column<bool>(type: "bit", nullable: false),
                    CheckInReadTimeout = table.Column<string>(type: "nvarchar(48)", nullable: false),
                    CheckInMessageResponseTimeout = table.Column<string>(type: "nvarchar(48)", nullable: false),
                    LastMessageExemptTime = table.Column<string>(type: "nvarchar(48)", nullable: false),
                    MinimumTimeBetweenRuns = table.Column<string>(type: "nvarchar(48)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Group", x => x.WId);
                });

            migrationBuilder.CreateTable(
                name: "MessageTemplate",
                schema: "Murder",
                columns: table => new
                {
                    MessageTemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "sysdatetimeoffset()"),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "sysdatetimeoffset()"),
                    MessageBody = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MessageTemplateType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageTemplate", x => x.MessageTemplateId);
                });

            migrationBuilder.CreateTable(
                name: "Participant",
                schema: "Murder",
                columns: table => new
                {
                    WId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "sysdatetimeoffset()"),
                    FriendlyName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participant", x => x.WId);
                });

            migrationBuilder.CreateTable(
                name: "GroupAutoReply",
                schema: "Murder",
                columns: table => new
                {
                    GroupAutoReplyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TriggerRegEx = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReplyMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "sysdatetimeoffset()"),
                    GroupId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupAutoReply", x => x.GroupAutoReplyId);
                    table.ForeignKey(
                        name: "FK_GroupAutoReply_Group_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "Murder",
                        principalTable: "Group",
                        principalColumn: "WId");
                });

            migrationBuilder.CreateTable(
                name: "GroupCheckIn",
                schema: "Murder",
                columns: table => new
                {
                    GroupCheckinId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UrlGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "sysdatetimeoffset()"),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "sysdatetimeoffset()"),
                    GroupId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FirstMessageSent = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ParticipantsReadFinished = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ChatResponsesFinished = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RemovalsCompleted = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupCheckIn", x => x.GroupCheckinId);
                    table.ForeignKey(
                        name: "FK_GroupCheckIn_Group_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "Murder",
                        principalTable: "Group",
                        principalColumn: "WId");
                });

            migrationBuilder.CreateTable(
                name: "GroupParticipant",
                schema: "Murder",
                columns: table => new
                {
                    GroupId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ParticipantId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "sysdatetimeoffset()"),
                    LastGroupMessage = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    IsOwner = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupParticipant", x => new { x.GroupId, x.ParticipantId });
                    table.ForeignKey(
                        name: "FK_GroupParticipant_Group_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "Murder",
                        principalTable: "Group",
                        principalColumn: "WId");
                    table.ForeignKey(
                        name: "FK_GroupParticipant_Participant_ParticipantId",
                        column: x => x.ParticipantId,
                        principalSchema: "Murder",
                        principalTable: "Participant",
                        principalColumn: "WId");
                });

            migrationBuilder.CreateTable(
                name: "GroupAutoReplyMessage",
                schema: "Murder",
                columns: table => new
                {
                    GroupAutoReplyMessageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupAutoReplyId = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "sysdatetimeoffset()"),
                    OutgoingMessageId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupAutoReplyMessage", x => x.GroupAutoReplyMessageId);
                    table.ForeignKey(
                        name: "FK_GroupAutoReplyMessage_ChatMessage_OutgoingMessageId",
                        column: x => x.OutgoingMessageId,
                        principalSchema: "Murder",
                        principalTable: "ChatMessage",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GroupAutoReplyMessage_GroupAutoReply_GroupAutoReplyId",
                        column: x => x.GroupAutoReplyId,
                        principalSchema: "Murder",
                        principalTable: "GroupAutoReply",
                        principalColumn: "GroupAutoReplyId");
                });

            migrationBuilder.CreateTable(
                name: "AutoReAddToken",
                schema: "Murder",
                columns: table => new
                {
                    TokenGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "sysdatetimeoffset()"),
                    ExpirationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DateClaimed = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    GroupCheckinId = table.Column<int>(type: "int", nullable: false),
                    ParticipantId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoReAddToken", x => x.TokenGuid);
                    table.ForeignKey(
                        name: "FK_AutoReAddToken_GroupCheckIn_GroupCheckinId",
                        column: x => x.GroupCheckinId,
                        principalSchema: "Murder",
                        principalTable: "GroupCheckIn",
                        principalColumn: "GroupCheckinId");
                });

            migrationBuilder.CreateTable(
                name: "GroupCheckInMessage",
                schema: "Murder",
                columns: table => new
                {
                    GroupCheckinMessageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupCheckinId = table.Column<int>(type: "int", nullable: false),
                    OutgoingMessageId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "sysdatetimeoffset()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupCheckInMessage", x => x.GroupCheckinMessageId);
                    table.ForeignKey(
                        name: "FK_GroupCheckInMessage_ChatMessage_OutgoingMessageId",
                        column: x => x.OutgoingMessageId,
                        principalSchema: "Murder",
                        principalTable: "ChatMessage",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GroupCheckInMessage_GroupCheckIn_GroupCheckinId",
                        column: x => x.GroupCheckinId,
                        principalSchema: "Murder",
                        principalTable: "GroupCheckIn",
                        principalColumn: "GroupCheckinId");
                });

            migrationBuilder.CreateTable(
                name: "GroupCheckInParticipantCheckIn",
                schema: "Murder",
                columns: table => new
                {
                    GroupCheckinId = table.Column<int>(type: "int", maxLength: 30, nullable: false),
                    ParticipantId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "sysdatetimeoffset()"),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "sysdatetimeoffset()"),
                    MessageSentTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CheckInMessageId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    RemovalMessageId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    AutoReAddTokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MessageReceivedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IncomingMessageId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CheckInSuccess = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CheckInMethod = table.Column<int>(type: "int", nullable: false),
                    RemovalTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupCheckInParticipantCheckIn", x => new { x.GroupCheckinId, x.ParticipantId });
                    table.ForeignKey(
                        name: "FK_GroupCheckInParticipantCheckIn_ChatMessage_CheckInMessageId",
                        column: x => x.CheckInMessageId,
                        principalSchema: "Murder",
                        principalTable: "ChatMessage",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GroupCheckInParticipantCheckIn_ChatMessage_IncomingMessageId",
                        column: x => x.IncomingMessageId,
                        principalSchema: "Murder",
                        principalTable: "ChatMessage",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GroupCheckInParticipantCheckIn_ChatMessage_RemovalMessageId",
                        column: x => x.RemovalMessageId,
                        principalSchema: "Murder",
                        principalTable: "ChatMessage",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GroupCheckInParticipantCheckIn_GroupCheckIn_GroupCheckinId",
                        column: x => x.GroupCheckinId,
                        principalSchema: "Murder",
                        principalTable: "GroupCheckIn",
                        principalColumn: "GroupCheckinId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutoReAddToken_GroupCheckinId",
                schema: "Murder",
                table: "AutoReAddToken",
                column: "GroupCheckinId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupAutoReply_GroupId",
                schema: "Murder",
                table: "GroupAutoReply",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupAutoReplyMessage_GroupAutoReplyId",
                schema: "Murder",
                table: "GroupAutoReplyMessage",
                column: "GroupAutoReplyId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupAutoReplyMessage_OutgoingMessageId",
                schema: "Murder",
                table: "GroupAutoReplyMessage",
                column: "OutgoingMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupCheckIn_GroupId",
                schema: "Murder",
                table: "GroupCheckIn",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupCheckInMessage_GroupCheckinId",
                schema: "Murder",
                table: "GroupCheckInMessage",
                column: "GroupCheckinId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupCheckInMessage_OutgoingMessageId",
                schema: "Murder",
                table: "GroupCheckInMessage",
                column: "OutgoingMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupCheckInParticipantCheckIn_CheckInMessageId",
                schema: "Murder",
                table: "GroupCheckInParticipantCheckIn",
                column: "CheckInMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupCheckInParticipantCheckIn_IncomingMessageId",
                schema: "Murder",
                table: "GroupCheckInParticipantCheckIn",
                column: "IncomingMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupCheckInParticipantCheckIn_RemovalMessageId",
                schema: "Murder",
                table: "GroupCheckInParticipantCheckIn",
                column: "RemovalMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupParticipant_ParticipantId",
                schema: "Murder",
                table: "GroupParticipant",
                column: "ParticipantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlwaysRemoveParticipant",
                schema: "Murder");

            migrationBuilder.DropTable(
                name: "AutoReAddToken",
                schema: "Murder");

            migrationBuilder.DropTable(
                name: "ExemptParticipant",
                schema: "Murder");

            migrationBuilder.DropTable(
                name: "GroupAutoReplyMessage",
                schema: "Murder");

            migrationBuilder.DropTable(
                name: "GroupCheckInMessage",
                schema: "Murder");

            migrationBuilder.DropTable(
                name: "GroupCheckInParticipantCheckIn",
                schema: "Murder");

            migrationBuilder.DropTable(
                name: "GroupParticipant",
                schema: "Murder");

            migrationBuilder.DropTable(
                name: "MessageTemplate",
                schema: "Murder");

            migrationBuilder.DropTable(
                name: "GroupAutoReply",
                schema: "Murder");

            migrationBuilder.DropTable(
                name: "ChatMessage",
                schema: "Murder");

            migrationBuilder.DropTable(
                name: "GroupCheckIn",
                schema: "Murder");

            migrationBuilder.DropTable(
                name: "Participant",
                schema: "Murder");

            migrationBuilder.DropTable(
                name: "Group",
                schema: "Murder");
        }
    }
}
