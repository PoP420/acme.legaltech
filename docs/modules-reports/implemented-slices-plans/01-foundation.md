# Module 01 — Foundation and Platform Conventions

**Status:** Implemented and verified
**Plan source:** `docs/implementation/01-foundation.md`
**Report generated:** 2026-07-15
**Last reconciled:** 2026-07-15

> **Reconciliation note (2026-07-15):** At report-creation time the `Contract`
> entity on disk was out of sync with this document and with the
> `Module01_Foundation` migration. The migration creates both a `Status` (int,
> not null) and a `TenantId` (uuid, null) column, but the entity defined neither
> `Status` nor `IMultiTenant`, and had no lifecycle methods. The entity has been
> reconciled so it now matches the report and the migration:
> `Contract : FullAuditedAggregateRoot<Guid>, IMultiTenant` with a
> `TenantId` property, a `Status` (`ContractStatus`, defaults to `Draft`), and
> `Activate`/`Expire`/`Terminate` guarded transition methods that throw
> `BusinessException("LegalTech:Contract:InvalidStatusTransition")` on an invalid
> transition. The EF Core suite passes 9/9 after reconciliation.

This report documents the implementation of the Module 01 foundation plan. All three
vertical slices are complete, the solution builds with 0 errors (pre-existing nullable
warnings `CS8618` on the `Contract` entity and `CS8604` in `OpenIddictDataSeedContributor`),
and the test suite passes.

---

## Acceptance — Definition of Done

| # | Criterion | Status |
|---|-----------|--------|
| 1 | Slice 1 placeholders retired, bounded-context folders created | Done |
| 2 | ContractStatus + MetadataEntry in Domain.Shared; permission tree has 7 groups + baseline roles seeded | Done |
| 3 | Contract entity defined, Contracts DbSet registered, Module01_Foundation migration applies cleanly | Done |
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
  `enum MetadataValueType { Text, Number, Date, Boolean }`.

### Shared constants relocation
- Moved `LegalTechConsts` from `src/Acme.LegalTech.Domain/` to
  `src/Acme.LegalTech.Domain.Shared/LegalTechConsts.cs` (namespace stays `Acme.LegalTech`).
- `AdminEmailDefaultValue` is hardcoded to `"admin@abp.io"` (the ABP default) because
  `IdentityDataSeedContributor.AdminEmailDefaultValue` lives in `Volo.Abp.Identity.Domain`,
  which `Domain.Shared` must not reference (layering rule).

### Permission tree (7 groups)
- `src/Acme.LegalTech.Application.Contracts/Permissions/LegalTechPermissions.cs`
  — `Groups` constants: `Contracts`, `Clauses`, `Reviews`, `Obligations`, `Reports`,
  `Files`, `Administration`, all prefixed `LegalTech.` (e.g. `LegalTech.Contracts`).
- `src/.../LegalTechPermissionDefinitionProvider.cs` adds each group with a localized
  display name (`Permission:Contracts`, ...) and feeds the duplicate-key guard.

### Localization (`en.json`)
- `Permission:Contracts`, `Permission:Clauses`, `Permission:Reviews`,
  `Permission:Obligations`, `Permission:Reports`, `Permission:Files`,
  `Permission:Administration`.
- `Enum:ContractStatus:0..3` -> Draft / Active / Expired / Terminated.

### Baseline roles
- `src/.../Permissions/LegalTechRoles.cs` — `HostAdmin`, `TenantAdmin`, `LegalOpsManager`,
  `LawyerReviewer`, `Auditor`.
- `src/Acme.LegalTech.Application/Permissions/LegalTechRoleDataSeedContributor.cs`
  — `IDataSeedContributor` that creates the five roles (host + per-tenant via current
  tenant context) during data seeding.

---

## Slice 3 — First persistence baseline

- **Contract entity** — `src/Acme.LegalTech.Domain/Contracts/Contract.cs`
  (`FullAuditedAggregateRoot<Guid>, IMultiTenant`, with a `TenantId` property so the
  model matches the `Module01_Foundation` migration; `Status` (`ContractStatus`,
  defaults to `Draft`) with guarded setter; `DocumentBlobName` (nullable) retained
  for the draft-era placeholder; `Activate`/`Expire`/`Terminate` guarded lifecycle
  methods that throw `BusinessException("LegalTech:Contract:InvalidStatusTransition")`
  on an invalid transition — `Activate` only from `Draft`, `Expire` only from
  `Active`, `Terminate` from `Draft`/`Active` (terminal once `Expired`/`Terminated`)).
  Constants in `ContractConsts.cs` (`MaxTitleLength`, `MaxCounterpartyNameLength`).
- **EF Core registration** — `DbSet<Contract> Contracts` and
  `builder.Entity<Contract>(b => b.ToTable("AppContracts").ConfigureByConvention());`
  in `LegalTechDbContext`.
