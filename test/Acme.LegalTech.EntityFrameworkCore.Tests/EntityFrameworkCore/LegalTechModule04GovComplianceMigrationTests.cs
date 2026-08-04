using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Acme.LegalTech.EntityFrameworkCore;

[Collection(LegalTechTestConsts.CollectionDefinitionName)]
public class LegalTechModule04GovComplianceMigrationTests : LegalTechEntityFrameworkCoreTestBase
{
    [Fact]
    public void Module04_Tables_Should_Be_Configured()
    {
        var dbContext = GetRequiredService<LegalTechDbContext>();

        var signatoryEntity = dbContext.Model.FindEntityType(typeof(Acme.LegalTech.Contracts.ContractSignatory));
        signatoryEntity.ShouldNotBeNull();
        signatoryEntity.GetTableName().ShouldBe("AppContractSignatories");

        var variationEntity = dbContext.Model.FindEntityType(typeof(Acme.LegalTech.Contracts.VariationOrder));
        variationEntity.ShouldNotBeNull();
        variationEntity.GetTableName().ShouldBe("AppVariationOrders");

        var tierEntity = dbContext.Model.FindEntityType(typeof(Acme.LegalTech.Contracts.GovernmentApprovalTier));
        tierEntity.ShouldNotBeNull();
        tierEntity.GetTableName().ShouldBe("AppGovernmentApprovalTiers");
    }

    [Fact]
    public void Module04_Contract_Columns_Should_Exist()
    {
        var dbContext = GetRequiredService<LegalTechDbContext>();
        var contractEntity = dbContext.Model.FindEntityType(typeof(Acme.LegalTech.Contracts.Contract));
        contractEntity.ShouldNotBeNull();

        contractEntity.FindProperty("DocumentNumber").ShouldNotBeNull();
        contractEntity.FindProperty("DocumentSeries").ShouldNotBeNull();
        contractEntity.FindProperty("DocumentYear").ShouldNotBeNull();
        contractEntity.FindProperty("Classification").ShouldNotBeNull();
        contractEntity.FindProperty("RetentionUntil").ShouldNotBeNull();
        contractEntity.FindProperty("ContractValue").ShouldNotBeNull();
        contractEntity.FindProperty("LastApprovalAuthorityTitle").ShouldNotBeNull();
        contractEntity.FindProperty("LastApprovalRequiresNeda").ShouldNotBeNull();
        contractEntity.FindProperty("LastApprovalRequiresPresident").ShouldNotBeNull();
    }
}
