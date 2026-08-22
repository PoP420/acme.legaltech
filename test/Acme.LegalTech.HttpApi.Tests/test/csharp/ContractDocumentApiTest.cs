using System;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Acme.LegalTech.HttpApi.Tests;

public class ContractDocumentApiTest(ApiTestFixture fixture) : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture = fixture;

    [Fact]
    public async Task GetVersions_WithoutAuth_Returns401()
    {
        var anon = new ApiClient(_fixture.AdminClient.BaseUrl, _fixture.AdminClient.IgnoreSsl);

        var response = await anon.GetVersionsAsync(_fixture.ContractId, withAuth: false);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetVersions_WithAuth_ReturnsList()
    {
        var response = await _fixture.AdminClient.GetVersionsAsync(_fixture.ContractId);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(ApiClient.JsonOptions);
        body.TryGetProperty("items", out var items).ShouldBeTrue();
        items.ValueKind.ShouldBe(JsonValueKind.Array);
    }

    [Fact]
    public async Task GetVersions_UnknownContract_ReturnsError()
    {
        var response = await _fixture.AdminClient.GetVersionsAsync(Guid.NewGuid());

        response.IsSuccessStatusCode.ShouldBeFalse();
    }

    [Fact]
    public async Task Upload_ThenDownload_ThenDelete_FullCrud()
    {
        var fileBytes = Encoding.UTF8.GetBytes("API test contract document content.");

        var upload = await _fixture.AdminClient.UploadDocumentAsync(
            _fixture.ContractId,
            fileBytes,
            "api-test.txt",
            "initial upload");

        upload.StatusCode.ShouldBe(HttpStatusCode.OK);

        var uploadBody = await upload.Content.ReadFromJsonAsync<JsonElement>(ApiClient.JsonOptions);
        var versionId = uploadBody.GetProperty("id").GetGuid();
        uploadBody.GetProperty("fileName").GetString().ShouldBe("api-test.txt");

        var download = await _fixture.AdminClient.DownloadAsync(versionId);
        download.StatusCode.ShouldBe(HttpStatusCode.OK);
        var downloaded = await download.Content.ReadAsByteArrayAsync();
        downloaded.ShouldBe(fileBytes);

        var delete = await _fixture.AdminClient.DeleteVersionAsync(versionId);
        delete.IsSuccessStatusCode.ShouldBeTrue();

        var getAfter = await _fixture.AdminClient.GetDocumentAsync(versionId);
        getAfter.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Upload_RequiresAttachPermission_Returns403()
    {
        var userName = "apitest_" + Guid.NewGuid().ToString("N")[..10];
        var password = "1q2w3E*Test!";

        var createUser = await _fixture.AdminClient.PostAsJsonAsync("api/identity/users", new
        {
            userName = userName,
            name = "API",
            surname = "Test",
            email = userName + "@example.com",
            password = password,
            isActive = true,
            roleNames = new string[0]
        });
        createUser.EnsureSuccessStatusCode();

        var userBody = await createUser.Content.ReadFromJsonAsync<JsonElement>(ApiClient.JsonOptions);
        var userId = userBody.GetProperty("id").GetGuid();

        try
        {
            var lowClient = new ApiClient(_fixture.AdminClient.BaseUrl, _fixture.AdminClient.IgnoreSsl);
            await lowClient.LoginAsync(userName, password);

            var fileBytes = Encoding.UTF8.GetBytes("should not be allowed");
            var upload = await lowClient.UploadDocumentAsync(_fixture.ContractId, fileBytes, "denied.txt");

            upload.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        }
        finally
        {
            await _fixture.AdminClient.DeleteAsync($"api/identity/users/{userId}");
        }
    }
}