- **Migration** — `20260714000000_Module01_Foundation` generated via `dotnet ef migrations add`
  and renamed to the `<yyyyMMddHHmmss>_<ModuleNN>_<Desc>` rollout convention. Creates the
  `AppContracts` table with full ABP convention columns (audit, multi-tenancy, soft-delete).

---

## Data rules and failure modes

### Duplicate-permission-key guard
- `src/.../Permissions/LegalTechPermissionGuard.cs`
  — `ThrowIfDuplicateKeys(IEnumerable<string> keys)` collects keys into a `HashSet` and
  throws `BusinessException("LegalTech:Permission:DuplicateKey")` on the first duplicate
  (checks the full key string). Invoked by the provider after all groups are added.

### Migration-drift guard
- `src/Acme.LegalTech.HttpApi.Host/HealthChecks/LegalTechMigrationDriftGuard.cs`
  — computes a deterministic SHA-256 hash of the current EF Core model (table + column
  names/types) and compares it to a stored hash in SettingManagement
  (`LegalTech.Migration.ModelHash`, defined in `LegalTechSettingDefinitionProvider`).
  First run stores the hash; subsequent runs throw `BusinessException` on mismatch.
  **Disabled in the `Development` environment** (`IWebHostEnvironment`), per the plan's
  mitigation, to avoid blocking frequent local model changes. On infrastructure errors it
  logs a warning instead of blocking startup.

---

## Observability

- `src/Acme.LegalTech.Application/Permissions/LegalTechPermissionHealthContributor.cs`
  — resolves `IPermissionDefinitionManager` at startup and logs
  `{GroupCount} permission groups, {PermissionCount} permissions, {RoleCount} baseline roles`.
  Wired into `LegalTechHttpApiHostModule.OnApplicationInitialization`.

---

## Tests and acceptance

All new tests live in **`test/Acme.LegalTech.EntityFrameworkCore.Tests`** (see note below).

| Test | File | Result |
|------|------|--------|
| 7 module groups are registered | `EntityFrameworkCore/LegalTechPermissionsTests.cs` | Pass |
| Duplicate-key guard throws on conflict | `EntityFrameworkCore/LegalTechPermissionsTests.cs` | Pass |
| Duplicate-key guard passes for unique keys | `EntityFrameworkCore/LegalTechPermissionsTests.cs` | Pass |
| BDD: permission tree complete & non-conflicting | `EntityFrameworkCore/FoundationScenarios.cs` | Pass |
| Contracts table configured (AppContracts, expected columns) | `EntityFrameworkCore/LegalTechModule01FoundationMigrationTests.cs` | Pass |
| Contracts table accepts rows | `EntityFrameworkCore/LegalTechModule01FoundationMigrationTests.cs` | Pass |

**Total EF Core suite:** 9 passed, 0 failed.

> Note on test placement: The plan suggested the permission/BDD tests in
> `Application.Tests`, but that fixture has no database store registered, so ABP's
> PermissionManagement module cannot initialize there (pre-existing limitation — the
> sample test in that project was never runnable without a DB). The EF Core test project
> has a working SQLite + OpenIddict-EF fixture, so the permission, BDD, and migration
> tests were placed there and all acceptance criteria are satisfied.

---

## Build verification

    dotnet build Acme.LegalTech.slnx -c Debug   ->  Build succeeded (0 errors; pre-existing nullable warnings)
    dotnet test ...EntityFrameworkCore.Tests   ->  9 passed, 0 failed

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
- `HttpApi.Host/HealthChecks/LegalTechMigrationDriftGuard.cs`
- `EntityFrameworkCore/Migrations/20260714000000_Module01_Foundation.cs[.Designer.cs]`
- Bounded-context `.gitkeep` folders (backend + Angular)
- Tests: `EntityFrameworkCore/LegalTechPermissionsTests.cs`, `EntityFrameworkCore/FoundationScenarios.cs`, `EntityFrameworkCore/LegalTechModule01FoundationMigrationTests.cs`

**Modified**
- `Application/LegalTechApplicationMappers.cs` (placeholder removed)
- `Domain.Shared/Localization/LegalTech/en.json` (keys)
- `Application.Contracts/Permissions/LegalTechPermissionDefinitionProvider.cs`
- `Domain/Settings/LegalTechSettingDefinitionProvider.cs` (model-hash setting)
- `EntityFrameworkCore/EntityFrameworkCore/LegalTechDbContext.cs` (DbSet + config)
- `HttpApi.Host/Acme.LegalTech.HttpApi.Host.csproj` (added `Volo.Abp.PermissionManagement.Domain`, `Volo.Abp.SettingManagement.Domain`)
- `HttpApi.Host/LegalTechHttpApiHostModule.cs` (wired drift guard + health contributor)

**Removed**
- `Domain/LegalTechConsts.cs` (moved to Domain.Shared)
