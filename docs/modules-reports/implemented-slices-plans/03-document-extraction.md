# Module 03 — Document Extraction (AI Assist)

**Status:** Partially implemented (provider integration + persistence layer) — in progress
**Plan source:** `docs/implementation/10-ai-assist-foundation.md`
**Report generated:** 2026-07-31
**Last reconciled:** 2026-07-31

> **Reconciliation note (2026-07-31):** The Module 01 foundation report
> (`01-foundation.md`) was reconciled on commit `e5e8e96` (2026-07-30 08:06),
> i.e. before Module 03 landed. A burst of document-extraction activity
> committed between 2026-07-30 09:37 and 2026-07-31 14:18 is not reflected in
> that report. This document captures the Module 03 state on disk.
>
> **Verified on the current tree:** `dotnet build Acme.LegalTech.slnx -c Debug`
> succeeds (0 errors, 0 warnings); the EF Core test suite passes **16/16**
> (no Module 03 tests exist yet, so the foundation count is unchanged).

This report documents the implementation status of Module 03 / AI Assist.
Only the **extraction-recording and provider-integration** layer is implemented.
The higher-level orchestration layer described in the plan
(`IngestionJob`, `ExtractionSuggestion`, `RiskAssessmentSuggestion`,
`SuggestionDecision` entities and the `AIAssist` permission group) is **not yet
implemented**. The slice is therefore **in progress**, not complete.

---

## Acceptance — Definition of Done

| # | Criterion | Status |
|---|-----------|--------|
| 1 | `DocumentExtraction` entity defined, `DbSet<DocumentExtractions>` registered, migration applies cleanly | Done |
| 2 | `IDocumentExtractionProvider` abstraction + Azure Document Intelligence implementation wired | Done |
| 3 | `DocumentExtractionResult` value object defined in `Domain.Shared` | Done |
| 4 | Contract document CRUD endpoints exposed and authorized | Done |
| 5 | Higher-level AI orchestration entities (`IngestionJob`, etc.) + `AIAssist` permissions per plan | Not started |
| 6 | Module 03 test/scenario coverage added | Not started |

---

## What is implemented on disk

### Domain.Shared
- `src/Acme.LegalTech.Domain.Shared/Common/DocumentExtractionResult.cs`
  — carrier for provider outputs: `IsSuccess`, `ErrorMessage`, `ProviderName`,
  `ExtractedAt`, plus extracted fields (`ExtractedTitle`, `ExtractedCounterparty`,
  `ExtractedEffectiveDate`, `ExtractedExpirationDate`, `ExtractedCategory`,
  `ExtractedRiskBaseline`, `ExtractedStatus`, `Obligations`, `DetectedTags`,
  `RawResponse`).
- `src/Acme.LegalTech.Domain.Shared/Common/DocumentExtractedObligation.cs`
  — value object carried in `DocumentExtractionResult.Obligations`
  (per the `prebuilt-contract` extraction result).

### Domain — Contracts bounded context
- `src/Acme.LegalTech.Domain/Contracts/ContractDocumentVersion.cs`
  — `Entity<Guid>, IMultiTenant`; `ContractId`, `VersionNumber`, `BlobName`,
  `FileName`, `ContentType`, `FileSize`, `UploadedById`, `UploadedAt`, `IsLatest`,
  `ChangeNote`; `MarkLatest()`/`UnmarkLatest()` mutators. (Pre-existed via
  Module 02 migration, refined during Module 03 work.)
- `src/Acme.LegalTech.Domain/Contracts/DocumentExtraction.cs`
  — `Entity<Guid>, IMultiTenant`; `ContractDocumentVersionId`, `ProviderName`,
  `ExtractedAt`, `Status`, `ErrorMessage`, extracted-field properties, `RawResponse`;
  constructed from a `DocumentExtractionResult`.

### Application.Contracts — Contracts
- `src/.../Application.Contracts/Contracts/IDocumentExtractionProvider.cs`
  — `ExtractAsync(IRemoteStreamContent, string contentType, CancellationToken)`
  returning `DocumentExtractionResult`.
- `src/.../Application.Contracts/Contracts/DocumentExtractionDto.cs` — DTO.
- `src/.../Application.Contracts/Contracts/IContractDocumentAppService.cs`
  — app-service contract (versions/upload/get/download/delete).
