using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Acme.LegalTech.Migrations
{
    /// <inheritdoc />
    public partial class _20260731010000_Module04_GovCompliance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Classification",
                table: "AppContracts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ContractValue",
                table: "AppContracts",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentNumber",
                table: "AppContracts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentSeries",
                table: "AppContracts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocumentYear",
                table: "AppContracts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionUntil",
                table: "AppContracts",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppContractSignatories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    PartyType = table.Column<int>(type: "integer", nullable: false),
                    PartyId = table.Column<string>(type: "text", nullable: true),
                    GovernmentAgency = table.Column<string>(type: "text", nullable: true),
                    SignedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Capacity = table.Column<string>(type: "text", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Classification = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppContractSignatories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppContractSignatories_AppContracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "AppContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppGovernmentApprovalTiers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    AmountFrom = table.Column<decimal>(type: "numeric", nullable: false),
                    AmountTo = table.Column<decimal>(type: "numeric", nullable: true),
                    AuthorityTitle = table.Column<string>(type: "text", nullable: false),
                    RequiresNedaReview = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresPresidentApproval = table.Column<bool>(type: "boolean", nullable: false),
                    AllowableVariationPercent = table.Column<decimal>(type: "numeric", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppGovernmentApprovalTiers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppVariationOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    CumulativeAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppVariationOrders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppContractSignatories_ContractId_Role",
                table: "AppContractSignatories",
                columns: new[] { "ContractId", "Role" },
                unique: true,
                filter: "[Role] = 4");

            migrationBuilder.CreateIndex(
                name: "IX_AppGovernmentApprovalTiers_TenantId",
                table: "AppGovernmentApprovalTiers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppGovernmentApprovalTiers_TenantId_AmountFrom_AmountTo",
                table: "AppGovernmentApprovalTiers",
                columns: new[] { "TenantId", "AmountFrom", "AmountTo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppVariationOrders_ContractId",
                table: "AppVariationOrders",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_AppVariationOrders_OrderId",
                table: "AppVariationOrders",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppContractSignatories");

            migrationBuilder.DropTable(
                name: "AppGovernmentApprovalTiers");

            migrationBuilder.DropTable(
                name: "AppVariationOrders");

            migrationBuilder.DropColumn(
                name: "Classification",
                table: "AppContracts");

            migrationBuilder.DropColumn(
                name: "ContractValue",
                table: "AppContracts");

            migrationBuilder.DropColumn(
                name: "DocumentNumber",
                table: "AppContracts");

            migrationBuilder.DropColumn(
                name: "DocumentSeries",
                table: "AppContracts");

            migrationBuilder.DropColumn(
                name: "DocumentYear",
                table: "AppContracts");

            migrationBuilder.DropColumn(
                name: "RetentionUntil",
                table: "AppContracts");
        }
    }
}
