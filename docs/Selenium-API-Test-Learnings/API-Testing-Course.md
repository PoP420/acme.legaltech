# Mastering API Testing — A Hands-On Course

> Built from a real, working test suite that drives the **Acme.LegalTech** ABP API over HTTP.
> Every example below maps to code in `test/Acme.LegalTech.HttpApi.Tests/`.
> Read it top-to-bottom and you will go from "what is an API test?" to writing robust
> integration tests against a real authentication/authorization framework.

---

## 0. What you will build mentally

By the end of this course you will be able to:

- Explain **why API tests exist** and how they differ from UI/Selenium tests.
- Read an **API contract** (routes, permissions, request/response shapes) and turn it into tests.
- Authenticate against an **OpenIddict / OAuth2** token endpoint from code.
- Understand **anti-forgery**, **Bearer vs cookie auth**, and **ABP error wrapping**.
- Build a **reusable HTTP test client** and a **test fixture** (xunit `IAsyncLifetime`).
- Write **happy-path and negative-path** tests (401, 403, 404/error, full CRUD).
- Debug a flaky integration test and make it **resilient**.

---

## 1. Module 1 — Why API testing (and not Selenium)

Selenium drives a browser. Our goal is to test the **API the browser would call**, not the pixels.

| Concern | UI / Selenium | HTTP API test |
|---|---|---|
| Speed | Slow (launch browser, render) | Fast (one TCP call) |
| Stability | Brittle (CSS/selectors change) | Stable (contracts change less) |
| Auth setup | Must click through login UI | POST credentials, get token |
| Parallelism | Hard (one browser per thread) | Easy (stateless clients) |
| Best for | End-to-end journeys, UX | Contract, permissions, data correctness |

**Rule of thumb:** Test *business rules and contracts* at the API layer; reserve Selenium for a
thin slice of real user journeys.

The plan that started this work originally said "Selenium", but that was a misnomer — these are
**HTTP integration tests** that exercise the API the same way a SPA would.

---

## 2. Module 2 — Know the system under test

You cannot test an API you do not understand. Here is what the Acme.LegalTech API looks like.

### 2.1 Framework: ABP (ASP.NET Boilerplate)

- Controllers inherit `AbpControllerBase` and use ABP permission attributes.
- Responses are **wrapped**:
  - Collections → `{ "items": [ ... ] }` (`ListResultDto<T>`)
  - Single object → the object directly (e.g. `{ "id": "...", "fileName": "..." }`)
  - Errors → `{ "error": { "code": "LegalTech:Contract:NotFound", "message": "..." } }`
- Some server exceptions (e.g. `BusinessException`) map to **403** rather than 404.

### 2.2 The endpoints under test

From `src/Acme.LegalTech.HttpApi/Controllers/ContractDocumentController.cs`:

| Method & route | Permission | Notes |
|---|---|---|
| `GET /api/app/contract-document/versions/{contractId}` | `Contracts.Default` | Returns `{ items: [...] }` |
| `POST /api/app/contract-document/upload/{contractId}` (multipart) | `Contracts.AttachDocument` | Uploads a file |
| `GET /api/app/contract-document/{id}` | `Contracts.AttachDocument` | Get a version |
| `GET /api/app/contract-document/versions/download/{versionId}` | `Contracts.AttachDocument` | Streams the file |
| `DELETE /api/app/contract-document/versions/{versionId}` | `Contracts.AttachDocument` | Deletes a version |

> The contracts themselves are exposed by `IContractAppService` (an `ICrudAppService`), so ABP
> auto-generates `GET /api/app/contract` and `POST /api/app/contract`. We use the list endpoint to
> obtain a real `contractId` for the document tests.

### 2.3 Authentication is NOT what you might guess

The plan assumed a simple `POST /api/account/login` returning `{ token }`. In reality this app:

- Uses **OpenIddict** (OAuth2). The browser/SPA logs in via the **password grant**.
- A public client `LegalTech_App` is seeded (`src/.../OpenIddict/OpenIddictDataSeedContributor.cs`) with
  `GrantTypes.Password`.
- We request a JWT:

```
POST /connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=password&client_id=LegalTech_App&username=admin@abp.io&password=1q2w3E*&scope=LegalTech
```

Response: `{ "access_token": "eyJ...", "token_type": "Bearer", "expires_in": 3599 }`

We then send `Authorization: Bearer <access_token>` on every call.

**Why Bearer and not the cookie?** ABP's anti-forgery filter
(`AbpAutoValidateAntiforgeryTokenAuthorizationFilter`) **skips validation when a Bearer header is
present**. Cookie-auth POSTs would be rejected with 400 ("antiforgery cookie not present"). So Bearer
auth is both correct *and* convenient.

### 2.4 Authorization (permissions)

