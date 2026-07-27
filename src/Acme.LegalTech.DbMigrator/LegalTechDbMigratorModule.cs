using Acme.LegalTech.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Acme.LegalTech.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(LegalTechEntityFrameworkCoreModule),
    typeof(LegalTechApplicationContractsModule)
)]
public class LegalTechDbMigratorModule : AbpModule
{
}
