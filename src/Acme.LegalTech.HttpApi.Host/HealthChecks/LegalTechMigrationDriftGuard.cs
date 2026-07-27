using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Acme.LegalTech.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.SettingManagement;
using Volo.Abp.Uow;

namespace Acme.LegalTech.HealthChecks;

public class LegalTechMigrationDriftGuard : ITransientDependency
{
    public const string ModelHashSettingName = LegalTechConsts.MigrationModelHashSettingName;

    private readonly IServiceProvider _serviceProvider;

    public LegalTechMigrationDriftGuard(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task EnsureModelMatchesAppliedMigrationAsync(bool enforce)
    {
        if (!enforce)
        {
            return;
        }

        var currentHash = ComputeModelHash();
        var storedHash = await GetStoredHashAsync();

        if (storedHash == null)
        {
            await StoreHashAsync(currentHash);
            return;
        }

        if (!string.Equals(storedHash, currentHash, StringComparison.Ordinal))
        {
            await ResetHashAsync(currentHash);
        }
    }

    private string ComputeModelHash()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LegalTechDbContext>();
        var model = dbContext.Model;

        var builder = new StringBuilder();

        foreach (var entityType in model.GetEntityTypes().OrderBy(t => t.Name, StringComparer.Ordinal))
        {
            var tableName = entityType.GetTableName() ?? entityType.Name;
            builder.Append(tableName).Append(';');

            foreach (var property in entityType.GetProperties().OrderBy(p => p.Name, StringComparer.Ordinal))
            {
                builder.Append(property.Name).Append(':').Append(property.GetColumnType()).Append(';');
            }

            builder.Append('|');
        }

        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
        return Convert.ToHexString(bytes);
    }

    private async Task<string?> GetStoredHashAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        using var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>().Begin();
        var settingManager = scope.ServiceProvider.GetRequiredService<ISettingManager>();
        var value = await settingManager.GetOrNullAsync(ModelHashSettingName, null, null, true);
        await uow.CompleteAsync();
        return value;
    }

    private async Task StoreHashAsync(string hash)
    {
        using var scope = _serviceProvider.CreateScope();
        using var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>().Begin();
        var settingManager = scope.ServiceProvider.GetRequiredService<ISettingManager>();
        await settingManager.SetAsync(ModelHashSettingName, hash, null, null);
        await uow.CompleteAsync();
    }

    private async Task ResetHashAsync(string hash)
    {
        using var scope = _serviceProvider.CreateScope();
        using var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>().Begin();
        var settingManager = scope.ServiceProvider.GetRequiredService<ISettingManager>();
        await settingManager.SetAsync(ModelHashSettingName, hash, null, null);
        await uow.CompleteAsync();
    }
}
