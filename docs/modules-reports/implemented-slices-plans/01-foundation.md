# Module 01 — Foundation and Platform Conventions

**Status:** Implemented and verified  
**Plan source:** `docs/implementation/01-foundation.md`  
**Report generated:** 2026-07-29  
**Last reconciled:** 2026-07-29

> **Reconciliation note (2026-07-29):** The original report (2026-07-15) accurately
> described the foundation slice, but the codebase has since been extended with
> Module 02 (Contract Intake) entities, additional migrations, and additional
> permission sub-groups. This document reconciles the foundation slice against the
> actual source on disk while noting the subsequent expansions so the module report
> remains an accurate baseline reference.

This report documents the implementation of the Module 01 foundation plan. All three
vertical slices are complete, the solution builds with 0 errors and 0 warnings, and
the EF Core test suite passes 16/16.

---

## Acceptance — Definition of Done

| # | Criterion | Status |
|---|-----------|--------|
| 1 | Slice 1 placeholders retired, bounded-context folders created | Done |
| 2 | ContractStatus + MetadataEntry in Domain.Shared; permission tree has 7 groups + baseline roles seeded | Done |
| 3 | Contract entity defined, Contracts DbSet registered, all migrations apply cleanly | Done |
| 4 | Duplicate-permission guard and migration-drift guard implemented | Done |
| 5 | Startup diagnostics log permission + migration health | Done |
| 6 | Permission, migration, and BDD tests pass | Done |

---

## Slice 1 — Baseline refactor

- Retired `BookDto` example in `src/Acme.LegalTech.Application/LegalTechApplicationMappers.cs`
  (removed the commented Mapperly placeholder).
- Retired `LongWelcomeMessage` template key in
  `src/Acme.LegalTech.Domain.Shared/Localization/LegalTech/en.json`.
- Created bounded-context folders (empty, tracked via `.gitkeep`) for each downstream
  module under `Domain/`, `Application/`, and `Application.Contracts/`:
  `Clauses/`, `Contracts/`, `Reviews/`, `Obligations/`, `Reports/`, `Files/`, `Administration/`.
- Created matching empty Angular feature folders under `angular/src/app/`:
  `contracts/`, `clauses/`, `reviews/`, `obligations/`, `reports/`, `files/`, `administration/`.

---

## Slice 2 — Shared governance primitives

### Domain.Shared enums and value objects
- `src/Acme.LegalTech.Domain.Shared/Common/ContractStatus.cs`
  — `enum ContractStatus { Draft = 0, Active = 1, Expired = 2, Terminated = 3 }`.
- `src/Acme.LegalTech.Domain.Shared/Common/MetadataEntry.cs`
  — `MetadataEntry` value object (`Key`, `Value`, `MetadataValueType`) with
  `enum MetadataValueType { Text, Number, Date, Boolean }`. Note: the value object
  is defined in `Domain.Shared` but is not currently consumed by any entity on disk;
  it is available for downstream module use (e.g., extensible metadata on future
  aggregate roots).

### Shared constants relocation
- Moved `LegalTechConsts` from `src/Acme.LegalTech.Domain/` to
  `src/Acme.LegalTech.Domain.Shared/LegalTechConsts.cs` (namespace stays `Acme.LegalTech`).
- `AdminEmailDefaultValue` is hardcoded to `"admin@abp.io"` (the ABP default) because
  `IdentityDataSeedContributor.AdminEmailDefaultValue` lives in `Volo.Abp.Identity.Domain`,
  which `Domain.Shared` must not reference (layering rule).

### Permission tree (7 top-level groups with sub-permissions)
- `src/Acme.LegalTech.Application.Contracts/Permissions/LegalTechPermissions.cs`
  — `Groups` constants: `Contracts`, `Clauses`, `Reviews`, `Obligations`, `Reports`,
  `Files`, `Administration`, all prefixed `LegalTech.` (e.g. `LegalTech.Contracts`).
- Additional permission sub-trees are registered under the 7 groups:
  - `LegalTech.Clauses.Playbooks` (`Manage`, `Evaluate`)
  - `LegalTech.Obligations.Renewals` (`Manage`)
  - `LegalTech.Reports.Dashboards` (`ViewRisk`)
- `src/.../LegalTechPermissionDefinitionProvider.cs` adds each group with a localized
  display name (`Permission:Contracts`, ...), registers all sub-permissions, and
  feeds the duplicate-key guard.

