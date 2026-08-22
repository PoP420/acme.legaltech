using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Acme.LegalTech.HttpApi.Tests;

public class ApiTestFixture : IAsyncLifetime
{
    public ApiClient AdminClient { get; private set; } = default!;

    public Guid ContractId { get; private set; }

    public bool CreatedContract { get; private set; }

    public async Task InitializeAsync()
    {
        AdminClient = new ApiClient(TestConfiguration.BaseUrl, TestConfiguration.IgnoreSsl);
        await AdminClient.LoginAsync(TestConfiguration.AdminUser, TestConfiguration.AdminPassword);

        ContractId = await GetOrCreateContractIdAsync();
    }

    public async Task DisposeAsync()
    {
        if (CreatedContract && ContractId != Guid.Empty)
        {
            try
            {
                await AdminClient.DeleteAsync($"api/app/contract/{ContractId}");
            }
            catch
            {
                // best-effort cleanup; ignore failures so disposal never masks test results
            }
        }
    }

    private async Task<Guid> GetOrCreateContractIdAsync()
    {
        var list = await AdminClient.GetAsync("api/app/contract?maxResultCount=1");
        if (list.IsSuccessStatusCode)
        {
            var body = await list.Content.ReadFromJsonAsync<JsonElement>(ApiClient.JsonOptions);
            if (body.TryGetProperty("items", out var items)
                && items.ValueKind == JsonValueKind.Array
                && items.GetArrayLength() > 0)
            {
                return items[0].GetProperty("id").GetGuid();
            }
        }

        var create = await AdminClient.PostAsJsonAsync("api/app/contract", new
        {
            title = "API Test Contract",
            counterpartyName = "API Test Counterparty"
        });

        create.EnsureSuccessStatusCode();
        var created = await create.Content.ReadFromJsonAsync<JsonElement>(ApiClient.JsonOptions);
        CreatedContract = true;
        return created.GetProperty("id").GetGuid();
    }
}
