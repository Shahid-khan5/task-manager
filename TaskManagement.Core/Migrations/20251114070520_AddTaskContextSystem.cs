using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskContextSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskContexts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TaskId = table.Column<int>(type: "INTEGER", nullable: false),
                    OriginalRequest = table.Column<string>(type: "TEXT", nullable: false),
                    ConversationHistory = table.Column<string>(type: "TEXT", nullable: true),
                    AdditionalNotes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskContexts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskContexts_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskIterations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TaskId = table.Column<int>(type: "INTEGER", nullable: false),
                    IterationNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    WhatWasTried = table.Column<string>(type: "TEXT", nullable: false),
                    Outcome = table.Column<int>(type: "INTEGER", nullable: false),
                    LessonsLearned = table.Column<string>(type: "TEXT", nullable: true),
                    FilesModified = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskIterations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskIterations_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TaskId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReferenceType = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    OriginalFileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    MimeType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskReferences_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskContexts_TaskId",
                table: "TaskContexts",
                column: "TaskId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskIterations_TaskId",
                table: "TaskIterations",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReferences_TaskId",
                table: "TaskReferences",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskContexts");

            migrationBuilder.DropTable(
                name: "TaskIterations");

            migrationBuilder.DropTable(
                name: "TaskReferences");
        }
    }
}
