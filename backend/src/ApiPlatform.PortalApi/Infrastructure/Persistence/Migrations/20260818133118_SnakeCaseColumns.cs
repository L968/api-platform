using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiPlatform.PortalApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SnakeCaseColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_application_organization_OrganizationId",
                table: "application");

            migrationBuilder.DropForeignKey(
                name: "FK_credential_application_ApplicationId",
                table: "credential");

            migrationBuilder.DropForeignKey(
                name: "FK_credential_scope_credential_CredentialId",
                table: "credential_scope");

            migrationBuilder.DropForeignKey(
                name: "FK_credential_scope_scope_ScopeId",
                table: "credential_scope");

            migrationBuilder.DropForeignKey(
                name: "FK_portal_user_organization_OrganizationId",
                table: "portal_user");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "scope",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "scope",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_scope_Name",
                table: "scope",
                newName: "IX_scope_name");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "portal_user",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "portal_user",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "portal_user",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "portal_user",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "portal_user",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "portal_user",
                newName: "organization_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "portal_user",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_portal_user_Email",
                table: "portal_user",
                newName: "IX_portal_user_email");

            migrationBuilder.RenameIndex(
                name: "IX_portal_user_OrganizationId",
                table: "portal_user",
                newName: "IX_portal_user_organization_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "organization_api_pricing",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "organization_api_pricing",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "PricePerRequest",
                table: "organization_api_pricing",
                newName: "price_per_request");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "organization_api_pricing",
                newName: "organization_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "organization_api_pricing",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ApiId",
                table: "organization_api_pricing",
                newName: "api_id");

            migrationBuilder.RenameIndex(
                name: "IX_organization_api_pricing_OrganizationId_ApiId",
                table: "organization_api_pricing",
                newName: "IX_organization_api_pricing_organization_id_api_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "organization",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "organization",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "organization",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "organization",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ScopeId",
                table: "credential_scope",
                newName: "scope_id");

            migrationBuilder.RenameColumn(
                name: "CredentialId",
                table: "credential_scope",
                newName: "credential_id");

            migrationBuilder.RenameIndex(
                name: "IX_credential_scope_ScopeId",
                table: "credential_scope",
                newName: "IX_credential_scope_scope_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "credential",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "credential",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "SecretHash",
                table: "credential",
                newName: "secret_hash");

            migrationBuilder.RenameColumn(
                name: "RevokedAt",
                table: "credential",
                newName: "revoked_at");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "credential",
                newName: "organization_id");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "credential",
                newName: "expires_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "credential",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "credential",
                newName: "client_id");

            migrationBuilder.RenameColumn(
                name: "ApplicationId",
                table: "credential",
                newName: "application_id");

            migrationBuilder.RenameIndex(
                name: "IX_credential_OrganizationId",
                table: "credential",
                newName: "IX_credential_organization_id");

            migrationBuilder.RenameIndex(
                name: "IX_credential_ClientId",
                table: "credential",
                newName: "IX_credential_client_id");

            migrationBuilder.RenameIndex(
                name: "IX_credential_ApplicationId",
                table: "credential",
                newName: "IX_credential_application_id");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "application",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "application",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "application",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "application",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "application",
                newName: "organization_id");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "application",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "application",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_application_OrganizationId",
                table: "application",
                newName: "IX_application_organization_id");

            migrationBuilder.RenameColumn(
                name: "Endpoint",
                table: "api_usage_daily",
                newName: "endpoint");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "api_usage_daily",
                newName: "date");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "api_usage_daily",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "RequestCount",
                table: "api_usage_daily",
                newName: "request_count");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "api_usage_daily",
                newName: "organization_id");

            migrationBuilder.RenameColumn(
                name: "ErrorCount",
                table: "api_usage_daily",
                newName: "error_count");

            migrationBuilder.RenameColumn(
                name: "AvgLatencyMs",
                table: "api_usage_daily",
                newName: "avg_latency_ms");

            migrationBuilder.RenameColumn(
                name: "ApplicationId",
                table: "api_usage_daily",
                newName: "application_id");

            migrationBuilder.RenameColumn(
                name: "ApiId",
                table: "api_usage_daily",
                newName: "api_id");

            migrationBuilder.RenameIndex(
                name: "IX_api_usage_daily_OrganizationId_ApplicationId_ApiId_Endpoint~",
                table: "api_usage_daily",
                newName: "IX_api_usage_daily_organization_id_application_id_api_id_endpo~");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "api",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "api",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_api_Name",
                table: "api",
                newName: "IX_api_name");

            migrationBuilder.AddForeignKey(
                name: "FK_application_organization_organization_id",
                table: "application",
                column: "organization_id",
                principalTable: "organization",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_credential_application_application_id",
                table: "credential",
                column: "application_id",
                principalTable: "application",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_credential_scope_credential_credential_id",
                table: "credential_scope",
                column: "credential_id",
                principalTable: "credential",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_credential_scope_scope_scope_id",
                table: "credential_scope",
                column: "scope_id",
                principalTable: "scope",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_portal_user_organization_organization_id",
                table: "portal_user",
                column: "organization_id",
                principalTable: "organization",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_application_organization_organization_id",
                table: "application");

            migrationBuilder.DropForeignKey(
                name: "FK_credential_application_application_id",
                table: "credential");

            migrationBuilder.DropForeignKey(
                name: "FK_credential_scope_credential_credential_id",
                table: "credential_scope");

            migrationBuilder.DropForeignKey(
                name: "FK_credential_scope_scope_scope_id",
                table: "credential_scope");

            migrationBuilder.DropForeignKey(
                name: "FK_portal_user_organization_organization_id",
                table: "portal_user");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "scope",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "scope",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_scope_name",
                table: "scope",
                newName: "IX_scope_Name");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "portal_user",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "portal_user",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "portal_user",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "portal_user",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "portal_user",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                table: "portal_user",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "portal_user",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_portal_user_email",
                table: "portal_user",
                newName: "IX_portal_user_Email");

            migrationBuilder.RenameIndex(
                name: "IX_portal_user_organization_id",
                table: "portal_user",
                newName: "IX_portal_user_OrganizationId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "organization_api_pricing",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "organization_api_pricing",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "price_per_request",
                table: "organization_api_pricing",
                newName: "PricePerRequest");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                table: "organization_api_pricing",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "organization_api_pricing",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "api_id",
                table: "organization_api_pricing",
                newName: "ApiId");

            migrationBuilder.RenameIndex(
                name: "IX_organization_api_pricing_organization_id_api_id",
                table: "organization_api_pricing",
                newName: "IX_organization_api_pricing_OrganizationId_ApiId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "organization",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "organization",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "organization",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "organization",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "scope_id",
                table: "credential_scope",
                newName: "ScopeId");

            migrationBuilder.RenameColumn(
                name: "credential_id",
                table: "credential_scope",
                newName: "CredentialId");

            migrationBuilder.RenameIndex(
                name: "IX_credential_scope_scope_id",
                table: "credential_scope",
                newName: "IX_credential_scope_ScopeId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "credential",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "credential",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "secret_hash",
                table: "credential",
                newName: "SecretHash");

            migrationBuilder.RenameColumn(
                name: "revoked_at",
                table: "credential",
                newName: "RevokedAt");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                table: "credential",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "expires_at",
                table: "credential",
                newName: "ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "credential",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "client_id",
                table: "credential",
                newName: "ClientId");

            migrationBuilder.RenameColumn(
                name: "application_id",
                table: "credential",
                newName: "ApplicationId");

            migrationBuilder.RenameIndex(
                name: "IX_credential_organization_id",
                table: "credential",
                newName: "IX_credential_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_credential_client_id",
                table: "credential",
                newName: "IX_credential_ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_credential_application_id",
                table: "credential",
                newName: "IX_credential_ApplicationId");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "application",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "application",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "application",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "application",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                table: "application",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "application",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "application",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_application_organization_id",
                table: "application",
                newName: "IX_application_OrganizationId");

            migrationBuilder.RenameColumn(
                name: "endpoint",
                table: "api_usage_daily",
                newName: "Endpoint");

            migrationBuilder.RenameColumn(
                name: "date",
                table: "api_usage_daily",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "api_usage_daily",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "request_count",
                table: "api_usage_daily",
                newName: "RequestCount");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                table: "api_usage_daily",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "error_count",
                table: "api_usage_daily",
                newName: "ErrorCount");

            migrationBuilder.RenameColumn(
                name: "avg_latency_ms",
                table: "api_usage_daily",
                newName: "AvgLatencyMs");

            migrationBuilder.RenameColumn(
                name: "application_id",
                table: "api_usage_daily",
                newName: "ApplicationId");

            migrationBuilder.RenameColumn(
                name: "api_id",
                table: "api_usage_daily",
                newName: "ApiId");

            migrationBuilder.RenameIndex(
                name: "IX_api_usage_daily_organization_id_application_id_api_id_endpo~",
                table: "api_usage_daily",
                newName: "IX_api_usage_daily_OrganizationId_ApplicationId_ApiId_Endpoint~");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "api",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "api",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_api_name",
                table: "api",
                newName: "IX_api_Name");

            migrationBuilder.AddForeignKey(
                name: "FK_application_organization_OrganizationId",
                table: "application",
                column: "OrganizationId",
                principalTable: "organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_credential_application_ApplicationId",
                table: "credential",
                column: "ApplicationId",
                principalTable: "application",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_credential_scope_credential_CredentialId",
                table: "credential_scope",
                column: "CredentialId",
                principalTable: "credential",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_credential_scope_scope_ScopeId",
                table: "credential_scope",
                column: "ScopeId",
                principalTable: "scope",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_portal_user_organization_OrganizationId",
                table: "portal_user",
                column: "OrganizationId",
                principalTable: "organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