- `src/.../Application.Contracts/Contracts/ContractsBlobContainer.cs` — blob
  container name abstraction.

### Application — Contracts / Processing
- `src/.../Application/Contracts/ContractDocumentAppService.cs`
  — implements `IContractDocumentAppService`: `GetVersionsAsync`, `UploadAsync`,
  `GetAsync`, `DownloadAsync`, `DeleteVersionAsync`; delegates blob I/O to ABP
  blob abstractions and validation per extension/content-type.
- `src/.../Application/Contracts/AzureDocumentIntelligenceExtractionProvider.cs`
  — `IDocumentExtractionProvider, ITransientDependency`; reads
  `Azure:DocumentIntelligence:Endpoint` / `:Key` from configuration; falls back
  to a failure `DocumentExtractionResult` with a configuration message when unset;
  calls `DocumentIntelligenceClient.AnalyzeDocumentAsync` with the
  `prebuilt-contract` model and maps `Title`, `CustomerName`/`VendorName`
  (counterparty), `EffectiveDate`, `ExpirationDate`, `ServiceType` (category);
  catches provider errors and returns a failed result (never throws out).
- `src/.../Application/Data/ContractDocumentBlobSeedContributor.cs` — blob
  data seeder for contract-document fixtures.

### HttpApi — Controllers
- `src/.../HttpApi/Controllers/ContractDocumentController.cs`
  — `[ApiController] [Route("api/app/contract-document")]` exposing:
  - `GET versions/{contractId}` → `[Authorize(Contracts.Default)]`
  - `POST upload/{contractId}` → `[Authorize(Contracts.AttachDocument)]`
  - `GET {id}` → `[Authorize(Contracts.AttachDocument)]`
  - `GET versions/download/{versionId}` → `[Authorize(Contracts.AttachDocument)]`
  - `DELETE versions/{versionId}` → `[Authorize(Contracts.AttachDocument)]`

> **Authorization note:** All document endpoints are secured with the existing
> `LegalTech.Contracts` permissions (`Default` / `AttachDocument`) from Module 01.
> No `AIAssist.*` permission group exists yet, so the AI-Assist permission tree
> described in the plan is not wired.

### EF Core
- `src/.../EntityFrameworkCore/LegalTechDbContext.cs` — registers
  `DbSet<DocumentExtraction> DocumentExtractions` and configures
  `AppDocumentExtractions` (`ConfigureByConvention`, indexes on
  `ContractDocumentVersionId` and the `(ContractDocumentVersionId, ProviderName)`
  pair).

### Migrations (chronological)
- `20260730010326_Module03_DocumentExtraction` — creates the `AppDocumentExtractions`
  table with full column set (audit, multi-tenancy, provider metadata, extracted
  fields, `RawResponse`) and the two supporting indexes.

---

## Use Cases

The following use cases are **currently implemented** in Module 03 (extraction
recording + provider integration). Each is backed by the code on disk; see
`ContractDocumentAppService.cs` and `AzureDocumentIntelligenceExtractionProvider.cs`.

### 1. Upload a contract document and automatically extract its fields

- **Actor:** authenticated tenant user with `LegalTech.Contracts.AttachDocument`.
- **Entry point:** `POST api/app/contract-document/upload/{contractId}`
  → `ContractDocumentAppService.UploadAsync`.
- **Flow:**
  1. Validates the request carries a non-empty file with an allowed extension
     (`.pdf`, `.doc`, `.docx`, `.txt`, `.xls`, `.xlsx`, `.png`, `.jpg`, `.jpeg`);
     otherwise throws `LegalTech:Contract:UnsupportedFileType`.
  2. Resolves the content type (extension map, falling back to the supplied type).
  3. Persists the blob via the `ContractsBlobContainer` ABP abstraction under
     `contracts/{contractId}/{guid}{ext}`.
  4. Auto-versioning: computes `VersionNumber = max(existing) + 1` (or `1`) and
     **unmarks any prior "latest" versions** so at most one `IsLatest` remains
     per contract (enforced by a unique index on `(ContractId, IsLatest)`).
  5. Creates and inserts a `ContractDocumentVersion` with `IsLatest = true`.
  6. **Triggers extraction synchronously**: resolves `IDocumentExtractionProvider`
     (Azure Document Intelligence `prebuilt-contract` model) and calls
     `ExtractAsync(document, contentType)`.
  7. Persists a `DocumentExtraction` row (provider, status `Success`/`Failed`,
     extracted `Title`, `Counterparty`, `EffectiveDate`, `ExpirationDate`,
     `Category`, `RiskBaseline`, `RawResponse`) against the new version.
  8. Returns the version DTO annotated with `ExtractionStatus` + extracted fields.
