using System.Threading.Tasks;

namespace Acme.LegalTech.Data;

public interface ILegalTechDbSchemaMigrator
{
    Task MigrateAsync();
}
