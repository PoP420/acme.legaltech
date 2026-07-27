using System;
using System.Linq;
using System.Threading.Tasks;
using Acme.LegalTech.Permissions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;

namespace Acme.LegalTech.Permissions;

public class LegalTechPermissionHealthContributor : ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public LegalTechPermissionHealthContributor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task LogAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<LegalTechPermissionHealthContributor>>();
        var permissionManager = scope.ServiceProvider.GetRequiredService<IPermissionDefinitionManager>();

        var groups = await permissionManager.GetGroupsAsync();
        var permissions = await permissionManager.GetPermissionsAsync();

        logger.LogInformation(
            "LegalTech startup diagnostics: {GroupCount} permission groups, {PermissionCount} permissions, {RoleCount} baseline roles.",
            groups.Count,
            permissions.Count,
            LegalTechRoles.All.Count);
    }
}
