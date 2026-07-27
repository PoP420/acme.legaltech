using System.Collections.Generic;

namespace Acme.LegalTech.Permissions;

public static class LegalTechRoles
{
    public const string HostAdmin = "host-admin";
    public const string TenantAdmin = "tenant-admin";
    public const string LegalOpsManager = "legal-ops-manager";
    public const string LawyerReviewer = "lawyer-reviewer";
    public const string Auditor = "auditor";

    public static readonly IReadOnlyList<string> All =
    [
        HostAdmin,
        TenantAdmin,
        LegalOpsManager,
        LawyerReviewer,
        Auditor
    ];
}
