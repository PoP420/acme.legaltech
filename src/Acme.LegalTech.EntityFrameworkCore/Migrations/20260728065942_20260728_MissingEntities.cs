using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acme.LegalTech.Migrations
{
    /// <inheritdoc />
    public partial class _20260728_MissingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppClauseTaxonomies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppClauseTaxonomies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppClauseTaxonomies_AppClauseTaxonomies_ParentId",
                        column: x => x.ParentId,
                        principalTable: "AppClauseTaxonomies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppContractObligations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CompletionEvidenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceClauseReference = table.Column<string>(type: "text", nullable: true),
                    IsRecurring = table.Column<bool>(type: "boolean", nullable: false),
                    RecurrencePattern = table.Column<string>(type: "text", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_AppContractObligations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPlaybookProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_AppPlaybookProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppRenewalSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    NextRenewalDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    RenewalPeriodDays = table.Column<int>(type: "integer", nullable: false),
                    AutoRenew = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRenewalSchedules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppReviewCases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    AssignedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Summary = table.Column<string>(type: "text", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_AppReviewCases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppClauseTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    TaxonomyId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Jurisdiction = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "text", nullable: true),
                    Tags = table.Column<string>(type: "text", nullable: true),
                    RiskLevel = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_AppClauseTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppClauseTemplates_AppClauseTaxonomies_TaxonomyId",
                        column: x => x.TaxonomyId,
                        principalTable: "AppClauseTaxonomies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppCompletionEvidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ObligationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: true),
                    BlobName = table.Column<string>(type: "text", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCompletionEvidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppCompletionEvidence_AppContractObligations_ObligationId",
                        column: x => x.ObligationId,
                        principalTable: "AppContractObligations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppObligationReminders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ObligationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReminderDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ReminderType = table.Column<string>(type: "text", nullable: false),
                    IsSent = table.Column<bool>(type: "boolean", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SentToUserId = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppObligationReminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppObligationReminders_AppContractObligations_ObligationId",
                        column: x => x.ObligationId,
                        principalTable: "AppContractObligations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppPlaybookRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlaybookId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ClausePattern = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    Rationale = table.Column<string>(type: "text", nullable: true),
                    IsPreferred = table.Column<bool>(type: "boolean", nullable: false),
                    IsFallback = table.Column<bool>(type: "boolean", nullable: false),
                    IsProhibited = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPlaybookRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppPlaybookRules_AppPlaybookProfiles_PlaybookId",
                        column: x => x.PlaybookId,
                        principalTable: "AppPlaybookProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppApprovalSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    StepOrder = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ApproverUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppApprovalSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppApprovalSteps_AppReviewCases_ReviewCaseId",
                        column: x => x.ReviewCaseId,
                        principalTable: "AppReviewCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppEscalationEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<string>(type: "text", nullable: false),
                    EscalatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EscalatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Resolution = table.Column<string>(type: "text", nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppEscalationEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppEscalationEvents_AppReviewCases_ReviewCaseId",
                        column: x => x.ReviewCaseId,
                        principalTable: "AppReviewCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppReviewComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppReviewComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppReviewComments_AppReviewCases_ReviewCaseId",
                        column: x => x.ReviewCaseId,
                        principalTable: "AppReviewCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppReviewTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    AssignedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppReviewTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppReviewTasks_AppReviewCases_ReviewCaseId",
                        column: x => x.ReviewCaseId,
                        principalTable: "AppReviewCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppApprovalSteps_ReviewCaseId",
                table: "AppApprovalSteps",
                column: "ReviewCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_AppApprovalSteps_Status",
                table: "AppApprovalSteps",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AppApprovalSteps_TenantId",
                table: "AppApprovalSteps",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppClauseTaxonomies_IsActive",
                table: "AppClauseTaxonomies",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AppClauseTaxonomies_ParentId",
                table: "AppClauseTaxonomies",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppClauseTaxonomies_TenantId",
                table: "AppClauseTaxonomies",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppClauseTemplates_Category",
                table: "AppClauseTemplates",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_AppClauseTemplates_IsActive",
                table: "AppClauseTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AppClauseTemplates_TaxonomyId",
                table: "AppClauseTemplates",
                column: "TaxonomyId");

            migrationBuilder.CreateIndex(
                name: "IX_AppClauseTemplates_TenantId",
                table: "AppClauseTemplates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCompletionEvidence_ObligationId",
                table: "AppCompletionEvidence",
                column: "ObligationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCompletionEvidence_TenantId",
                table: "AppCompletionEvidence",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppContractObligations_ContractId",
                table: "AppContractObligations",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_AppContractObligations_DueDate",
                table: "AppContractObligations",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_AppContractObligations_Status",
                table: "AppContractObligations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AppContractObligations_TenantId",
                table: "AppContractObligations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppEscalationEvents_ReviewCaseId",
                table: "AppEscalationEvents",
                column: "ReviewCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_AppEscalationEvents_Severity",
                table: "AppEscalationEvents",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_AppEscalationEvents_TenantId",
                table: "AppEscalationEvents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppObligationReminders_ObligationId",
                table: "AppObligationReminders",
                column: "ObligationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppObligationReminders_ReminderDate",
                table: "AppObligationReminders",
                column: "ReminderDate");

            migrationBuilder.CreateIndex(
                name: "IX_AppObligationReminders_TenantId",
                table: "AppObligationReminders",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPlaybookProfiles_IsActive",
                table: "AppPlaybookProfiles",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AppPlaybookProfiles_TenantId",
                table: "AppPlaybookProfiles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPlaybookRules_PlaybookId",
                table: "AppPlaybookRules",
                column: "PlaybookId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPlaybookRules_TenantId",
                table: "AppPlaybookRules",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRenewalSchedules_ContractId",
                table: "AppRenewalSchedules",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRenewalSchedules_Status",
                table: "AppRenewalSchedules",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AppRenewalSchedules_TenantId",
                table: "AppRenewalSchedules",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewCases_AssignedUserId",
                table: "AppReviewCases",
                column: "AssignedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewCases_ContractId",
                table: "AppReviewCases",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewCases_Status",
                table: "AppReviewCases",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewCases_TenantId",
                table: "AppReviewCases",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewComments_ReviewCaseId",
                table: "AppReviewComments",
                column: "ReviewCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewComments_TenantId",
                table: "AppReviewComments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewTasks_AssignedUserId",
                table: "AppReviewTasks",
                column: "AssignedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewTasks_ReviewCaseId",
                table: "AppReviewTasks",
                column: "ReviewCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewTasks_Status",
                table: "AppReviewTasks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AppReviewTasks_TenantId",
                table: "AppReviewTasks",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppApprovalSteps");

            migrationBuilder.DropTable(
                name: "AppClauseTemplates");

            migrationBuilder.DropTable(
                name: "AppCompletionEvidence");

            migrationBuilder.DropTable(
                name: "AppEscalationEvents");

            migrationBuilder.DropTable(
                name: "AppObligationReminders");

            migrationBuilder.DropTable(
                name: "AppPlaybookRules");

            migrationBuilder.DropTable(
                name: "AppRenewalSchedules");

            migrationBuilder.DropTable(
                name: "AppReviewComments");

            migrationBuilder.DropTable(
                name: "AppReviewTasks");

            migrationBuilder.DropTable(
                name: "AppClauseTaxonomies");

            migrationBuilder.DropTable(
                name: "AppContractObligations");

            migrationBuilder.DropTable(
                name: "AppPlaybookProfiles");

            migrationBuilder.DropTable(
                name: "AppReviewCases");
        }
    }
}
