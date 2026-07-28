using Acme.LegalTech.Clauses;
using Acme.LegalTech.Contracts;
using Acme.LegalTech.Obligations;
using Acme.LegalTech.Playbooks;
using Acme.LegalTech.Reviews;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace Acme.LegalTech.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class LegalTechDbContext :
    AbpDbContext<LegalTechDbContext>,
    ITenantManagementDbContext,
    IIdentityDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    public DbSet<Contract> Contracts { get; set; }
    public DbSet<ContractDocumentVersion> ContractDocumentVersions { get; set; }
    public DbSet<CounterpartyReference> CounterpartyReferences { get; set; }
    public DbSet<ContractTag> ContractTags { get; set; }
    public DbSet<ClauseTemplate> ClauseTemplates { get; set; }
    public DbSet<ClauseTaxonomy> ClauseTaxonomies { get; set; }
    public DbSet<PlaybookProfile> PlaybookProfiles { get; set; }
    public DbSet<PlaybookRule> PlaybookRules { get; set; }
    public DbSet<ReviewCase> ReviewCases { get; set; }
    public DbSet<ReviewTask> ReviewTasks { get; set; }
    public DbSet<ApprovalStep> ApprovalSteps { get; set; }
    public DbSet<ReviewComment> ReviewComments { get; set; }
    public DbSet<EscalationEvent> EscalationEvents { get; set; }
    public DbSet<ContractObligation> ContractObligations { get; set; }
    public DbSet<RenewalSchedule> RenewalSchedules { get; set; }
    public DbSet<ObligationReminder> ObligationReminders { get; set; }
    public DbSet<CompletionEvidence> CompletionEvidence { get; set; }


    #region Entities from the modules

    /* Notice: We only implemented IIdentityProDbContext and ISaasDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext and ISaasDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public LegalTechDbContext(DbContextOptions<LegalTechDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureTenantManagement();
        builder.ConfigureBlobStoring();

        /* Configure your own tables/entities inside here */

        builder.Entity<Contract>(b =>
        {
            b.ToTable(LegalTechConsts.DbTablePrefix + "Contracts", LegalTechConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasIndex(c => c.Status);
            b.HasIndex(c => c.Category);
            b.HasIndex(c => c.OwnerUserId);
        });

        builder.Entity<ContractDocumentVersion>(b =>
        {
            b.ToTable(LegalTechConsts.DbTablePrefix + "ContractDocumentVersions", LegalTechConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasIndex(v => v.ContractId);
            b.HasIndex(v => new { v.ContractId, v.IsLatest }).IsUnique();
        });

        builder.Entity<CounterpartyReference>(b =>
        {
            b.ToTable(LegalTechConsts.DbTablePrefix + "CounterpartyReferences", LegalTechConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasIndex(r => r.ContractId);
        });

        builder.Entity<ContractTag>(b =>
        {
            b.ToTable(LegalTechConsts.DbTablePrefix + "ContractTags", LegalTechConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasIndex(t => new { t.ContractId, t.Name }).IsUnique();
        });

        builder.Entity<ClauseTemplate>(b =>
        {
            b.ToTable(LegalTechConsts.DbTablePrefix + "ClauseTemplates", LegalTechConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasIndex(c => c.TenantId);
            b.HasIndex(c => c.TaxonomyId);
            b.HasIndex(c => c.IsActive);
            b.HasIndex(c => c.Category);
        });

        builder.Entity<ClauseTaxonomy>(b =>
        {
            b.ToTable(LegalTechConsts.DbTablePrefix + "ClauseTaxonomies", LegalTechConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasIndex(c => c.TenantId);
            b.HasIndex(c => c.ParentId);
            b.HasIndex(c => c.IsActive);
        });

        builder.Entity<PlaybookProfile>(b =>
        {
            b.ToTable(LegalTechConsts.DbTablePrefix + "PlaybookProfiles", LegalTechConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasIndex(p => p.TenantId);
            b.HasIndex(p => p.IsActive);
        });

        builder.Entity<PlaybookRule>(b =>
        {
            b.ToTable(LegalTechConsts.DbTablePrefix + "PlaybookRules", LegalTechConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasIndex(r => r.PlaybookId);
            b.HasIndex(r => r.TenantId);
        });

        builder.Entity<ReviewCase>(b =>
        {
            b.ToTable(LegalTechConsts.DbTablePrefix + "ReviewCases", LegalTechConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasIndex(r => r.TenantId);
            b.HasIndex(r => r.ContractId);
            b.HasIndex(r => r.Status);
            b.HasIndex(r => r.AssignedUserId);
        });

        builder.Entity<ReviewTask>(b =>
        {
            b.ToTable(LegalTechConsts.DbTablePrefix + "ReviewTasks", LegalTechConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasIndex(t => t.ReviewCaseId);
            b.HasIndex(t => t.TenantId);
            b.HasIndex(t => t.AssignedUserId);
            b.HasIndex(t => t.Status);
        });

        builder.Entity<ApprovalStep>(b =>
        {
            b.ToTable(LegalTechConsts.DbTablePrefix + "ApprovalSteps", LegalTechConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasIndex(s => s.ReviewCaseId);
            b.HasIndex(s => s.TenantId);
            b.HasIndex(s => s.Status);
        });

        builder.Entity<ReviewComment>(b =>
        {
            b.ToTable(LegalTechConsts.DbTablePrefix + "ReviewComments", LegalTechConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasIndex(c => c.ReviewCaseId);
            b.HasIndex(c => c.TenantId);
        });

        builder.Entity<EscalationEvent>(b =>
        {
            b.ToTable(LegalTechConsts.DbTablePrefix + "EscalationEvents", LegalTechConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasIndex(e => e.ReviewCaseId);
            b.HasIndex(e => e.TenantId);
            b.HasIndex(e => e.Severity);
        });

        builder.Entity<ContractObligation>(b =>
        {
            b.ToTable(LegalTechConsts.DbTablePrefix + "ContractObligations", LegalTechConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasIndex(o => o.TenantId);
            b.HasIndex(o => o.ContractId);
            b.HasIndex(o => o.Status);
            b.HasIndex(o => o.DueDate);
        });

        builder.Entity<RenewalSchedule>(b =>
        {
            b.ToTable(LegalTechConsts.DbTablePrefix + "RenewalSchedules", LegalTechConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasIndex(r => r.ContractId);
            b.HasIndex(r => r.TenantId);
            b.HasIndex(r => r.Status);
        });

        builder.Entity<ObligationReminder>(b =>
        {
            b.ToTable(LegalTechConsts.DbTablePrefix + "ObligationReminders", LegalTechConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasIndex(r => r.ObligationId);
            b.HasIndex(r => r.TenantId);
            b.HasIndex(r => r.ReminderDate);
        });

        builder.Entity<CompletionEvidence>(b =>
        {
            b.ToTable(LegalTechConsts.DbTablePrefix + "CompletionEvidence", LegalTechConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasIndex(e => e.ObligationId);
            b.HasIndex(e => e.TenantId);
        });
    }
}
