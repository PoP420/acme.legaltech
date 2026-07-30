using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acme.LegalTech.Migrations
{
    /// <inheritdoc />
    public partial class Module03_DocumentExtraction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppDocumentExtractions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContractDocumentVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderName = table.Column<string>(type: "text", nullable: false),
                    ExtractedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    ExtractedTitle = table.Column<string>(type: "text", nullable: true),
                    ExtractedCounterparty = table.Column<string>(type: "text", nullable: true),
                    ExtractedEffectiveDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ExtractedExpirationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ExtractedCategory = table.Column<string>(type: "text", nullable: true),
                    ExtractedRiskBaseline = table.Column<string>(type: "text", nullable: true),
                    ExtractedContractStatus = table.Column<string>(type: "text", nullable: true),
                    RawResponse = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppDocumentExtractions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppDocumentExtractions_ContractDocumentVersionId",
                table: "AppDocumentExtractions",
                column: "ContractDocumentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_AppDocumentExtractions_ContractDocumentVersionId_ProviderNa~",
                table: "AppDocumentExtractions",
                columns: new[] { "ContractDocumentVersionId", "ProviderName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppDocumentExtractions");
        }
    }
}