- **Failure mode / graceful degradation:** if the provider call throws (e.g. SDK
  error or non-configured Azure credentials), the exception is caught, logged at
  `Error` level with the version id, `ExtractionStatus = "Error"` is set on the DTO,
  and the **upload still succeeds** — the version record and blob are retained so
  the document is never lost. A missing Azure configuration returns a `Failed`
  extraction result (not a startup crash).
- **Provider:** `AzureDocumentIntelligenceExtractionProvider` — maps
  `CustomerName`/`VendorName` → counterparty, `EffectiveDate`/`ExpirationDate`,
  `ServiceType` → category, `Title`.

### 2. View a document version with its extraction results

- **Actor:** authenticated tenant user with `LegalTech.Contracts.Default`.
- **Entry point:** `GET api/app/contract-document/{id}` → `ContractDocumentAppService.GetAsync`.
- **Flow:** looks up the `ContractDocumentVersion` by id; if a `DocumentExtraction`
  exists for that version, overlays `ExtractionStatus` + extracted fields onto the
  returned DTO. Returns 404-style behavior when the version does not exist
  (repository `GetAsync` throws the standard ABP not-found flow).

### 3. List all versions of a contract's documents with per-version extraction status

- **Actor:** authenticated tenant user with `LegalTech.Contracts.Default`.
- **Entry point:** `GET api/app/contract-document/versions/{contractId}`
  → `ContractDocumentAppService.GetVersionsAsync`.
- **Flow:** validates the parent `Contract` exists (throws
  `LegalTech:Contract:NotFound` with `ContractId` otherwise), fetches all versions,
  orders them **descending by `VersionNumber`**, and for each version attaches its
  extraction status/fields if present. Enables a version-history timeline view with
  at-a-glance extraction health.

### 4. Download / stream a document version

- **Actor:** authenticated tenant user with `LegalTech.Contracts.Default`.
- **Entry point:** `GET api/app/contract-document/versions/download/{versionId}`
  → `ContractDocumentAppService.DownloadAsync`.
- **Flow:** resolves the version, streams the blob via
  `_blobContainer.GetAsync(version.BlobName)`, and returns an
  `IRemoteStreamContent` carrying the original `FileName` and `ContentType`
  (correct download behavior in clients).

### 5. Delete a document version

- **Actor:** authenticated tenant user with `LegalTech.Contracts.AttachDocument`.
- **Entry point:** `DELETE api/app/contract-document/versions/{versionId}`
  → `ContractDocumentAppService.DeleteVersionAsync`.
- **Flow:** removes the `ContractDocumentVersion` record and **deletes its blob**
  from the `ContractsBlobContainer`. (No cascade on `DocumentExtraction` rows; the
  extraction is left as an audit record referencing the now-deleted version.)

---

## What is NOT yet implemented

Per `docs/implementation/10-ai-assist-foundation.md`, the plan also calls for:

- **`AIAssist` permission group** (`Default`, `RunJobs`, `ReviewSuggestions`,
  `ConfigureProviders`) — **not present**. No `LegalTechPermissions.AIAssist`
  constant, no registration in `LegalTechPermissionDefinitionProvider`.
- **Orchestration entities** — `IngestionJob`, `ExtractionSuggestion`,
  `RiskAssessmentSuggestion`, `SuggestionDecision` — **not present** (confirmed
  no files match these names anywhere under `src/`).
- **Retrieval assist pipeline** — the "retrieval assist" slice (controlled
  retrieval over approved content) — **not present**.
- **Module 03 tests** — no BDD / migration / unit tests for document extraction
  exist (EF Core suite is still the 16 foundation + contract-intake tests).

These remain planned. Module 03 is therefore **partially implemented**: the
persistence + provider-integration foundation is in place and green, but the
human-in-the-loop suggestion/review/review console is not.

---

## Data rules and failure modes

