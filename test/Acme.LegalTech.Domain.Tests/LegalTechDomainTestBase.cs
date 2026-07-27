using Volo.Abp.Modularity;

namespace Acme.LegalTech;

/* Inherit from this class for your domain layer tests. */
public abstract class LegalTechDomainTestBase<TStartupModule> : LegalTechTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
