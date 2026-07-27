using Acme.LegalTech.Contracts;
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
    }
}