- Provider calls are wrapped in try/catch and **never throw** to the caller; a
  failed extraction returns a `DocumentExtractionResult` with `IsSuccess = false`,
  the `ErrorMessage`, and `ProviderName` set, enabling retry/observability without
  breaking contract-document core flows (consistent with plan rule
  "Prevent provider errors from breaking core CLM operations").
- Missing Azure configuration returns a deterministic failure result rather than
  a startup crash.

---

## Observability

- Provider failures are logged via
  `ILogger<AzureDocumentIntelligenceExtractionProvider>` with content type.
- (Gap vs. plan:) The plan asks for job-queue health, provider-failure, and
  suggestion-acceptance-rate metrics; no such metrics or background-job
  instrumentation exist yet for Module 03.

---

## Tests and acceptance

No Module 03 tests have been added. The existing EF Core suite (the canonical
project for foundation/permission/migration tests, per `01-foundation.md`) remains
at **16 passed, 0 failed**. Build verification:

```
dotnet build Acme.LegalTech.slnx -c Debug   ->  Build succeeded (0 errors; 0 warnings)
dotnet test ...EntityFrameworkCore.Tests    ->  16 passed, 0 failed
```

---

## Application services implemented on disk

- `Application/Contracts/ContractDocumentAppService.cs` — document version CRUD
  (list/upload/get/download/delete) with extension/content-type validation.
- `Application/Contracts/AzureDocumentIntelligenceExtractionProvider.cs` —
  `Azure.AI.DocumentIntelligence` adapter (provider abstraction).
- `HttpApi/Controllers/ContractDocumentController.cs` — REST endpoints
  (`api/app/contract-document/...`).
- `Application/Data/ContractDocumentBlobSeedContributor.cs` — blob seeding.

These extend the contract-document feature that was scaffolded in Module 01/02;
they do not yet back a full AI-assist workflow.

---

## Risks and mitigations

| Risk | Mitigation | Outcome |
|------|------------|---------|
| Provider errors break document upload/download | Guard provider calls; return failure result, never throw | Done |
| Missing Azure config crashes startup | Early-return failure result when endpoint/key unset | Done |
| AI orchestration entities deferred past foundation report | Track as explicit not-started backlog | Open |
| No Module 03 tests → green build hides unverified behavior | Add migration + BDD coverage for `AppDocumentExtractions` | Open |

---

## Files changed (summary)

**New**
- `Domain.Shared/Common/DocumentExtractionResult.cs`
- `Domain.Shared/Common/DocumentExtractedObligation.cs`
- `Domain/Contracts/DocumentExtraction.cs`
- `Application.Contracts/Contracts/IDocumentExtractionProvider.cs`
- `Application.Contracts/Contracts/DocumentExtractionDto.cs`
- `Application/Contracts/AzureDocumentIntelligenceExtractionProvider.cs`
- `Application/Data/ContractDocumentBlobSeedContributor.cs`
- `HttpApi/Controllers/ContractDocumentController.cs`
- `EntityFrameworkCore/Migrations/20260730010326_Module03_DocumentExtraction.cs[.Designer.cs]`
- `docs/Deployment-formRecognizerCreate/deployment.json`, `deployment_operations.json`

**Modified**
- `EntityFrameworkCore/LegalTechDbContext.cs` (`DbSet<DocumentExtraction>` + config)
- `EntityFrameworkCore/Migrations/LegalTechDbContextModelSnapshot.cs` (model snapshot)
- `Application/LegalTechApplicationMappers.cs` (DocumentExtraction mapping)
- `src/Acme.LegalTech.HttpApi.Host/appsettings.json` (added
  `Azure:DocumentIntelligence:Endpoint` / `:Key` configuration keys)
- `src/Acme.LegalTech.Application/Acme.LegalTech.Application.csproj` (added
  `Azure.AI.DocumentIntelligence` SDK reference)
- Angular `app/contracts/contracts.routes.ts`,
  `app/contracts/contract-detail.component.ts` (route/permission wiring;
  latest commit `313d83b`, 2026-07-31)

**Not started (planned, per `docs/implementation/10-ai-assist-foundation.md`)**
- `AIAssist` permission group + sub-permissions
- `IngestionJob`, `ExtractionSuggestion`, `RiskAssessmentSuggestion`,
  `SuggestionDecision` domain entities / application services
- Retrieval-assist pipeline
- Module 03 unit / integration / BDD tests
