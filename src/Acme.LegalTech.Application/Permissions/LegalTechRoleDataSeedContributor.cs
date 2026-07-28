using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acme.LegalTech.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;

namespace Acme.LegalTech.Permissions;

public class LegalTechRoleDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly IdentityRoleManager _roleManager;
    private readonly IPermissionGrantRepository _permissionGrantRepository;

    public LegalTechRoleDataSeedContributor(
        IIdentityRoleRepository roleRepository,
        IdentityRoleManager roleManager,
        IPermissionGrantRepository permissionGrantRepository)
    {
        _roleRepository = roleRepository;
        _roleManager = roleManager;
        _permissionGrantRepository = permissionGrantRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        foreach (var roleName in LegalTechRoles.All)
        {
            await CreateRoleIfNotExistsAsync(roleName);
        }

        await GrantContractsPermissionsAsync();
        await GrantClausesPermissionsAsync();
        await GrantPlaybooksPermissionsAsync();
        await GrantReviewsPermissionsAsync();
        await GrantObligationsPermissionsAsync();
        await GrantRenewalsPermissionsAsync();
        await GrantReportsPermissionsAsync();
        await GrantDashboardsPermissionsAsync();
        await GrantFilesPermissionsAsync();
        await GrantAdministrationPermissionsAsync();
    }

    private async Task CreateRoleIfNotExistsAsync(string roleName)
    {
        if (await _roleRepository.FindByNormalizedNameAsync(roleName.ToUpperInvariant()) != null)
        {
            return;
        }

        var role = new IdentityRole(System.Guid.NewGuid(), roleName);
        var result = await _roleManager.CreateAsync(role);

        if (!result.Succeeded)
        {
            throw new Volo.Abp.AbpException(
                $"Could not create role '{roleName}': " +
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    private async Task GrantPermissionsAsync(
        IEnumerable<string> permissionNames,
        string[] roleNames)
    {
        foreach (var roleName in roleNames)
        {
            var role = await _roleRepository.FindByNormalizedNameAsync(roleName.ToUpperInvariant());
            if (role == null)
            {
                continue;
            }

            foreach (var perm in permissionNames)
            {
                var existing = await _permissionGrantRepository.FindAsync(perm, "R", role.Name, default);
                if (existing == null)
                {
                    await _permissionGrantRepository.InsertAsync(new PermissionGrant(
                        System.Guid.NewGuid(),
                        perm,
                        role.Name,
                        "R"));
                }
            }
        }
    }

    private async Task GrantContractsPermissionsAsync()
    {
        var contractsPerms = LegalTechPermissions.Contracts.All.ToList();
        var roleNames = new[] { LegalTechRoles.LegalOpsManager, LegalTechRoles.LawyerReviewer, LegalTechRoles.TenantAdmin, "admin", LegalTechRoles.HostAdmin };

        await GrantPermissionsAsync(contractsPerms, roleNames);
    }

    private async Task GrantClausesPermissionsAsync()
    {
        var clausesPerms = LegalTechPermissions.Clauses.All.ToList();
        var roleNames = new[] { LegalTechRoles.LegalOpsManager, LegalTechRoles.TenantAdmin, "admin", LegalTechRoles.HostAdmin };

        await GrantPermissionsAsync(clausesPerms, roleNames);
    }

    private async Task GrantPlaybooksPermissionsAsync()
    {
        var playbooksPerms = LegalTechPermissions.Playbooks.All.ToList();
        var roleNames = new[] { LegalTechRoles.LegalOpsManager, LegalTechRoles.TenantAdmin, "admin", LegalTechRoles.HostAdmin };

        await GrantPermissionsAsync(playbooksPerms, roleNames);
    }

    private async Task GrantReviewsPermissionsAsync()
    {
        var reviewsPerms = LegalTechPermissions.Reviews.All.ToList();
        var roleNames = new[] { LegalTechRoles.LegalOpsManager, LegalTechRoles.LawyerReviewer, LegalTechRoles.TenantAdmin, "admin", LegalTechRoles.HostAdmin };

        await GrantPermissionsAsync(reviewsPerms, roleNames);
    }

    private async Task GrantObligationsPermissionsAsync()
    {
        var obligationsPerms = LegalTechPermissions.Obligations.All.ToList();
        var roleNames = new[] { LegalTechRoles.LegalOpsManager, LegalTechRoles.TenantAdmin, "admin", LegalTechRoles.HostAdmin };

        await GrantPermissionsAsync(obligationsPerms, roleNames);
    }

    private async Task GrantRenewalsPermissionsAsync()
    {
        var renewalsPerms = LegalTechPermissions.Renewals.All.ToList();
        var roleNames = new[] { LegalTechRoles.LegalOpsManager, LegalTechRoles.TenantAdmin, "admin", LegalTechRoles.HostAdmin };

        await GrantPermissionsAsync(renewalsPerms, roleNames);
    }

    private async Task GrantReportsPermissionsAsync()
    {
        var reportsPerms = LegalTechPermissions.Reports.All.ToList();
        var roleNames = new[] { LegalTechRoles.LegalOpsManager, LegalTechRoles.LawyerReviewer, LegalTechRoles.TenantAdmin, "admin", LegalTechRoles.HostAdmin };

        await GrantPermissionsAsync(reportsPerms, roleNames);
    }

    private async Task GrantDashboardsPermissionsAsync()
    {
        var dashboardsPerms = LegalTechPermissions.Dashboards.All.ToList();
        var roleNames = new[] { LegalTechRoles.LegalOpsManager, LegalTechRoles.TenantAdmin, "admin", LegalTechRoles.HostAdmin, LegalTechRoles.Auditor };

        await GrantPermissionsAsync(dashboardsPerms, roleNames);
    }

    private async Task GrantFilesPermissionsAsync()
    {
        var filesPerms = LegalTechPermissions.Files.All.ToList();
        var roleNames = new[] { LegalTechRoles.LegalOpsManager, LegalTechRoles.LawyerReviewer, LegalTechRoles.TenantAdmin, "admin", LegalTechRoles.HostAdmin };

        await GrantPermissionsAsync(filesPerms, roleNames);
    }

    private async Task GrantAdministrationPermissionsAsync()
    {
        var adminPerms = LegalTechPermissions.Administration.All.ToList();
        var roleNames = new[] { LegalTechRoles.HostAdmin, "admin" };

        await GrantPermissionsAsync(adminPerms, roleNames);
    }
}