Permissions are checked with `[Authorize(LegalTechPermissions.Contracts.AttachDocument)]`.
A user **without** that permission gets **403** on upload. This is exactly what we assert in the
negative test.

---

## 3. Module 3 — Project structure & tooling

We placed the suite **inside the Acme.LegalTech solution** at
`test/Acme.LegalTech.HttpApi.Tests/` and added it to `Acme.LegalTech.slnx`. It is **decoupled** —
it references the API only by URL, never by project reference. This is the "external consumer" design.

```
test/Acme.LegalTech.HttpApi.Tests/
├── Acme.LegalTech.HttpApi.Tests.csproj
├── src/
│   ├── TestConfiguration.cs     # base URL + credentials from env vars
│   ├── ApiClient.cs             # the reusable HTTP client (login, auth, helpers)
│   └── ApiTestFixture.cs        # xunit IAsyncLifetime: login once, get a contractId
└── test/csharp/
    ├── AuthApiTest.cs
    └── ContractDocumentApiTest.cs
```

**Conventions (mirrored from the rest of the repo):**
- `net10.0`, `Nullable enable`, imports `common.props`.
- Test stack: **xunit 2.9.3** + **Shouldly 4.3.0** + `Microsoft.NET.Test.Sdk`.
- No `ProjectReference` to app code → pure black-box HTTP testing.

> The original plan suggested NUnit + RestSharp + a `SeleniumCodebaseCourse` folder. None of those
> exist in this repo, so we followed the repo's *actual* conventions (xunit/Shouldly) and placed the
> project where it naturally belongs. Always prefer the conventions of the codebase you are in.

---

## 4. Module 4 — The test client (`ApiClient`)

A good API test suite has **one thin client** that every test reuses. Ours is `src/ApiClient.cs`.

Key responsibilities:

1. **Relax the dev cert** (self-signed `localhost:44334`):
   ```csharp
   var handler = new HttpClientHandler();
   if (ignoreSslErrors)
       handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
   ```
   > ⚠️ Never ship this relaxation to a real-TLS CI endpoint. We default it to `true` only for
   > `localhost`/`127.0.0.1`.

2. **Login via the token endpoint** (form-urlencoded, not JSON):
   ```csharp
   var content = new FormUrlEncodedContent(new Dictionary<string, string>
   {
       ["grant_type"] = "password",
       ["client_id"] = ClientId,      // "LegalTech_App"
       ["username"]   = userName,
       ["password"]   = password,
       ["scope"]      = Scope          // "LegalTech"
   });
   return await _httpClient.PostAsync("connect/token", content);
   ```

3. **Attach the Bearer token** on every authed call:
   ```csharp
   private void ApplyAuth(HttpRequestMessage request, bool withAuth)
   {
       if (withAuth && !string.IsNullOrEmpty(_token))
           request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
   }
   ```

4. **Typed helpers per endpoint** keep tests readable:
   ```csharp
   public async Task<HttpResponseMessage> GetVersionsAsync(Guid contractId, bool withAuth = true)
       => await GetAsync($"api/app/contract-document/versions/{contractId}", withAuth);

   public async Task<HttpResponseMessage> UploadDocumentAsync(Guid contractId, byte[] fileBytes,
       string fileName, string? changeNote = null, bool withAuth = true) { /* multipart */ }
   ```

5. **Resilience:** the token endpoint occasionally 500s (see Module 8). `LoginAsync` retries on 5xx:
   ```csharp
   for (var attempt = 0; attempt < 3; attempt++)
   {
       response = await LoginRawAsync(userName, password);
       if (response.IsSuccessStatusCode || (int)response.StatusCode < 500) break;
       await Task.Delay(TimeSpan.FromMilliseconds(400 * (attempt + 1)));
   }
   ```

> **Learning point:** centralize transport concerns (auth, TLS, retries, JSON options) in one class.
> Tests should read like sentences, not like HTTP plumbing.

---

## 5. Module 5 — Fixtures & the test lifecycle

xunit has no `[OneTimeSetUp]`. Instead we implement **`IAsyncLifetime`**:

```csharp
public class ApiTestFixture : IAsyncLifetime
{
    public ApiClient AdminClient { get; private set; } = default!;
    public Guid ContractId { get; private set; }
    public bool CreatedContract { get; private set; }

    public async Task InitializeAsync()   // runs ONCE before the class's tests
    {
        AdminClient = new ApiClient(TestConfiguration.BaseUrl, TestConfiguration.IgnoreSsl);
        await AdminClient.LoginAsync(TestConfiguration.AdminUser, TestConfiguration.AdminPassword);
        ContractId = await GetOrCreateContractIdAsync();
    }

    public async Task DisposeAsync()      // runs ONCE after
    {
        if (CreatedContract && ContractId != Guid.Empty)
            try { await AdminClient.DeleteAsync($"api/app/contract/{ContractId}"); }
            catch { /* best-effort cleanup */ }
    }
}
```

