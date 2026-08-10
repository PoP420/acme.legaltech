using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acme.LegalTech.Migrations
{
    /// <inheritdoc />
    public partial class _20260810000000_Module03_AIOrchestration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppIngestionJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContractDocumentVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobType = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ProviderName = table.Column<string>(type: "text", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_AppIngestionJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppSuggestionDecisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    SuggestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SuggestionType = table.Column<string>(type: "text", nullable: false),
                    DeciderUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Decision = table.Column<string>(type: "text", nullable: false),
                    CorrectedValue = table.Column<string>(type: "text", nullable: true),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    DecidedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_AppSuggestionDecisions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppExtractionSuggestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    IngestionJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractDocumentVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldName = table.Column<string>(type: "text", nullable: false),
                    SuggestedValue = table.Column<string>(type: "text", nullable: true),
                    OriginalValue = table.Column<string>(type: "text", nullable: true),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SourceSpan = table.Column<string>(type: "text", nullable: true),
                    ProviderName = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_AppExtractionSuggestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppExtractionSuggestions_AppIngestionJobs_IngestionJobId",
                        column: x => x.IngestionJobId,
                        principalTable: "AppIngestionJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppRiskAssessmentSuggestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    IngestionJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    RiskType = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Severity = table.Column<string>(type: "text", nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RuleId = table.Column<string>(type: "text", nullable: true),
                    ProviderName = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_AppRiskAssessmentSuggestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppRiskAssessmentSuggestions_AppIngestionJobs_IngestionJobId",
                        column: x => x.IngestionJobId,
                        principalTable: "AppIngestionJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppGovernmentApprovalTiers_TenantId_AmountFrom_AmountTo",
                table: "AppGovernmentApprovalTiers",
                columns: new[] { "TenantId", "AmountFrom", "AmountTo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppExtractionSuggestions_ContractDocumentVersionId",
                table: "AppExtractionSuggestions",
                column: "ContractDocumentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_AppExtractionSuggestions_IngestionJobId",
                table: "AppExtractionSuggestions",
                column: "IngestionJobId");

            migrationBuilder.CreateIndex(
                name: "IX_AppExtractionSuggestions_Status",
                table: "AppExtractionSuggestions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AppIngestionJobs_ContractDocumentVersionId",
                table: "AppIngestionJobs",
                column: "ContractDocumentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_AppIngestionJobs_Status",
                table: "AppIngestionJobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AppIngestionJobs_TenantId",
                table: "AppIngestionJobs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRiskAssessmentSuggestions_ContractId",
                table: "AppRiskAssessmentSuggestions",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRiskAssessmentSuggestions_IngestionJobId",
                table: "AppRiskAssessmentSuggestions",
                column: "IngestionJobId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRiskAssessmentSuggestions_Status",
                table: "AppRiskAssessmentSuggestions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AppSuggestionDecisions_DecidedAt",
                table: "AppSuggestionDecisions",
                column: "DecidedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AppSuggestionDecisions_DeciderUserId",
                table: "AppSuggestionDecisions",
                column: "DeciderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSuggestionDecisions_SuggestionId",
                table: "AppSuggestionDecisions",
                column: "SuggestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppExtractionSuggestions");

            migrationBuilder.DropTable(
                name: "AppRiskAssessmentSuggestions");

            migrationBuilder.DropTable(
                name: "AppSuggestionDecisions");

            migrationBuilder.DropTable(
                name: "AppIngestionJobs");

            migrationBuilder.DropIndex(
                name: "IX_AppGovernmentApprovalTiers_TenantId_AmountFrom_AmountTo",
                table: "AppGovernmentApprovalTiers");
        }
    }
}
