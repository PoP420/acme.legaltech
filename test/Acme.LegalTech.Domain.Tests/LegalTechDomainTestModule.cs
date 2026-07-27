using Volo.Abp.Modularity;

namespace Acme.LegalTech;

[DependsOn(
    typeof(LegalTechDomainModule),
    typeof(LegalTechTestBaseModule)
)]
public class LegalTechDomainTestModule : AbpModule
{

}