The test class declares it via the interface:

```csharp
public class ContractDocumentApiTest(ApiTestFixture fixture) : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture = fixture;
    ...
}
```

**Why login once?** Token issuance is relatively expensive and the token is reusable. Logging in per
test would be slow and could trip rate limits / concurrency quirks.

**Getting a `contractId`:** we first try the seeded list (`GET /api/app/contract?maxResultCount=1`);
only if empty do we `POST` one. This avoids depending on seed data while still not polluting it when
it exists.

> **Data hygiene rule:** tests that *create* data must *delete* it. The fixture tracks
> `CreatedContract` so it only cleans up what it made — never the seeded contracts.

---

## 6. Module 6 — The test cases (read each as a lesson)

### 6.1 `AuthApiTest.Login_Success_ReturnsToken`
Asserts the happy path: after `LoginAsync`, `client.Token` is non-empty (a JWT).

### 6.2 `AuthApiTest.Login_BadPassword_ReturnsUnauthorized`
```csharp
var response = await client.LoginRawAsync(..., "NotTheRightPassword123!");
((int)response.StatusCode).ShouldBeOneOf(400, 401);
```
> **Lesson:** wrong credentials return **400** (`invalid_grant`) here, not 401. Always probe the
> real API before hard-coding an expected status. We accept 400 *or* 401 to stay portable.

### 6.3 `ContractDocumentApiTest.GetVersions_WithoutAuth_Returns401`
Creates a *fresh, unauthenticated* client and calls with `withAuth: false`:
```csharp
var anon = new ApiClient(_fixture.AdminClient.BaseUrl, _fixture.AdminClient.IgnoreSsl);
var response = await anon.GetVersionsAsync(_fixture.ContractId, withAuth: false);
response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
```
> **Lesson:** negative tests need a client that deliberately has *no* credentials. Never reuse the
> authed client for an "unauthorized" check.

### 6.4 `ContractDocumentApiTest.GetVersions_WithAuth_ReturnsList`
```csharp
var body = await response.Content.ReadFromJsonAsync<JsonElement>(ApiClient.JsonOptions);
body.TryGetProperty("items", out var items).ShouldBeTrue();
items.ValueKind.ShouldBe(JsonValueKind.Array);
```
> **Lesson:** assert the **wrapper shape** (`items` array), not just 200. This catches contract
> regressions where the envelope changes.

### 6.5 `ContractDocumentApiTest.GetVersions_UnknownContract_ReturnsError`
```csharp
var response = await _fixture.AdminClient.GetVersionsAsync(Guid.NewGuid());
response.IsSuccessStatusCode.ShouldBeFalse();
```
> **Lesson:** for an unknown id the server throws a `BusinessException` → non-2xx. We assert
> *failure* without over-constraining the exact code (403 vs 404), because ABP's mapping can change.

### 6.6 `ContractDocumentApiTest.Upload_ThenDownload_ThenDelete_FullCrud`
The full happy path:
```csharp
var upload   = await client.UploadDocumentAsync(contractId, fileBytes, "api-test.txt", "initial upload");
upload.StatusCode.ShouldBe(HttpStatusCode.OK);
var versionId = (await upload.Content.ReadFromJsonAsync<JsonElement>(...)).GetProperty("id").GetGuid();

var download = await client.DownloadAsync(versionId);
(await download.Content.ReadAsByteArrayAsync()).ShouldBe(fileBytes);   // round-trip bytes

var delete   = await client.DeleteVersionAsync(versionId);
delete.IsSuccessStatusCode.ShouldBeTrue();                             // 204 NoContent

var gone     = await client.GetDocumentAsync(versionId);
gone.StatusCode.ShouldBe(HttpStatusCode.NotFound);
```
> **Lesson:** prefer **behavioral assertions** (uploaded bytes == downloaded bytes) over merely
> "status 200". Also note DELETE returns **204**, not 200.
> The server calls Azure Document Intelligence during upload; when that fails it is caught
> server-side and the upload still returns 200 — a good reminder that not every internal error
> surfaces to the client.

### 6.7 `ContractDocumentApiTest.Upload_RequiresAttachPermission_Returns403`
Creates a low-privilege user (no roles → lacks `Contracts.AttachDocument`), logs in as them, and
asserts 403 on upload:
```csharp
var createUser = await AdminClient.PostAsJsonAsync("api/identity/users", new
{
    userName = userName, name = "API", surname = "Test",
    email = userName + "@example.com", password = password,
    isActive = true,                 // required! inactive users cannot login
    roleNames = new string[0]        // no roles → no AttachDocument permission
});
// ... login as low-priv user, upload, expect Forbidden ...
finally { await AdminClient.DeleteAsync($"api/identity/users/{userId}"); }  // cleanup
```
> **Lesson:** authorization tests need a *real* low-privilege principal. Create one dynamically and
> **always delete it in `finally`**. Also: a freshly created user is **inactive** by default — you
> must set `isActive: true` or login returns `account_inactive`.

