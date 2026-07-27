using Volo.Abp.Settings;

namespace Acme.LegalTech.Settings;

public class LegalTechSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(new SettingDefinition(LegalTechConsts.MigrationModelHashSettingName, isVisibleToClients: false));
    }
}
