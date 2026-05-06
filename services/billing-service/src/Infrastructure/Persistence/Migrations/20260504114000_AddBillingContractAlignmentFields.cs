using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingService.Infrastructure.Persistence.Migrations
{
    public partial class AddBillingContractAlignmentFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "invoice_number",
                table: "invoices",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "current_period_start",
                table: "subscriptions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "current_period_end",
                table: "subscriptions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.Sql(@"
UPDATE invoices
SET invoice_number = CONCAT('INV-', UPPER(SUBSTRING(REPLACE(CAST(""TenantId"" AS text), '-', ''), 1, 6)), '-', TO_CHAR(COALESCE(created_at, NOW()), 'YYYYMMDDHH24MISS'))
WHERE invoice_number = '';
");

            migrationBuilder.Sql(@"
UPDATE subscriptions
SET current_period_start = start_date,
    current_period_end = next_billing_date
WHERE current_period_start = NOW() OR current_period_end = NOW();
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "invoice_number",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "current_period_start",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "current_period_end",
                table: "subscriptions");
        }
    }
}
