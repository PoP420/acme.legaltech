using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acme.LegalTech.Permissions;
using Shouldly;
using Volo.Abp.Authorization.Permissions;
using Xunit;

namespace Acme.LegalTech.EntityFrameworkCore;

public class FoundationScenarios : LegalTechEntityFrameworkCoreTestBase
{
    private readonly IPermissionDefinitionManager _permissionDefinitionManager;

    public FoundationScenarios()
    {
        _permissionDefinitionManager = GetRequiredService<IPermissionDefinitionManager>();
    }

    [Fact]
    public async Task Foundation_PermissionTree_Is_Complete_And_NonConflicting()
    {
        // Given the LegalTech module is initialized
        // When permissions are discovered
        var groups = await _permissionDefinitionManager.GetGroupsAsync();
        var permissions = await _permissionDefinitionManager.GetPermissionsAsync();

        // Then all 7 bounded-context groups exist with unique keys
        var groupNames = groups.Select(g => g.Name).ToList();
        foreach (var expected in LegalTechPermissions.Groups.All)
        {
            groupNames.ShouldContain(expected);
        }

        var permissionNames = permissions.Select(p => p.Name).ToList();

        groupNames.Distinct().Count().ShouldBe(groupNames.Count);
        permissionNames.Distinct().Count().ShouldBe(permissionNames.Count);
    }
}
