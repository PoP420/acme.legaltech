using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acme.LegalTech.Migrations
{
    /// <inheritdoc />
    public partial class _20260804093914_Module04_GovCompliance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Classification",
                table: "AppContracts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.AddColumn<string>(
                name: "LastApprovalAuthorityTitle",
                table: "AppContracts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LastApprovalRequiresNeda",
                table: "AppContracts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LastApprovalRequiresPresident",
                table: "AppContracts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionUntil",
                table: "AppContracts",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppContractSignatories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    PartyType = table.Column<int>(type: "integer", nullable: false),
                    PartyId = table.Column<string>(type: "text", nullable: false),
                    GovernmentAgency = table.Column<string>(type: "text", nullable: false),
                    Capacity = table.Column<string>(type: "text", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    SignedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
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
                    RequiresPresident = table.Column<bool>(type: "boolean", nullable: false),
                    AllowableVariationPercent = table.Column<decimal>(type: "numeric", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
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
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    CumulativeAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppVariationOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppVariationOrders_AppContracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "AppContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppContracts_Classification",
                table: "AppContracts",
                column: "Classification");

            migrationBuilder.CreateIndex(
                name: "IX_AppContracts_DocumentNumber",
                table: "AppContracts",
                column: "DocumentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AppContractSignatories_ContractId",
                table: "AppContractSignatories",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_AppContractSignatories_ContractId_PartyId",
                table: "AppContractSignatories",
                columns: new[] { "ContractId", "PartyId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppGovernmentApprovalTiers_AmountFrom_AmountTo",
                table: "AppGovernmentApprovalTiers",
                columns: new[] { "AmountFrom", "AmountTo" });

            migrationBuilder.CreateIndex(
                name: "IX_AppGovernmentApprovalTiers_TenantId",
                table: "AppGovernmentApprovalTiers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppVariationOrders_ApprovedBy",
                table: "AppVariationOrders",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppVariationOrders_ContractId",
                table: "AppVariationOrders",
                column: "ContractId");
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

            migrationBuilder.DropIndex(
                name: "IX_AppContracts_Classification",
                table: "AppContracts");

            migrationBuilder.DropIndex(
                name: "IX_AppContracts_DocumentNumber",
                table: "AppContracts");

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
                name: "LastApprovalAuthorityTitle",
                table: "AppContracts");

            migrationBuilder.DropColumn(
                name: "LastApprovalRequiresNeda",
                table: "AppContracts");

            migrationBuilder.DropColumn(
                name: "LastApprovalRequiresPresident",
                table: "AppContracts");

            migrationBuilder.DropColumn(
                name: "RetentionUntil",
                table: "AppContracts");
        }
    }
}
