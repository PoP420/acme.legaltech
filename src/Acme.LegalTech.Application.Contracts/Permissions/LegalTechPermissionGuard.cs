using System;
using System.Collections.Generic;
using Volo.Abp;

namespace Acme.LegalTech.Permissions;

public static class LegalTechPermissionGuard
{
    public const string ErrorCode = "LegalTech:Permission:DuplicateKey";

    public static void ThrowIfDuplicateKeys(IEnumerable<string> keys)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var key in keys)
        {
            if (!seen.Add(key))
            {
                throw new BusinessException(ErrorCode)
                {
                    Data =
                    {
                        ["DuplicateKey"] = key
                    }
                };
            }
        }
    }
}
