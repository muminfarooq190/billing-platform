using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService.Infrastructure.Persistence.Migrations
{
    public partial class RemoveLegacyTenantPlan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Raw SQL — table is "tenants" (lowercase, unquoted), column is "Plan" (quoted PascalCase).
            // IF EXISTS guards against drift across environments.
            migrationBuilder.Sql(@"ALTER TABLE IF EXISTS tenants DROP COLUMN IF EXISTS ""Plan"";");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE IF EXISTS tenants ADD COLUMN IF NOT EXISTS ""Plan"" character varying(50) NOT NULL DEFAULT 'Free';");
        }
    }
}
