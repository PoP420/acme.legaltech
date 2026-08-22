using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Acme.LegalTech.HttpApi.Tests;

public class ApiClient
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private string? _token;

    public ApiClient(string baseUrl, bool ignoreSslErrors = false)
    {
        BaseUrl = baseUrl.TrimEnd('/');
        IgnoreSsl = ignoreSslErrors;

        var handler = new HttpClientHandler();
        if (ignoreSslErrors)
        {
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        }

        _httpClient = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
    }

    public string BaseUrl { get; }

    public bool IgnoreSsl { get; }

    public string? Token => _token;

    public string ClientId { get; set; } = "LegalTech_App";

    public string Scope { get; set; } = "LegalTech";

    public async Task<HttpResponseMessage> LoginRawAsync(string userName, string password)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = ClientId,
            ["username"] = userName,
            ["password"] = password,
            ["scope"] = Scope
        });

        return await _httpClient.PostAsync("connect/token", content);
    }

    public async Task LoginAsync(string userName, string password)
    {
        // The token endpoint can intermittently return 500 (AbpDbConcurrencyException
        // when ABP updates the user row on sign-in). Retry to ride through the
        // transient error; non-5xx responses (e.g. 400 invalid_grant) are not retried.
        HttpResponseMessage? response = null;
        for (var attempt = 0; attempt < 3; attempt++)
        {
            response = await LoginRawAsync(userName, password);
            if (response.IsSuccessStatusCode || (int)response.StatusCode < 500)
            {
                break;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(400 * (attempt + 1)));
        }

        response!.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        _token = body.GetProperty("access_token").GetString();

        if (string.IsNullOrEmpty(_token))
        {
            throw new InvalidOperationException("Token endpoint did not return an access_token.");
        }
    }

    private void ApplyAuth(HttpRequestMessage request, bool withAuth)
    {
        if (withAuth && !string.IsNullOrEmpty(_token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }
    }

    public async Task<HttpResponseMessage> GetAsync(string resource, bool withAuth = true)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, resource);
        ApplyAuth(request, withAuth);
        return await _httpClient.SendAsync(request);
    }

    public async Task<HttpResponseMessage> GetVersionsAsync(Guid contractId, bool withAuth = true)
        => await GetAsync($"api/app/contract-document/versions/{contractId}", withAuth);

    public async Task<HttpResponseMessage> GetDocumentAsync(Guid id, bool withAuth = true)
        => await GetAsync($"api/app/contract-document/{id}", withAuth);

    public async Task<HttpResponseMessage> DownloadAsync(Guid versionId, bool withAuth = true)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/app/contract-document/versions/download/{versionId}");
        ApplyAuth(request, withAuth);
        return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
    }

    public async Task<HttpResponseMessage> UploadDocumentAsync(
        Guid contractId,
        byte[] fileBytes,
        string fileName,
        string? changeNote = null,
        bool withAuth = true)
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "File", fileName);
        if (!string.IsNullOrEmpty(changeNote))
        {
            content.Add(new StringContent(changeNote), "ChangeNote");
        }

        var request = new HttpRequestMessage(HttpMethod.Post, $"api/app/contract-document/upload/{contractId}")
        {
            Content = content
        };
        ApplyAuth(request, withAuth);
        return await _httpClient.SendAsync(request);
    }

    public async Task<HttpResponseMessage> DeleteVersionAsync(Guid versionId, bool withAuth = true)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"api/app/contract-document/versions/{versionId}");
        ApplyAuth(request, withAuth);
        return await _httpClient.SendAsync(request);
    }

    public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string resource, T payload, bool withAuth = true)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, resource)
        {
            Content = JsonContent.Create(payload, options: JsonOptions)
        };
        ApplyAuth(request, withAuth);
        return await _httpClient.SendAsync(request);
    }

    public async Task<HttpResponseMessage> DeleteAsync(string resource, bool withAuth = true)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, resource);
        ApplyAuth(request, withAuth);
        return await _httpClient.SendAsync(request);
    }
}
