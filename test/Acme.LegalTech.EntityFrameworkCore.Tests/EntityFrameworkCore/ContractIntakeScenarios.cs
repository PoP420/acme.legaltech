using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acme.LegalTech.Common;
using Acme.LegalTech.Contracts;
using Acme.LegalTech.Permissions;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Xunit;

namespace Acme.LegalTech.EntityFrameworkCore;

public class ContractIntakeScenarios : LegalTechEntityFrameworkCoreTestBase
{
    [Fact]
    public async Task Create_Contract_With_Tags_And_Counterparties()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var dbContext = GetRequiredService<LegalTechDbContext>();
            var contract = new Contract(Guid.NewGuid(), "Test Agreement", "Acme Corp")
            {
                Category = "Vendor",
                RiskBaseline = "Low"
            };
            dbContext.Contracts.Add(contract);
            await dbContext.SaveChangesAsync();

            dbContext.Set<CounterpartyReference>().Add(new CounterpartyReference(Guid.NewGuid(), null, contract.Id, "Beta Inc"));
            dbContext.Set<ContractTag>().Add(new ContractTag(Guid.NewGuid(), null, contract.Id, "NDA"));
            await dbContext.SaveChangesAsync();

            var tags = await dbContext.Set<ContractTag>().Where(t => t.ContractId == contract.Id).ToListAsync();
            tags.Count.ShouldBe(1);
            tags[0].Name.ShouldBe("NDA");
        });
    }

    [Fact]
    public async Task Attach_Document_Version_And_Mark_Latest()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var dbContext = GetRequiredService<LegalTechDbContext>();
            var contract = new Contract(Guid.NewGuid(), "Test Agreement", "Acme Corp");
            dbContext.Contracts.Add(contract);
            await dbContext.SaveChangesAsync();

            var v1 = new ContractDocumentVersion(Guid.NewGuid(), null, contract.Id, 1, "blob1", "a.pdf", "application/pdf", 1024, null);
            dbContext.Set<ContractDocumentVersion>().Add(v1);
            await dbContext.SaveChangesAsync();

            v1.UnmarkLatest();
            dbContext.Update(v1);
            await dbContext.SaveChangesAsync();

            var v2 = new ContractDocumentVersion(Guid.NewGuid(), null, contract.Id, 2, "blob2", "b.pdf", "application/pdf", 2048, null);
            dbContext.Set<ContractDocumentVersion>().Add(v2);
            await dbContext.SaveChangesAsync();

            var versions = await dbContext.Set<ContractDocumentVersion>().Where(v => v.ContractId == contract.Id).ToListAsync();
            versions.Count.ShouldBe(2);
            versions.Single(v => v.VersionNumber == 2).IsLatest.ShouldBeTrue();
            versions.Single(v => v.VersionNumber == 1).IsLatest.ShouldBeFalse();
        });
    }

    [Fact]
    public async Task Lifecycle_Activate_Expire_Terminate()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var contract = new Contract(Guid.NewGuid(), "Test Agreement", "Acme Corp");
            contract.Activate();
            contract.Status.ShouldBe(ContractStatus.Active);

            contract.Expire();
            contract.Status.ShouldBe(ContractStatus.Expired);

            var contract2 = new Contract(Guid.NewGuid(), "Test Agreement 2", "Acme Corp");
            contract2.Activate();
            contract2.Terminate();
            contract2.Status.ShouldBe(ContractStatus.Terminated);
        });
    }

    [Fact]
    public async Task Invalid_Status_Transition_Throws()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var contract = new Contract(Guid.NewGuid(), "Test Agreement", "Acme Corp");
            Should.Throw<BusinessException>(() => contract.Expire())
                .Code.ShouldBe("LegalTech:Contract:InvalidStatusTransition");
        });
    }
}
