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
}
