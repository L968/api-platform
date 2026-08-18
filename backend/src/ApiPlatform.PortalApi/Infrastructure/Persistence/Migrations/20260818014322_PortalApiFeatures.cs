using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiPlatform.PortalApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PortalApiFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "credential",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "application",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "credential");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "application");
        }
    }
}
