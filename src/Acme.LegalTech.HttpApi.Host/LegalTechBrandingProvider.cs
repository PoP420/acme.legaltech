using Microsoft.Extensions.Localization;
using Acme.LegalTech.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Acme.LegalTech;

[Dependency(ReplaceServices = true)]
public class LegalTechBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<LegalTechResource> _localizer;

    public LegalTechBrandingProvider(IStringLocalizer<LegalTechResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
