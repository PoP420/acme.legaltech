# Plan: Add HTTP API tests for the Acme.LegalTech API

## Context & decisions (from user)
- **What to build:** HTTP/API tests (no browser). Selenium cannot test APIs; we use an HTTP client.
- **Target:** the Acme.LegalTech API (ABP app), not `practicesoftwaretesting.com`.
- **Layout:** a **separate test project** inside `SeleniumCodebaseCourse`, independent from the existing Selenium UI project.

> "Selenium" in the request is a misnomer — these are HTTP integration tests that drive the API the same way a browser/spa would.

## Target API (discovered)
- Base URL (dev): `https://localhost:44334` (from `src/Acme.LegalTech.HttpApi.Host/Properties/launchSettings.json`).
- Auth: ABP JWT. `POST /api/account/login` with `{ userNameOrEmailAddress, password }` → `{ token }`; send as `Authorization: Bearer <token>`.
- Endpoints (`Acme.LegalTech.HttpApi/Controllers/ContractDocumentController.cs`), all require auth:
  - `GET  /api/app/contract-document/versions/{contractId:guid}` — needs `Contracts.Default`
  - `POST /api/app/contract-document/upload/{contractId:guid}` (multipart/form-data) — needs `Contracts.AttachDocument`
  - `GET  /api/app/contract-document/{id:guid}` — needs `Contracts.AttachDocument`
  - `GET  /api/app/contract-document/versions/download/{versionId:guid}` — needs `Contracts.AttachDocument`
  - `DELETE /api/app/contract-document/versions/{versionId:guid}` — needs `Contracts.AttachDocument`
- ABP wraps lists as `ListResultDto<T>` (`{ items: [...] }`) and errors as `{ error: { code, message } }`.

## Proposed project layout (inside SeleniumCodebaseCourse)
```
SeleniumCodebaseCourse/
├── SeleniumCSharpTests/            (existing UI library — untouched)
├── SeleniumCSharpTests.Tests/      (existing UI NUnit — untouched)
└── AcmeLegalTechApi.Tests/         ← NEW separate project (HTTP API tests)
    ├── AcmeLegalTechApi.Tests.csproj
    ├── src/
    │   ├── ApiClient.cs            (base URL + JWT auth + RestSharp wrapper)
    │   └── ApiTestBase.cs          (OneTimeSetUp: login → capture token; config)
    └── test/csharp/
        ├── AuthApiTest.cs          (login happy/negative)
        └── ContractDocumentApiTest.cs
```
Add the new `.csproj` to `SeleniumCSharpTests.slnx` so `dotnet test` discovers it.

## Dependencies (new csproj)
- `net10.0`, `ImplicitUsings`, `Nullable`, `EnableDefaultCompileItems=false`, `<Compile Include="src\**\*.cs;test\**\*.cs" />`.
- Test: `Microsoft.NET.Test.Sdk`, `NUnit` (4.3.2, match course), `NUnit3TestAdapter`, `coverlet.collector`.
- HTTP: `RestSharp` (or `System.Net.Http` + `Newtonsoft.Json` already used by the course). **Recommend RestSharp** for terse request/auth handling.

## Configuration
- Base URL + credentials from env vars / `runsettings` (`BaseUrl`, `ApiUser`, `ApiPassword`), default `https://localhost:44334`. Keep secrets out of source (use `dotnet user-secrets` or CI variables).
- A seeded user with `Contracts.Default` + `Contracts.AttachDocument` is required (use the ABP seeded admin or a dedicated test user).

## Key design
- `ApiClient`: holds `RestClient`, `BaseUrl`, and a `Token`; method `LoginAsync()` posts `/api/account/login` and stores the bearer token; `Request(resource)` auto-attaches `Authorization`.
- `ApiTestBase`: `[OneTimeSetUp]` calls `ApiClient.LoginAsync()`; exposes the client to fixtures. `[TearDown]` optional cleanup of created documents.
- Reuse the course's POM-like separation: one thin client (main) + NUnit fixtures (test), mirroring `SeleniumCSharpTests`.

## Test cases (first cut)
1. `AuthApiTest.Login_Success_ReturnsToken` — 200 + non-empty token.
2. `AuthApiTest.Login_BadPassword_Returns401`.
3. `ContractDocumentApiTest.GetVersions_WithoutAuth_Returns401` — call without token.
4. `ContractDocumentApiTest.GetVersions_WithAuth_ReturnsList` — valid contractId → `items` array (may be empty).
5. `ContractDocumentApiTest.GetVersions_UnknownContract_Returns404orEmpty`.
6. `ContractDocumentApiTest.Upload_RequiresAttachPermission` — verify 403 with a low-privilege user (optional, needs 2nd user).
7. `ContractDocumentApiTest.Upload_ThenDownload_ThenDelete` — multipart upload → get version id → download stream → delete (full CRUD happy path).

## How to run
- Start the Acme.LegalTech host: `dotnet run --project src/Acme.LegalTech.HttpApi.Host` (serves `https://localhost:44334`).
- Run tests: `dotnet test AcmeLegalTechApi.Tests/AcmeLegalTechApi.Tests.csproj` (or whole solution).
- CI (GitHub Actions already in course): add a job that builds the host, runs API tests headless — no ChromeDriver needed (faster than UI suite).

## Risks / open questions
- **Location ambiguity:** per your answers the project lives in `SeleniumCodebaseCourse` and reaches Acme.LegalTech only by URL (external consumer). Alternative is to place it inside the Acme.LegalTech repo/solution (more natural for in-repo API tests, can run against the host directly). Confirm this is intended.
- Must confirm exact `/api/account/login` request/response shape against the running host (ABP version differences).
- Needs a test user with the right permissions; confirm seeding or create one.
- HTTPS self-signed on `localhost:44334` — may need `ServerCertificateCustomValidationCallback` to ignore dev cert in tests.
- Upload test requires a sample file fixture.
