using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acme.LegalTech.Migrations
{
    /// <inheritdoc />
    public partial class _20260715074852_Module02_ContractIntake : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "AppContracts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveDate",
                table: "AppContracts",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "AppContracts",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                table: "AppContracts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RiskBaseline",
                table: "AppContracts",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppContractDocumentVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    BlobName = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadedById = table.Column<Guid>(type: "uuid", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsLatest = table.Column<bool>(type: "boolean", nullable: false),
                    ChangeNote = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppContractDocumentVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppContractTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppContractTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppCounterpartyReferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ExternalReference = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCounterpartyReferences", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppContracts_Category",
                table: "AppContracts",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_AppContracts_OwnerUserId",
                table: "AppContracts",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppContracts_Status",
                table: "AppContracts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AppContractDocumentVersions_ContractId",
                table: "AppContractDocumentVersions",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_AppContractDocumentVersions_ContractId_IsLatest",
                table: "AppContractDocumentVersions",
                columns: new[] { "ContractId", "IsLatest" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppContractTags_ContractId_Name",
                table: "AppContractTags",
                columns: new[] { "ContractId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppCounterpartyReferences_ContractId",
                table: "AppCounterpartyReferences",
                column: "ContractId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppContractDocumentVersions");

            migrationBuilder.DropTable(
                name: "AppContractTags");

            migrationBuilder.DropTable(
                name: "AppCounterpartyReferences");

            migrationBuilder.DropIndex(
                name: "IX_AppContracts_Category",
                table: "AppContracts");

            migrationBuilder.DropIndex(
                name: "IX_AppContracts_OwnerUserId",
                table: "AppContracts");

            migrationBuilder.DropIndex(
                name: "IX_AppContracts_Status",
                table: "AppContracts");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "AppContracts");

            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "AppContracts");

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "AppContracts");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "AppContracts");

            migrationBuilder.DropColumn(
                name: "RiskBaseline",
                table: "AppContracts");
        }
    }
}
