using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Acme.LegalTech.Contracts;
using Acme.LegalTech.Common;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Acme.LegalTech;

public class GovernmentApprovalTierDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<GovernmentApprovalTier, Guid> _tierRepository;

    public GovernmentApprovalTierDataSeedContributor(IRepository<GovernmentApprovalTier, Guid> tierRepository)
    {
        _tierRepository = tierRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (await _tierRepository.GetCountAsync() > 0)
        {
            return;
        }

        var tenantId = context.TenantId;
        var tiers = new List<GovernmentApprovalTier>
        {
            new GovernmentApprovalTier(
                Guid.NewGuid(),
                tenantId,
                0,
                499999,
                "Agency Head",
                requiresNedaReview: false,
                requiresPresident: false,
                allowableVariationPercent: 5m),
            new GovernmentApprovalTier(
                Guid.NewGuid(),
                tenantId,
                500000,
                299999999,
                "Head of Procuring Entity",
                requiresNedaReview: false,
                requiresPresident: false,
                allowableVariationPercent: 10m),
            new GovernmentApprovalTier(
                Guid.NewGuid(),
                tenantId,
                300000000,
                3999999999,
                "NEDA Review",
                requiresNedaReview: true,
                requiresPresident: false,
                allowableVariationPercent: 10m),
            new GovernmentApprovalTier(
                Guid.NewGuid(),
                tenantId,
                4000000000,
                null,
                "President",
                requiresNedaReview: true,
                requiresPresident: true,
                allowableVariationPercent: 10m)
        };

        await _tierRepository.InsertManyAsync(tiers);
    }
}
