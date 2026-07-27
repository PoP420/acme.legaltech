using Acme.LegalTech.Localization;
using Volo.Abp.Application.Services;

namespace Acme.LegalTech;

/* Inherit your application services from this class.
 */
public abstract class LegalTechAppService : ApplicationService
{
    protected LegalTechAppService()
    {
        LocalizationResource = typeof(LegalTechResource);
    }
}
