using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KnaulCorp.PersonalTools.Worklog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialWorklog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "worklog");

            migrationBuilder.CreateTable(
                name: "entries",
                schema: "worklog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Project = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Epic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Activity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Planned = table.Column<bool>(type: "boolean", nullable: true),
                    Interruption = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Origin = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Result = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entries", x => x.Id);
                    table.CheckConstraint("ck_entries_time", "\"EndedAt\" IS NULL OR \"EndedAt\" > \"StartedAt\"");
                });

            migrationBuilder.CreateIndex(
                name: "IX_entries_StartedAt",
                schema: "worklog",
                table: "entries",
                column: "StartedAt");

            // A exclusão por intervalo também protege duas gravações simultâneas.
            migrationBuilder.Sql("""
                ALTER TABLE worklog.entries ADD CONSTRAINT ex_entries_no_overlap
                EXCLUDE USING gist (tstzrange("StartedAt", "EndedAt", '[)') WITH &&);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entries",
                schema: "worklog");
        }
    }
}
