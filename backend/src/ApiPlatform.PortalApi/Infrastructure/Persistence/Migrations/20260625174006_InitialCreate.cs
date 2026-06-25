using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiPlatform.PortalApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "api",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_api", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "api_usage_daily",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApiId = table.Column<Guid>(type: "uuid", nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    RequestCount = table.Column<int>(type: "integer", nullable: false),
                    ErrorCount = table.Column<int>(type: "integer", nullable: false),
                    AvgLatencyMs = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_api_usage_daily", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "organization_api_pricing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApiId = table.Column<Guid>(type: "uuid", nullable: false),
                    PricePerRequest = table.Column<decimal>(type: "numeric(10,4)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_api_pricing", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "scope",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scope", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "application",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application", x => x.Id);
                    table.ForeignKey(
                        name: "FK_application_organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "portal_user",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portal_user", x => x.Id);
                    table.ForeignKey(
                        name: "FK_portal_user_organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "credential",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SecretHash = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_credential", x => x.Id);
                    table.ForeignKey(
                        name: "FK_credential_application_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "application",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "credential_scope",
                columns: table => new
                {
                    CredentialId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_credential_scope", x => new { x.CredentialId, x.ScopeId });
                    table.ForeignKey(
                        name: "FK_credential_scope_credential_CredentialId",
                        column: x => x.CredentialId,
                        principalTable: "credential",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_credential_scope_scope_ScopeId",
                        column: x => x.ScopeId,
                        principalTable: "scope",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_api_Name",
                table: "api",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_api_usage_daily_OrganizationId_ApplicationId_ApiId_Endpoint~",
                table: "api_usage_daily",
                columns: new[] { "OrganizationId", "ApplicationId", "ApiId", "Endpoint", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_application_OrganizationId",
                table: "application",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_credential_ApplicationId",
                table: "credential",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_credential_ClientId",
                table: "credential",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_credential_OrganizationId",
                table: "credential",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_credential_scope_ScopeId",
                table: "credential_scope",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_organization_api_pricing_OrganizationId_ApiId",
                table: "organization_api_pricing",
                columns: new[] { "OrganizationId", "ApiId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_portal_user_Email",
                table: "portal_user",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_portal_user_OrganizationId",
                table: "portal_user",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_scope_Name",
                table: "scope",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "api");

            migrationBuilder.DropTable(
                name: "api_usage_daily");

            migrationBuilder.DropTable(
                name: "credential_scope");

            migrationBuilder.DropTable(
                name: "organization_api_pricing");

            migrationBuilder.DropTable(
                name: "portal_user");

            migrationBuilder.DropTable(
                name: "credential");

            migrationBuilder.DropTable(
                name: "scope");

            migrationBuilder.DropTable(
                name: "application");

            migrationBuilder.DropTable(
                name: "organization");
        }
    }
}
