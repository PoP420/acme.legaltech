using Acme.LegalTech.Samples;
using Xunit;

namespace Acme.LegalTech.EntityFrameworkCore.Applications;

[Collection(LegalTechTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<LegalTechEntityFrameworkCoreTestModule>
{

}
