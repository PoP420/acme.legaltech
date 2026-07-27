using System.Linq;
using System.Threading.Tasks;
using Acme.LegalTech.Contracts;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Acme.LegalTech.EntityFrameworkCore;

[Collection(LegalTechTestConsts.CollectionDefinitionName)]
public class LegalTechModule01FoundationMigrationTests : LegalTechEntityFrameworkCoreTestBase
{
    [Fact]
    public void Contracts_Table_Should_Be_Configured()
    {
        var dbContext = GetRequiredService<LegalTechDbContext>();
        var entityType = dbContext.Model.FindEntityType(typeof(Contract));

        entityType.ShouldNotBeNull();
        entityType.GetTableName().ShouldBe("AppContracts");

        var columnNames = entityType.GetProperties().Select(p => p.GetColumnName()).ToList();
        columnNames.ShouldContain("Title");
        columnNames.ShouldContain("CounterpartyName");
        columnNames.ShouldContain("Status");
        columnNames.ShouldContain("TenantId");
    }

    [Fact]
    public async Task Contracts_Table_Should_Accept_Rows()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var dbContext = GetRequiredService<LegalTechDbContext>();
            dbContext.Contracts.Add(new Contract(System.Guid.NewGuid(), "Sample Agreement", "Acme Corp"));
            await dbContext.SaveChangesAsync();

            (await dbContext.Contracts.CountAsync()).ShouldBeGreaterThan(0);
        });
    }
}
