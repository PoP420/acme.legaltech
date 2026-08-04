using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acme.LegalTech.Contracts;
using Acme.LegalTech.Common;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Shouldly;
using Xunit;

namespace Acme.LegalTech.EntityFrameworkCore;

[Collection(LegalTechTestConsts.CollectionDefinitionName)]
public class LegalTechModule04GovComplianceTests : LegalTechEntityFrameworkCoreTestBase
{
    [Fact]
    public async Task Tier_Computation_Should_Resolve_By_Value_Boundary()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var tierRepo = GetRequiredService<IRepository<GovernmentApprovalTier, Guid>>();
            await tierRepo.DeleteAsync(t => true);

            await tierRepo.InsertManyAsync(new[]
            {
                new GovernmentApprovalTier(Guid.NewGuid(), null, 0, 499999m, "Agency Head", false, false, 5m),
                new GovernmentApprovalTier(Guid.NewGuid(), null, 500000m, 999999m, "Head of Procuring Entity", false, false, 10m),
                new GovernmentApprovalTier(Guid.NewGuid(), null, 1000000m, null, "President", true, true, 10m)
            });

            var contractRepo = GetRequiredService<IRepository<Contract, Guid>>();
            var contract = new Contract(Guid.NewGuid(), "Gov Contract", "Agency", contractValue: 750000m);
            await contractRepo.InsertAsync(contract);

            var tiers = await tierRepo.GetListAsync();
            var tier = contract.ComputeApprovingAuthority(750000m, tiers);
            tier.AuthorityTitle.ShouldBe("Head of Procuring Entity");
        });
    }

    [Fact]
    public async Task Variation_At_5_Percent_Should_Succeed()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var tierRepo = GetRequiredService<IRepository<GovernmentApprovalTier, Guid>>();
            await tierRepo.DeleteAsync(t => true);

            await tierRepo.InsertAsync(new GovernmentApprovalTier(Guid.NewGuid(), null, 1000000m, null, "Agency Head", false, false, 5m));

            var contractRepo = GetRequiredService<IRepository<Contract, Guid>>();
            var contract = new Contract(Guid.NewGuid(), "Gov Contract", "Agency", contractValue: 2000000m);
            await contractRepo.InsertAsync(contract);

            var tiers = await tierRepo.GetListAsync();
            var vo = new VariationOrder(Guid.NewGuid(), null, contract.Id, "Test", 100000m, 100000m);
            contract.AddVariationOrder(vo, tiers);
            vo.CumulativeAmount.ShouldBe(100000m);
        });
    }

    [Fact]
    public async Task Variation_At_7_Percent_Should_Succeed_With_HoPE_Tier()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var tierRepo = GetRequiredService<IRepository<GovernmentApprovalTier, Guid>>();
            await tierRepo.DeleteAsync(t => true);

            await tierRepo.InsertAsync(new GovernmentApprovalTier(Guid.NewGuid(), null, 1000000m, null, "Head of Procuring Entity", false, false, 10m));

            var contractRepo = GetRequiredService<IRepository<Contract, Guid>>();
            var contract = new Contract(Guid.NewGuid(), "Gov Contract", "Agency", contractValue: 2000000m);
            await contractRepo.InsertAsync(contract);

            var tiers = await tierRepo.GetListAsync();
            var vo = new VariationOrder(Guid.NewGuid(), null, contract.Id, "Test", 140000m, 140000m);
            contract.AddVariationOrder(vo, tiers);
            vo.CumulativeAmount.ShouldBe(140000m);
        });
    }

    [Fact]
    public async Task Variation_Above_10_Percent_Should_Be_Rejected()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var tierRepo = GetRequiredService<IRepository<GovernmentApprovalTier, Guid>>();
            await tierRepo.DeleteAsync(t => true);

            await tierRepo.InsertAsync(new GovernmentApprovalTier(Guid.NewGuid(), null, 1000000m, null, "Agency Head", false, false, 10m));

            var contractRepo = GetRequiredService<IRepository<Contract, Guid>>();
            var contract = new Contract(Guid.NewGuid(), "Gov Contract", "Agency", contractValue: 2000000m);
            await contractRepo.InsertAsync(contract);

            var tiers = await tierRepo.GetListAsync();
            var vo = new VariationOrder(Guid.NewGuid(), null, contract.Id, "Test", 250000m, 250000m);

            Should.Throw<BusinessException>(() => contract.AddVariationOrder(vo, tiers))
                .Code.ShouldBe("LegalTech:Contract:ApprovedVariationLimitExceeded");
        });
    }

    [Fact]
    public async Task AuthorizedSignatory_Uniqueness_Should_Be_Enforced()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var contractRepo = GetRequiredService<IRepository<Contract, Guid>>();
            var contract = new Contract(Guid.NewGuid(), "Gov Contract", "Agency");
            await contractRepo.InsertAsync(contract);

            var signatory1 = new ContractSignatory(Guid.NewGuid(), null, contract.Id, GovernmentSignatoryRole.AuthorizedSignatory, DocumentPartyType.Individual, "P1", "Agency", "Head", 1);
            contract.AddSignatory(signatory1);

            var signatory2 = new ContractSignatory(Guid.NewGuid(), null, contract.Id, GovernmentSignatoryRole.AuthorizedSignatory, DocumentPartyType.Individual, "P2", "Agency", "Head", 2);
            Should.Throw<BusinessException>(() => contract.AddSignatory(signatory2))
                .Code.ShouldBe("LegalTech:Contract:GovSignatoryExists");
        });
    }

    [Fact]
    public async Task AddVariationOrder_On_Null_ContractValue_Should_Throw()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var contractRepo = GetRequiredService<IRepository<Contract, Guid>>();
            var contract = new Contract(Guid.NewGuid(), "Gov Contract", "Agency");
            await contractRepo.InsertAsync(contract);

            var vo = new VariationOrder(Guid.NewGuid(), null, contract.Id, "Test", 1000m, 1000m);

            Should.Throw<BusinessException>(() => contract.AddVariationOrder(vo, new List<GovernmentApprovalTier>()))
                .Code.ShouldBe("LegalTech:Contract:ValueRequiredForVariation");
        });
    }
}
