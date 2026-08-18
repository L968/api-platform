using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiPlatform.PortalApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class VersionApiPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_organization_api_pricing_organization_id_api_id",
                table: "organization_api_pricing");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "organization_api_pricing");

            migrationBuilder.AddColumn<DateOnly>(
                name: "effective_from",
                table: "organization_api_pricing",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(2000, 1, 1));

            migrationBuilder.CreateIndex(
                name: "IX_organization_api_pricing_organization_id_api_id_effective_f~",
                table: "organization_api_pricing",
                columns: new[] { "organization_id", "api_id", "effective_from" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_organization_api_pricing_organization_id_api_id_effective_f~",
                table: "organization_api_pricing");

            migrationBuilder.DropColumn(
                name: "effective_from",
                table: "organization_api_pricing");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "organization_api_pricing",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_organization_api_pricing_organization_id_api_id",
                table: "organization_api_pricing",
                columns: new[] { "organization_id", "api_id" },
                unique: true);
        }
    }
}
