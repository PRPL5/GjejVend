using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GjejVend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProviderPaymentAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "payment_account_id",
                table: "parking_providers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payment_account_id",
                table: "parking_providers");
        }
    }
}
