using Xunit;

namespace Acme.LegalTech.EntityFrameworkCore;

[CollectionDefinition(LegalTechTestConsts.CollectionDefinitionName)]
public class LegalTechEntityFrameworkCoreCollection : ICollectionFixture<LegalTechEntityFrameworkCoreFixture>
{

}
