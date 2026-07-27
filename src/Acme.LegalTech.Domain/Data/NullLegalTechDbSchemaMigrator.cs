using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Acme.LegalTech.Data;

/* This is used if database provider does't define
 * ILegalTechDbSchemaMigrator implementation.
 */
public class NullLegalTechDbSchemaMigrator : ILegalTechDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