---

## 7. Module 7 — Running the suite

```bash
# 1) Start the API host (serves https://localhost:44334, seeds 5 contracts + versions)
dotnet run --project src/Acme.LegalTech.HttpApi.Host

# 2) In another terminal, run the tests
dotnet test test/Acme.LegalTech.HttpApi.Tests/Acme.LegalTech.HttpApi.Tests.csproj
```

Environment overrides (all optional):

| Variable | Default | Purpose |
|---|---|---|
| `ApiBaseUrl` | `https://localhost:44334` | target host |
| `ApiUser` | `admin@abp.io` | admin username |
| `ApiPassword` | `1q2w3E*` | admin password |
| `ApiIgnoreSsl` | `true` for localhost | relax self-signed cert |

**Result:** `Passed! - Failed: 0, Passed: 7`.

---

## 8. Module 8 — Pitfalls we hit (and you will too)

1. **Cookie vs Bearer.** The login page sets a cookie, but POST/DELETE need a Bearer token or they
   fail anti-forgery (400). Use the token endpoint + Bearer.
2. **Bad password → 400, not 401.** Probe first; assert what the API actually returns.
3. **DELETE → 204, not 200.** `ShouldBe(HttpStatusCode.OK)` on a delete will fail.
4. **ABP wraps lists in `items`.** Assert the envelope, not just status.
5. **`BusinessException` → 403.** Unknown-resource errors may come back as 403; assert "not success".
6. **Inactive seeded/test users.** New users need `isActive: true` to log in.
7. **Intermittent 500 on login.** `IdentityUserStore.UpdateAsync` can throw
   `AbpDbConcurrencyException` updating the user row on sign-in. Retry on 5xx. **This is a real app
   bug** (stale `ConcurrencyStamp` on the seeded admin) worth fixing at the data layer.
8. **Self-signed TLS.** Relax cert validation for `localhost` only; never for production URLs.
9. **Test pollution.** Every created entity (contract, user) must be cleaned up, ideally only when
   the test created it.
10. **Don't hard-code GUIDs.** Resolve a real `contractId` at runtime instead of guessing.

---

## 9. Module 9 — Exercises to master API testing

1. **Add a test** that uploads a `.pdf` (not `.txt`) and asserts the returned `contentType`.
2. **Add a negative test** for upload with an empty file — what status does the API return?
3. **Add a permission test** for `GetVersionsAsync` using a low-privilege user (expect 403).
4. **Write a test** that calls `GET /api/app/contract` and asserts `totalCount >= 5` (seed data).
5. **Refactor** `ApiClient` to cache the token and auto-refresh it before `expires_in` elapses.
6. **Make the suite CI-ready:** run `dotnet test` against a fresh host in GitHub Actions (no
   ChromeDriver needed — faster than the UI suite).
7. **Reproduce & fix the concurrency bug:** give the seeded admin a correct `ConcurrencyStamp` and
   remove the login retry (it should no longer be needed).
8. **Add contract-update coverage:** `PUT /api/app/contract/{id}` with a low-priv user → 403, with
   admin → 200.

---

## 10. Appendix — quick reference

```csharp
// Auth
POST /connect/token  (form-urlencoded: grant_type=password, client_id=LegalTech_App,
                       username, password, scope=LegalTech)  → { access_token }

// Contract documents (Bearer)
GET    /api/app/contract-document/versions/{contractId}        → { items: [...] }      (Default)
POST   /api/app/contract-document/upload/{contractId}  multipart (AttachDocument)
GET    /api/app/contract-document/{id}                          (AttachDocument)
GET    /api/app/contract-document/versions/download/{versionId} (AttachDocument)
DELETE /api/app/contract-document/versions/{versionId}  → 204  (AttachDocument)

// Helpers
GET    /api/app/contract?maxResultCount=1   → pick items[0].id as contractId
POST   /api/identity/users  (isActive:true, roleNames:[])  → low-priv user
```

**Status-code cheat sheet for this API:** `200` OK · `204` No Content (delete) · `400` invalid
grant / anti-forgery · `401` unauthorized · `403` missing permission or `BusinessException` ·
`404` not found (after delete).

---

*This course was generated from a working, passing 7-test suite. The best way to learn is to run it,
break one assertion on purpose, watch it fail, then fix it.*
