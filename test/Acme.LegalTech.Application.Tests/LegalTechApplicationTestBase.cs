using Volo.Abp.Modularity;

namespace Acme.LegalTech;

public abstract class LegalTechApplicationTestBase<TStartupModule> : LegalTechTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
