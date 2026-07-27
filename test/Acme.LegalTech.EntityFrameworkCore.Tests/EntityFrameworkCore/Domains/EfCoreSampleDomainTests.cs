using Acme.LegalTech.Samples;
using Xunit;

namespace Acme.LegalTech.EntityFrameworkCore.Domains;

[Collection(LegalTechTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<LegalTechEntityFrameworkCoreTestModule>
{

}