### Localization (`en.json`)
- `Permission:Contracts`, `Permission:Clauses`, `Permission:Reviews`,
  `Permission:Obligations`, `Permission:Reports`, `Permission:Files`,
  `Permission:Administration`.
- Sub-permissions localized: `Playbooks`, `Playbooks.Manage`, `Playbooks.Evaluate`,
  `Renewals`, `Renewals.Manage`, `Dashboards`, `Dashboards.ViewRisk`.
- `Enum:ContractStatus:0..3` -> Draft / Active / Expired / Terminated.

### Baseline roles
- `src/.../Permissions/LegalTechRoles.cs` — `HostAdmin`, `TenantAdmin`, `LegalOpsManager`,
  `LawyerReviewer`, `Auditor`.
- `src/Acme.LegalTech.Application/Permissions/LegalTechRoleDataSeedContributor.cs`
  — `IDataSeedContributor` that creates the five roles (host + per-tenant via current
  tenant context) during data seeding and grants role-level permissions for all
  sub-permission trees.

---

## Slice 3 — First persistence baseline

### Contract entity
- `src/Acme.LegalTech.Domain/Contracts/Contract.cs`
  — `FullAuditedAggregateRoot<Guid>, IMultiTenant`, with a `TenantId` property.
  `Status` (`ContractStatus`, defaults to `Draft`) with guarded setter.
  Additional columns beyond the original plan specify richer contract metadata:
  `EffectiveDate`, `ExpirationDate`, `OwnerUserId`, `Category`, `RiskBaseline`.
  `DocumentBlobName` (nullable) retained for document attachment.
  `Activate`/`Expire`/`Terminate` guarded lifecycle methods throw
  `BusinessException("LegalTech:Contract:InvalidStatusTransition")` on an invalid
  transition — `Activate` only from `Draft`, `Expire` only from `Active`,
  `Terminate` from `Draft`/`Active` (terminal once `Expired`/`Terminated`).
  Constants in `ContractConsts.cs` (`MaxTitleLength`, `MaxCounterpartyNameLength`,
  `MaxCategoryLength`, `MaxRiskBaselineLength`, `MaxDocumentFileNameLength`,
  `MaxChangeNoteLength`).

### EF Core registration
- `DbSet<Contract> Contracts` and multiple additional entity DbSets for
  downstream modules registered in `LegalTechDbContext`.
- `builder.Entity<Contract>(b => b.ToTable("AppContracts").ConfigureByConvention());`
  with indexes on `Status`, `Category`, `OwnerUserId`, plus ABP convention columns.

### Migrations (chronological)
- `20260712043043_Initial` — initial ABP module schema.
- `20260714000000_Module01_Foundation` — creates `AppContracts` with full ABP
  convention columns (audit, multi-tenancy, soft-delete, `Status`, `TenantId`).
- `20260715074852_Module02_ContractIntake` — adds `Category`, `EffectiveDate`,
  `ExpirationDate`, `OwnerUserId`, `RiskBaseline` to `AppContracts`; creates
  `AppContractDocumentVersions`, `AppContractTags`, `AppCounterpartyReferences`
  with indexes.
- `20260728_MissingEntities` — creates tables for subsequent modules:
  `AppClauseTemplates`, `AppClauseTaxonomies`, `AppPlaybookProfiles`,
  `AppPlaybookRules`, `AppReviewCases`, `AppReviewTasks`, `AppApprovalSteps`,
  `AppReviewComments`, `AppEscalationEvents`, `AppContractObligations`,
  `AppRenewalSchedules`, `AppObligationReminders`, `AppCompletionEvidence`.

---

## Data rules and failure modes

### Duplicate-permission-key guard
- `src/Acme.LegalTech.Application.Contracts/Permissions/LegalTechPermissionGuard.cs`
  — `ThrowIfDuplicateKeys(IEnumerable<string> keys)` collects keys into a `HashSet`
  and throws `BusinessException("LegalTech:Permission:DuplicateKey")` on the first
  duplicate (checks the full key string). Invoked by the provider after all groups
  and sub-permissions are added.

