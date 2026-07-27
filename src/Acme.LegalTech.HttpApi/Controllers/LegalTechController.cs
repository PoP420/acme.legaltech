using Acme.LegalTech.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Acme.LegalTech.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class LegalTechController : AbpControllerBase
{
    protected LegalTechController()
    {
        LocalizationResource = typeof(LegalTechResource);
    }
}
