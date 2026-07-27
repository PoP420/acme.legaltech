using Acme.LegalTech.Contracts;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Acme.LegalTech.EntityFrameworkCore;

[Collection(LegalTechTestConsts.CollectionDefinitionName)]
public class LegalTechModule02ContractIntakeMigrationTests : LegalTechEntityFrameworkCoreTestBase
{
    [Fact]
    public void ContractDocumentVersions_Table_Should_Have_Unique_Index_On_ContractId_And_IsLatest()
    {
        var dbContext = GetRequiredService<LegalTechDbContext>();
        var entityType = dbContext.Model.FindEntityType(typeof(ContractDocumentVersion));

        entityType.ShouldNotBeNull();
        entityType.GetTableName().ShouldBe("AppContractDocumentVersions");

        var uniqueIndex = entityType.GetIndexes()
            .FirstOrDefault(i => i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual(new[] { nameof(ContractDocumentVersion.ContractId), nameof(ContractDocumentVersion.IsLatest) }));

        uniqueIndex.ShouldNotBeNull();
    }

    [Fact]
    public void Module02_Tables_Should_Be_Configured()
    {
        var dbContext = GetRequiredService<LegalTechDbContext>();

        var contractEntity = dbContext.Model.FindEntityType(typeof(Contract));
        contractEntity.ShouldNotBeNull();
        contractEntity.GetTableName().ShouldBe("AppContracts");

        var documentEntity = dbContext.Model.FindEntityType(typeof(ContractDocumentVersion));
        documentEntity.ShouldNotBeNull();
        documentEntity.GetTableName().ShouldBe("AppContractDocumentVersions");

        var counterpartyEntity = dbContext.Model.FindEntityType(typeof(CounterpartyReference));
        counterpartyEntity.ShouldNotBeNull();
        counterpartyEntity.GetTableName().ShouldBe("AppCounterpartyReferences");

        var tagEntity = dbContext.Model.FindEntityType(typeof(ContractTag));
        tagEntity.ShouldNotBeNull();
        tagEntity.GetTableName().ShouldBe("AppContractTags");
    }

    [Fact]
    public async Task Module02_Tables_Should_Accept_Rows()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var dbContext = GetRequiredService<LegalTechDbContext>();

            var contract = new Contract(Guid.NewGuid(), "Module02 Agreement", "Acme Corp")
            {
                Category = "Vendor",
                RiskBaseline = "Medium"
            };
            dbContext.Contracts.Add(contract);
            await dbContext.SaveChangesAsync();

            dbContext.Set<CounterpartyReference>().Add(new CounterpartyReference(Guid.NewGuid(), null, contract.Id, "Beta Inc"));
            dbContext.Set<ContractTag>().Add(new ContractTag(Guid.NewGuid(), null, contract.Id, "NDA"));
            dbContext.Set<ContractDocumentVersion>().Add(new ContractDocumentVersion(Guid.NewGuid(), null, contract.Id, 1, "blob1", "a.pdf", "application/pdf", 1024, null));
            await dbContext.SaveChangesAsync();

            (await dbContext.Contracts.CountAsync()).ShouldBeGreaterThan(0);
            (await dbContext.Set<CounterpartyReference>().CountAsync()).ShouldBeGreaterThan(0);
            (await dbContext.Set<ContractTag>().CountAsync()).ShouldBeGreaterThan(0);
            (await dbContext.Set<ContractDocumentVersion>().CountAsync()).ShouldBeGreaterThan(0);
        });
    }
}
