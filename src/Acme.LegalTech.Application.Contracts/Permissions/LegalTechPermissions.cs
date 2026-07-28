using System.Collections.Generic;

namespace Acme.LegalTech.Permissions;

public static class LegalTechPermissions
{
    public const string GroupName = "LegalTech";

    public static class Groups
    {
        public const string Contracts = GroupName + ".Contracts";
        public const string Clauses = GroupName + ".Clauses";
        public const string Reviews = GroupName + ".Reviews";
        public const string Obligations = GroupName + ".Obligations";
        public const string Reports = GroupName + ".Reports";
        public const string Files = GroupName + ".Files";
        public const string Administration = GroupName + ".Administration";

        public static readonly IReadOnlyList<string> All =
        [
            Contracts,
            Clauses,
            Reviews,
            Obligations,
            Reports,
            Files,
            Administration
        ];
    }

    public static class Contracts
    {
        public const string Default = Groups.Contracts;
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string ChangeStatus = Default + ".ChangeStatus";
        public const string AttachDocument = Default + ".AttachDocument";

        public static readonly IReadOnlyList<string> All =
        [
            Default,
            Create,
            Edit,
            ChangeStatus,
            AttachDocument
        ];
    }

    public static class Clauses
    {
        public const string Default = Groups.Clauses;
        public const string Manage = Default + ".Manage";

        public static readonly IReadOnlyList<string> All =
        [
            Default,
            Manage
        ];
    }

    public static class Playbooks
    {
        public const string Default = Groups.Clauses + ".Playbooks";
        public const string Manage = Default + ".Manage";
        public const string Evaluate = Default + ".Evaluate";

        public static readonly IReadOnlyList<string> All =
        [
            Default,
            Manage,
            Evaluate
        ];
    }

    public static class Reviews
    {
        public const string Default = Groups.Reviews;
        public const string Assign = Default + ".Assign";
        public const string Decide = Default + ".Decide";
        public const string Escalate = Default + ".Escalate";
        public const string AuditView = Default + ".AuditView";

        public static readonly IReadOnlyList<string> All =
        [
            Default,
            Assign,
            Decide,
            Escalate,
            AuditView
        ];
    }

    public static class Obligations
    {
        public const string Default = Groups.Obligations;
        public const string Manage = Default + ".Manage";
        public const string Complete = Default + ".Complete";

        public static readonly IReadOnlyList<string> All =
        [
            Default,
            Manage,
            Complete
        ];
    }

    public static class Renewals
    {
        public const string Default = Groups.Obligations + ".Renewals";
        public const string Manage = Default + ".Manage";

        public static readonly IReadOnlyList<string> All =
        [
            Default,
            Manage
        ];
    }

    public static class Reports
    {
        public const string Default = Groups.Reports;
        public const string Export = Default + ".Export";

        public static readonly IReadOnlyList<string> All =
        [
            Default,
            Export
        ];
    }

    public static class Dashboards
    {
        public const string Default = Groups.Reports + ".Dashboards";
        public const string ViewRisk = Default + ".ViewRisk";

        public static readonly IReadOnlyList<string> All =
        [
            Default,
            ViewRisk
        ];
    }

    public static class Files
    {
        public const string Default = Groups.Files;
        public const string Upload = Default + ".Upload";
        public const string Download = Default + ".Download";
        public const string Delete = Default + ".Delete";
        public const string ManageAll = Default + ".ManageAll";

        public static readonly IReadOnlyList<string> All =
        [
            Default,
            Upload,
            Download,
            Delete,
            ManageAll
        ];
    }

    public static class Administration
    {
        public const string Default = Groups.Administration;
        public const string Tenants = Default + ".Tenants";
        public const string PlanManagement = Default + ".PlanManagement";

        public static readonly IReadOnlyList<string> All =
        [
            Default,
            Tenants,
            PlanManagement
        ];
    }
}
