using Volo.Abp.Modularity;

namespace Acme.LegalTech;

[DependsOn(
    typeof(LegalTechApplicationModule),
    typeof(LegalTechDomainTestModule)
)]
public class LegalTechApplicationTestModule : AbpModule
{

}