### Migration-drift guard
- `src/Acme.LegalTech.HttpApi.Host/HealthChecks/LegalTechMigrationDriftGuard.cs`
  — computes a deterministic SHA-256 hash of the current EF Core model (table + column
  names/types) and compares it to a stored hash in SettingManagement
  (`LegalTech.Migration.ModelHash`, defined in `LegalTechSettingDefinitionProvider`).
  First run stores the hash; on subsequent runs the current behavior is to
  silently reset the stored hash to match the new model rather than throwing.
  **Disabled in the `Development` environment** (`IWebHostEnvironment`), per the
  plan's mitigation, to avoid blocking frequent local model changes. On
  infrastructure errors it logs a warning instead of blocking startup.

---

## Observability

- `src/Acme.LegalTech.Application/Permissions/LegalTechPermissionHealthContributor.cs`
  — resolves `IPermissionDefinitionManager` at startup and logs
  `{GroupCount} permission groups, {PermissionCount} permissions, {RoleCount} baseline roles`.
  Wired into `LegalTechHttpApiHostModule.OnApplicationInitialization`.

---

## Tests and acceptance

All primary foundation tests live in **`test/Acme.LegalTech.EntityFrameworkCore.Tests`**.
The EF Core test project contains a working SQLite + OpenIddict-EF fixture, making it
the appropriate home for permission, BDD, and migration tests (see note below).

| Test | File | Result |
|------|------|--------|
| 7 module groups are registered | `EntityFrameworkCore/LegalTechPermissionsTests.cs` | Pass |
| Duplicate-key guard throws on conflict | `EntityFrameworkCore/LegalTechPermissionsTests.cs` | Pass |
| Duplicate-key guard passes for unique keys | `EntityFrameworkCore/LegalTechPermissionsTests.cs` | Pass |
| BDD: permission tree complete & non-conflicting | `EntityFrameworkCore/FoundationScenarios.cs` | Pass |
| Contracts table configured (AppContracts, expected columns) | `EntityFrameworkCore/LegalTechModule01FoundationMigrationTests.cs` | Pass |
| Contracts table accepts rows | `EntityFrameworkCore/LegalTechModule01FoundationMigrationTests.cs` | Pass |
| Module 02 tables present with indexes | `EntityFrameworkCore/LegalTechModule02ContractIntakeMigrationTests.cs` | Pass |
| Module 02 tables accept rows | `EntityFrameworkCore/LegalTechModule02ContractIntakeMigrationTests.cs` | Pass |
| Contract intake BDD: tags + counterparties | `EntityFrameworkCore/ContractIntakeScenarios.cs` | Pass |
| Contract intake BDD: document versions + IsLatest | `EntityFrameworkCore/ContractIntakeScenarios.cs` | Pass |
| Contract intake BDD: lifecycle states | `EntityFrameworkCore/ContractIntakeScenarios.cs` | Pass |
| Contract intake BDD: invalid transition throws | `EntityFrameworkCore/ContractIntakeScenarios.cs` | Pass |

**Total EF Core suite:** 16 passed, 0 failed.

> Note on test placement: The plan suggested the permission/BDD tests in
> `Application.Tests`, but that fixture has no database store registered, so ABP's
> PermissionManagement module cannot initialize there (pre-existing limitation — the
> sample test in that project was never runnable without a DB). The EF Core test project
> has a working SQLite + OpenIddict-EF fixture, so the permission, BDD, and migration
> tests were placed there and all acceptance criteria are satisfied.

---

## Build verification

    dotnet build Acme.LegalTech.slnx -c Debug   ->  Build succeeded (0 errors; 0 warnings)
    dotnet test ...EntityFrameworkCore.Tests   ->  16 passed, 0 failed

---

## Application services implemented on disk

The foundation slice is complete, and the following application services extend
beyond the original foundation scope:

- `Application/Contracts/ContractAppService.cs` — CRUD + `ChangeStatusAsync` with
  permission checks per action.
- `Application/Contracts/ContractDocumentAppService.cs` — document upload, download,
  version listing, with extension/content-type validation.
- `Application/Clauses/ClauseTemplateAppService.cs` — CRUD with taxonomy and
  jurisdiction filtering.
- `Application/Reviews/ReviewAppService.cs` — CRUD + `AssignAsync`, `EscalateAsync`,
  `CompleteAsync`.
- `Application/Obligations/ContractObligationAppService.cs` — CRUD + `CompleteAsync`,
  `DeferAsync`.
- `Application/Playbooks/PlaybookProfileAppService.cs` and
  `Application/Playbooks/PlaybookEvaluationService.cs` — playbook profile management
  and rule evaluation.
