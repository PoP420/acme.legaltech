using System.Collections.Generic;
using Acme.LegalTech.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace Acme.LegalTech.Permissions;

public class LegalTechPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var keys = new List<string>();

        foreach (var groupName in LegalTechPermissions.Groups.All)
        {
            var displayNameKey = "Permission:" + groupName.Substring(LegalTechPermissions.GroupName.Length + 1);
            context.AddGroup(groupName, L(displayNameKey));
        }

        var contractsGroup = context.GetGroup(LegalTechPermissions.Groups.Contracts);
        var defaultPerm = contractsGroup.AddPermission(LegalTechPermissions.Contracts.Default, L("Permission:Contracts"));
        keys.Add(defaultPerm.Name);
        var create = contractsGroup.AddPermission(LegalTechPermissions.Contracts.Create, L("Permission:Contracts.Create"));
        keys.Add(create.Name);
        var edit = contractsGroup.AddPermission(LegalTechPermissions.Contracts.Edit, L("Permission:Contracts.Edit"));
        keys.Add(edit.Name);
        var changeStatus = contractsGroup.AddPermission(LegalTechPermissions.Contracts.ChangeStatus, L("Permission:Contracts.ChangeStatus"));
        keys.Add(changeStatus.Name);
        var attachDocument = contractsGroup.AddPermission(LegalTechPermissions.Contracts.AttachDocument, L("Permission:Contracts.AttachDocument"));
        keys.Add(attachDocument.Name);

        LegalTechPermissionGuard.ThrowIfDuplicateKeys(keys);
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<LegalTechResource>(name);
    }
}
