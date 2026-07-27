using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acme.LegalTech.Permissions;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Xunit;

namespace Acme.LegalTech.EntityFrameworkCore;

public class LegalTechPermissionsTests : LegalTechEntityFrameworkCoreTestBase
{
    private readonly IPermissionDefinitionManager _permissionDefinitionManager;

    public LegalTechPermissionsTests()
    {
        _permissionDefinitionManager = GetRequiredService<IPermissionDefinitionManager>();
    }

    [Fact]
    public async Task Should_Register_All_Seven_Module_Groups()
    {
        var groups = await _permissionDefinitionManager.GetGroupsAsync();
        var groupNames = groups.Select(g => g.Name).ToList();

        foreach (var expected in LegalTechPermissions.Groups.All)
        {
            groupNames.ShouldContain(expected);
        }

        LegalTechPermissions.Groups.All.Distinct().Count().ShouldBe(7);
    }

    [Fact]
    public void Duplicate_Key_Guard_Should_Throw_On_Conflict()
    {
        var keys = new List<string>
        {
            LegalTechPermissions.Groups.Contracts,
            LegalTechPermissions.Groups.Contracts
        };

        Should.Throw<BusinessException>(() => LegalTechPermissionGuard.ThrowIfDuplicateKeys(keys))
            .Code.ShouldBe(LegalTechPermissionGuard.ErrorCode);
    }

    [Fact]
    public void Duplicate_Key_Guard_Should_Not_Throw_For_Unique_Keys()
    {
        var keys = new List<string>
        {
            LegalTechPermissions.Groups.Contracts,
            LegalTechPermissions.Groups.Clauses
        };

        Should.NotThrow(() => LegalTechPermissionGuard.ThrowIfDuplicateKeys(keys));
    }
}
