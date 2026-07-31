using Acme.LegalTech.Contracts;
using Acme.LegalTech.Processing;
using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.FileSystem;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.Mapperly;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.TenantManagement;

namespace Acme.LegalTech;

[DependsOn(
    typeof(LegalTechDomainModule),
    typeof(LegalTechApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(AbpBlobStoringFileSystemModule)
    )]
public class LegalTechApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<IDocumentExtractionProvider, AzureDocumentIntelligenceExtractionProvider>();

        Configure<AbpBlobStoringOptions>(options =>
        {
            options.Containers.Configure<ContractsBlobContainer>(container =>
            {
                container.UseFileSystem(fileSystem =>
                {
                    fileSystem.BasePath = "C:\\BlobStorage\\contracts";
                });
            });
        });
    }
}
