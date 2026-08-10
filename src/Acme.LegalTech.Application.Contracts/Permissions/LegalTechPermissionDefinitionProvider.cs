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

        RegisterContractsPermissions(context, keys);
        RegisterClausesPermissions(context, keys);
        RegisterPlaybooksPermissions(context, keys);
        RegisterReviewsPermissions(context, keys);
        RegisterObligationsPermissions(context, keys);
        RegisterRenewalsPermissions(context, keys);
        RegisterReportsPermissions(context, keys);
        RegisterDashboardsPermissions(context, keys);
        RegisterFilesPermissions(context, keys);
        RegisterAdministrationPermissions(context, keys);
        RegisterAIAssistPermissions(context, keys);

        LegalTechPermissionGuard.ThrowIfDuplicateKeys(keys);
    }

    private void RegisterContractsPermissions(IPermissionDefinitionContext context, List<string> keys)
    {
        var group = context.GetGroup(LegalTechPermissions.Groups.Contracts);
        var defaultPerm = group.AddPermission(LegalTechPermissions.Contracts.Default, L("Permission:Contracts"));
        keys.Add(defaultPerm.Name);
        var create = group.AddPermission(LegalTechPermissions.Contracts.Create, L("Permission:Contracts.Create"));
        keys.Add(create.Name);
        var edit = group.AddPermission(LegalTechPermissions.Contracts.Edit, L("Permission:Contracts.Edit"));
        keys.Add(edit.Name);
        var changeStatus = group.AddPermission(LegalTechPermissions.Contracts.ChangeStatus, L("Permission:Contracts.ChangeStatus"));
        keys.Add(changeStatus.Name);
        var attachDocument = group.AddPermission(LegalTechPermissions.Contracts.AttachDocument, L("Permission:Contracts.AttachDocument"));
        keys.Add(attachDocument.Name);
        var manageSignatories = group.AddPermission(LegalTechPermissions.Contracts.ManageSignatories, L("Permission:Contracts.ManageSignatories"));
        keys.Add(manageSignatories.Name);
        var amend = group.AddPermission(LegalTechPermissions.Contracts.Amend, L("Permission:Contracts.Amend"));
        keys.Add(amend.Name);
        var terminate = group.AddPermission(LegalTechPermissions.Contracts.Terminate, L("Permission:Contracts.Terminate"));
        keys.Add(terminate.Name);
        var viewGovFields = group.AddPermission(LegalTechPermissions.Contracts.ViewGovFields, L("Permission:Contracts.ViewGovFields"));
        keys.Add(viewGovFields.Name);
    }

    private void RegisterClausesPermissions(IPermissionDefinitionContext context, List<string> keys)
    {
        var group = context.GetGroup(LegalTechPermissions.Groups.Clauses);
        var defaultPerm = group.AddPermission(LegalTechPermissions.Clauses.Default, L("Permission:Clauses"));
        keys.Add(defaultPerm.Name);
        var manage = group.AddPermission(LegalTechPermissions.Clauses.Manage, L("Permission:Clauses.Manage"));
        keys.Add(manage.Name);
    }

    private void RegisterPlaybooksPermissions(IPermissionDefinitionContext context, List<string> keys)
    {
        var group = context.GetGroup(LegalTechPermissions.Groups.Clauses);
        var defaultPerm = group.AddPermission(LegalTechPermissions.Playbooks.Default, L("Permission:Playbooks"));
        keys.Add(defaultPerm.Name);
        var manage = group.AddPermission(LegalTechPermissions.Playbooks.Manage, L("Permission:Playbooks.Manage"));
        keys.Add(manage.Name);
        var evaluate = group.AddPermission(LegalTechPermissions.Playbooks.Evaluate, L("Permission:Playbooks.Evaluate"));
        keys.Add(evaluate.Name);
    }

    private void RegisterReviewsPermissions(IPermissionDefinitionContext context, List<string> keys)
    {
        var group = context.GetGroup(LegalTechPermissions.Groups.Reviews);
        var defaultPerm = group.AddPermission(LegalTechPermissions.Reviews.Default, L("Permission:Reviews"));
        keys.Add(defaultPerm.Name);
        var assign = group.AddPermission(LegalTechPermissions.Reviews.Assign, L("Permission:Reviews.Assign"));
        keys.Add(assign.Name);
        var decide = group.AddPermission(LegalTechPermissions.Reviews.Decide, L("Permission:Reviews.Decide"));
        keys.Add(decide.Name);
        var escalate = group.AddPermission(LegalTechPermissions.Reviews.Escalate, L("Permission:Reviews.Escalate"));
        keys.Add(escalate.Name);
        var auditView = group.AddPermission(LegalTechPermissions.Reviews.AuditView, L("Permission:Reviews.AuditView"));
        keys.Add(auditView.Name);
    }

    private void RegisterObligationsPermissions(IPermissionDefinitionContext context, List<string> keys)
    {
        var group = context.GetGroup(LegalTechPermissions.Groups.Obligations);
        var defaultPerm = group.AddPermission(LegalTechPermissions.Obligations.Default, L("Permission:Obligations"));
        keys.Add(defaultPerm.Name);
        var manage = group.AddPermission(LegalTechPermissions.Obligations.Manage, L("Permission:Obligations.Manage"));
        keys.Add(manage.Name);
        var complete = group.AddPermission(LegalTechPermissions.Obligations.Complete, L("Permission:Obligations.Complete"));
        keys.Add(complete.Name);
    }

    private void RegisterRenewalsPermissions(IPermissionDefinitionContext context, List<string> keys)
    {
        var group = context.GetGroup(LegalTechPermissions.Groups.Obligations);
        var defaultPerm = group.AddPermission(LegalTechPermissions.Renewals.Default, L("Permission:Renewals"));
        keys.Add(defaultPerm.Name);
        var manage = group.AddPermission(LegalTechPermissions.Renewals.Manage, L("Permission:Renewals.Manage"));
        keys.Add(manage.Name);
    }

    private void RegisterReportsPermissions(IPermissionDefinitionContext context, List<string> keys)
    {
        var group = context.GetGroup(LegalTechPermissions.Groups.Reports);
        var defaultPerm = group.AddPermission(LegalTechPermissions.Reports.Default, L("Permission:Reports"));
        keys.Add(defaultPerm.Name);
        var export = group.AddPermission(LegalTechPermissions.Reports.Export, L("Permission:Reports.Export"));
        keys.Add(export.Name);
    }

    private void RegisterDashboardsPermissions(IPermissionDefinitionContext context, List<string> keys)
    {
        var group = context.GetGroup(LegalTechPermissions.Groups.Reports);
        var defaultPerm = group.AddPermission(LegalTechPermissions.Dashboards.Default, L("Permission:Dashboards"));
        keys.Add(defaultPerm.Name);
        var viewRisk = group.AddPermission(LegalTechPermissions.Dashboards.ViewRisk, L("Permission:Dashboards.ViewRisk"));
        keys.Add(viewRisk.Name);
    }

    private void RegisterFilesPermissions(IPermissionDefinitionContext context, List<string> keys)
    {
        var group = context.GetGroup(LegalTechPermissions.Groups.Files);
        var defaultPerm = group.AddPermission(LegalTechPermissions.Files.Default, L("Permission:Files"));
        keys.Add(defaultPerm.Name);
        var upload = group.AddPermission(LegalTechPermissions.Files.Upload, L("Permission:Files.Upload"));
        keys.Add(upload.Name);
        var download = group.AddPermission(LegalTechPermissions.Files.Download, L("Permission:Files.Download"));
        keys.Add(download.Name);
        var delete = group.AddPermission(LegalTechPermissions.Files.Delete, L("Permission:Files.Delete"));
        keys.Add(delete.Name);
        var manageAll = group.AddPermission(LegalTechPermissions.Files.ManageAll, L("Permission:Files.ManageAll"));
        keys.Add(manageAll.Name);
    }

    private void RegisterAdministrationPermissions(IPermissionDefinitionContext context, List<string> keys)
    {
        var group = context.GetGroup(LegalTechPermissions.Groups.Administration);
        var defaultPerm = group.AddPermission(LegalTechPermissions.Administration.Default, L("Permission:Administration"));
        keys.Add(defaultPerm.Name);
        var tenants = group.AddPermission(LegalTechPermissions.Administration.Tenants, L("Permission:Administration.Tenants"));
        keys.Add(tenants.Name);
        var planManagement = group.AddPermission(LegalTechPermissions.Administration.PlanManagement, L("Permission:Administration.PlanManagement"));
        keys.Add(planManagement.Name);
    }

    private void RegisterAIAssistPermissions(IPermissionDefinitionContext context, List<string> keys)
    {
        var group = context.GetGroup(LegalTechPermissions.Groups.AIAssist);
        var defaultPerm = group.AddPermission(LegalTechPermissions.AIAssist.Default, L("Permission:AIAssist"));
        keys.Add(defaultPerm.Name);
        var runJobs = group.AddPermission(LegalTechPermissions.AIAssist.RunJobs, L("Permission:AIAssist.RunJobs"));
        keys.Add(runJobs.Name);
        var reviewSuggestions = group.AddPermission(LegalTechPermissions.AIAssist.ReviewSuggestions, L("Permission:AIAssist.ReviewSuggestions"));
        keys.Add(reviewSuggestions.Name);
        var configureProviders = group.AddPermission(LegalTechPermissions.AIAssist.ConfigureProviders, L("Permission:AIAssist.ConfigureProviders"));
        keys.Add(configureProviders.Name);
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<LegalTechResource>(name);
    }
}
