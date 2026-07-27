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

    private async Task GrantContractsPermissionsAsync()
    {
        var contractsPerms = LegalTechPermissions.Contracts.All.ToList();
        var roleNames = new[] { LegalTechRoles.LegalOpsManager, LegalTechRoles.LawyerReviewer, LegalTechRoles.TenantAdmin, "admin" };

        foreach (var roleName in roleNames)
        {
            var role = await _roleRepository.FindByNormalizedNameAsync(roleName.ToUpperInvariant());
            if (role == null)
            {
                continue;
            }

            foreach (var perm in contractsPerms)
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
}