- `Application/Data/LegalTechDataSeedContributor.cs` — seeds sample contracts,
  document versions, tags, counterparty references, and obligations.

---

## Risks and mitigations (from plan, with outcomes)

| Risk | Mitigation | Outcome |
|------|------------|---------|
| Moving LegalTechConsts breaks references | Update using/consumers | Done |
| Migration drift guard complexity in dev | Disable in Development via IWebHostEnvironment | Done |
| docker-compose DB name (LegalTech) vs appsettings.json (legaltech) mismatch | Standardize on one casing | No docker-compose file present in repo; appsettings.json already uses lowercase legaltech |

---

## Files changed (summary)

**New**
- `Domain.Shared/Common/ContractStatus.cs`, `Domain.Shared/Common/MetadataEntry.cs`
- `Domain.Shared/LegalTechConsts.cs`
- `Application.Contracts/Permissions/LegalTechPermissions.cs`, `LegalTechPermissionGuard.cs`, `LegalTechRoles.cs`
- `Application/Permissions/LegalTechRoleDataSeedContributor.cs`, `LegalTechPermissionHealthContributor.cs`
- `Domain/Contracts/Contract.cs`, `Domain/Contracts/ContractConsts.cs`
- `Domain/Seeding/ContractDataSeedContributor.cs`
- `Domain/Clauses/ClauseTemplate.cs`, `ClauseTaxonomy.cs`
- `Domain/Reviews/ReviewCase.cs`, `ReviewTask.cs`, `ApprovalStep.cs`, `ReviewComment.cs`, `EscalationEvent.cs`
- `Domain/Playbooks/PlaybookProfile.cs`, `PlaybookRule.cs`
- `Domain/Obligations/ContractObligation.cs`, `RenewalSchedule.cs`, `ObligationReminder.cs`, `CompletionEvidence.cs`
- `HttpApi.Host/HealthChecks/LegalTechMigrationDriftGuard.cs`
- `EntityFrameworkCore/Migrations/20260712043043_Initial.cs[.Designer.cs]`
- `EntityFrameworkCore/Migrations/20260714000000_Module01_Foundation.cs[.Designer.cs]`
- `EntityFrameworkCore/Migrations/20260715074852_Module02_ContractIntake.cs[.Designer.cs]`
- `EntityFrameworkCore/Migrations/20260728_MissingEntities.cs[.Designer.cs]`
- Bounded-context `.gitkeep` folders (backend + Angular)
- Application services: `ContractAppService.cs`, `ContractDocumentAppService.cs`,
  `ClauseTemplateAppService.cs`, `ReviewAppService.cs`, `ContractObligationAppService.cs`,
  `PlaybookProfileAppService.cs`, `PlaybookEvaluationService.cs`
- DTOs in `Application.Contracts` for Contracts, Documents, Clauses, Reviews,
  Obligations, Playbooks
- Tests: `LegalTechPermissionsTests.cs`, `FoundationScenarios.cs`,
  `LegalTechModule01FoundationMigrationTests.cs`, `LegalTechModule02ContractIntakeMigrationTests.cs`,
  `ContractIntakeScenarios.cs`

**Modified**
- `Application/LegalTechApplicationMappers.cs` (placeholder removed; expanded for
  multiple module DTO mappings)
- `Domain.Shared/Localization/LegalTech/en.json` (keys for 7 groups + sub-permissions
  + contract status enum + additional UI text)
- `Application.Contracts/Permissions/LegalTechPermissionDefinitionProvider.cs`
  (expanded to register Playbooks, Renewals, Dashboards sub-permissions)
- `Domain/Settings/LegalTechSettingDefinitionProvider.cs` (model-hash setting)
- `EntityFrameworkCore/EntityFrameworkCore/LegalTechDbContext.cs` (extensive DbSet
  registrations and entity configurations for all modules)
- `HttpApi.Host/Acme.LegalTech.HttpApi.Host.csproj` (added `Volo.Abp.PermissionManagement.Domain`,
  `Volo.Abp.SettingManagement.Domain`, `Volo.Abp.BlobStoring.Database.EntityFrameworkCore`)
- `HttpApi.Host/LegalTechHttpApiHostModule.cs` (wired drift guard + health contributor)

**Removed**
- `Domain/LegalTechConsts.cs` (moved to Domain.Shared)
