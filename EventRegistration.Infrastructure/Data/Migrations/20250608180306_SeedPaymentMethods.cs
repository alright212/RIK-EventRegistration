using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventRegistration.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedPaymentMethods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "PaymentMethods",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Sularaha" },
                    { 2, "Pangaülekanne" },
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "PaymentMethods", keyColumn: "Id", keyValue: 1);

            migrationBuilder.DeleteData(table: "PaymentMethods", keyColumn: "Id", keyValue: 2);
        }
    }
}
