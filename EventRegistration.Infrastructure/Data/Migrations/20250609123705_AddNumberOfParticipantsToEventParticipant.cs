using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistration.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNumberOfParticipantsToEventParticipant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberOfParticipants",
                table: "Participants");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfParticipants",
                table: "EventParticipants",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberOfParticipants",
                table: "EventParticipants");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfParticipants",
                table: "Participants",
                type: "INTEGER",
                nullable: true);
        }
    }
}
