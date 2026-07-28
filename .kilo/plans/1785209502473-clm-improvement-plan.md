# Acme LegalTech — Improvement & Feature Plan

## 1. Current State Assessment

### What is implemented (Phase A + partial Phase B)

| Area | Status | Details |
|------|--------|---------|
| ABP scaffolding & multi-tenancy | Done | Layered monolith, `IMultiTenant` on entities |
| Contract aggregate (domain) | Done | `Contract`, `ContractDocumentVersion`, `CounterpartyReference`, `ContractTag` |
| Contract CRUD + lifecycle | Done | `ContractAppService` with Create/Update/Delete/ChangeStatus |
| Document versioning | Done | `ContractDocumentAppService` with upload/download/versioning |
| EF Core persistence | Done | `LegalTechDbContext`, 3 migrations (Initial, Module01, Module02) |
| Blob storage | Done | `ContractsBlobContainer` configured |
| Permission tree (Contracts) | Partial | 5 permissions defined (Default, Create, Edit, ChangeStatus, AttachDocument) |
| Role seed data | Partial | 5 roles defined; only Contracts permissions granted |
| Angular contracts module | Partial | List, create, detail routes; basic service |
| OpenIddict seed | Done | LegalTech scope + Angular app client |
| Localization | Partial | 15 locales present; English JSON updated 2026-07-15 |
| Settings | Minimal | `MigrationModelHashSettingName` only |
| Tests (EF Core) | Partial | Foundation + ContractIntake migration tests; permission tests |

### What is NOT implemented (all placeholders)

| Module | Doc Reference | Implementation Status |
|--------|---------------|----------------------|
| Clauses & Playbooks | `03-clause-library-playbooks.md` | All `.gitkeep` — no domain, app, or UI code |
| Review Workflow | `04-review-workflow-negotiations.md` | All `.gitkeep` |
| Obligations & Renewals | `05-obligations-renewals.md` | All `.gitkeep` |
| Search, Reports, Dashboards | `06-search-reports-dashboards.md` | All `.gitkeep` |
| SaaS Administration & Packaging | `07-saas-administration-packaging.md` | All `.gitkeep` |
| File & Evidence Management | `08-file-evidence-management.md` | All `.gitkeep` |
| Identity & OAuth Administration | `09-identity-oauth-administration.md` | All `.gitkeep` |
| AI Assist Foundation | `10-ai-assist-foundation.md` | All `.gitkeep` |

### Gaps in existing implementation

1. **Permission tree incomplete** — Only `Contracts` group exists; `Clauses`, `Reviews`, `Obligations`, `Reports`, `Files`, `Administration` groups are declared in `LegalTechPermissions.Groups.All` but never defined in `LegalTechPermissionDefinitionProvider`.
2. **Angular UI is contracts-only** — All other module directories (`clauses`, `reviews`, `obligations`, `reports`, `files`, `administration`) contain only `.gitkeep`.
3. **No SaaS administration** — No `SubscriptionPlan`, `PlanFeatureToggle`, or `TenantProvisioningProfile` entities or services.
4. **No file management** — `Files` directories in Domain and Application are `.gitkeep`; no `ManagedFile` aggregate.
5. **No identity/OAuth admin UI** — OpenIddict runtime exists but no host-side management workflows.
6. **No AI assist infrastructure** — No ingestion jobs, extraction, or suggestion entities.
7. **No reporting/export** — No `ReportJob` entity or export endpoints.
8. **No review workflow** — No `ReviewCase`, `ReviewTask`, `ApprovalStep`, or `EscalationEvent`.
9. **No obligations/renewals** — No `ContractObligation`, `RenewalSchedule`, or reminder logic.
10. **Test coverage is thin** — Only migration tests and a sample app service test exist; no business logic unit tests.
11. **Database connection errors** — `Logs/logs.txt` shows migration connection failures (empty server/connection string).
12. **Outdated plans.md** — The root `plans.md` references .NET 8, Docker Compose, and a different project structure.

## 2. Priority Features to Improve or Add

### Priority 1 — Complete the Permission Tree and Role Matrix

**Why:** All other modules cannot function without their permission groups and role assignments defined. The current `LegalTechPermissionDefinitionProvider` only defines Contracts permissions, and `LegalTechRoleDataSeedContributor` only grants Contracts permissions to roles.

**What to do:**
- Add `Clauses`, `Reviews`, `Obligations`, `Reports`, `Files`, `Administration` permission groups to `LegalTechPermissionDefinitionProvider`
- Define sub-permissions for each group per the implementation docs (e.g., `Clauses.Default`, `Clauses.Manage`, `Playbooks.Default`, `Playbooks.Manage`, etc.)
- Update `LegalTechRoleDataSeedContributor` to grant appropriate permissions per role per module
- Add `LegalTechPermissionGuard` checks for duplicate keys across all groups

### Priority 2 — Implement Clauses & Playbooks Module (FR-CL)

**Why:** This is the highest-value differentiator for a CLM platform — policy-driven review guidance. It is Phase B, Slice 1 of the implementation plan.

**What to do:**
- Domain: `ClauseTemplate`, `PlaybookProfile`, `ClauseTaxonomy`, `PlaybookRule`, `RuleSeverity` entities
- Application: CRUD services for clauses and playbooks, `PlaybookEvaluationService` for rule-based risk marking
- Application.Contracts: DTOs for clause templates, playbook profiles, evaluation results
- EF Core: DbSet registrations, mappings, indexes
- Angular: Clause list, playbook editor, contract comparison view
- Permissions: `Clauses.Default`, `Clauses.Manage`, `Playbooks.Default`, `Playbooks.Manage`, `Playbooks.Evaluate`

### Priority 3 — Implement Review Workflow Module (FR-RW)

**Why:** Core CLM workflow — without review and approval, contracts cannot move through the lifecycle.

**What to do:**
- Domain: `ReviewCase`, `ReviewTask`, `ApprovalStep`, `ReviewComment`, `EscalationEvent` entities
- Application: Review orchestration service, task assignment, approval progression, escalation
- Application.Contracts: DTOs for review cases, tasks, comments, escalation events
- EF Core: DbSet registrations, mappings
- Angular: Review queue, case detail, action controls (approve, request changes, escalate)
- Permissions: `Reviews.Default`, `Reviews.Assign`, `Reviews.Decide`, `Reviews.Escalate`, `Reviews.AuditView`

### Priority 4 — Implement Obligations & Renewals Module (FR-OR)

**Why:** Obligation tracking and renewal management are core CLM value propositions.

**What to do:**
- Domain: `ContractObligation`, `RenewalSchedule`, `ObligationReminder`, `CompletionEvidence` entities
- Application: Obligation CRUD, schedule generation, reminder logic, completion workflows
- Application.Contracts: DTOs for obligations, renewal schedules, reminders, evidence
- EF Core: DbSet registrations, mappings
- Angular: Obligation list, renewal workspace, dashboard tiles
- Permissions: `Obligations.Default`, `Obligations.Manage`, `Obligations.Complete`, `Renewals.Default`, `Renewals.Manage`

### Priority 5 — Implement Search, Reports & Dashboards (FR-SR)

**Why:** Operational visibility is required for legal ops managers and tenants.

**What to do:**
- Domain: `ReportJob` entity for async export tracking
- Application: Search endpoints with metadata filters, report export services (portfolio summary, obligations health, renewal pipeline, risk distribution)
- Application.Contracts: DTOs for search input, report results, export requests
- EF Core: Query models, indexes for search filters
- Angular: Dashboards by role with KPI cards, drill-down views, export buttons
- Permissions: `Reports.Default`, `Reports.Export`, `Dashboards.Default`, `Dashboards.ViewRisk`

### Priority 6 — Implement SaaS Administration & Packaging (FR-SA)

**Why:** Required for multi-tenant commercialization — plan-based feature entitlements and tenant provisioning.

**What to do:**
- Domain: `SubscriptionPlan`, `PlanFeatureToggle`, `TenantProvisioningProfile` entities
- Application: Tenant provisioning workflow, plan/feature gating, host admin services
- Application.Contracts: DTOs for plans, feature toggles, provisioning profiles
- EF Core: DbSet registrations, mappings
- Angular: Host admin UI — plan matrix, tenant provisioning status
- Permissions: `SaaSAdministration.Default`, `SaaSAdministration.Tenants`, `SaaSAdministration.PlanManagement`

### Priority 7 — Implement File & Evidence Management (FR-FM)

**Why:** Contracts and review processes require secure file handling with tenant isolation.

**What to do:**
- Domain: `ManagedFile` aggregate with metadata, ownership, retention profile
- Application: File upload/download/delete services with tenant boundary validation
- Application.Contracts: DTOs for file metadata, upload requests
- EF Core: DbSet registrations, mappings
- Angular: File picker, upload flows in contracts and review modules
- Permissions: `Files.Default`, `Files.Upload`, `Files.Download`, `Files.Delete`, `Files.ManageAll`

### Priority 8 — Implement Identity & OAuth Administration (FR-ID)

**Why:** Host-side OpenIddict management is needed for integration client governance.

**What to do:**
- Application: Host-side CRUD for OpenIddict applications and scopes
- Application.Contracts: DTOs for client applications, scope configurations
- Angular: Host admin UI — applications list/editor, scopes list/editor
- Permissions: `OpenIddictAdmin.Default`, `OpenIddictAdmin.Applications`, `OpenIddictAdmin.Scopes`, `OpenIddictAdmin.Secrets`

### Priority 9 — Implement AI Assist Foundation (FR-AI)

**Why:** Differentiator for the product, but depends on all prior modules being in place.

**What to do:**
- Domain: `IngestionJob`, `ExtractionSuggestion`, `RiskAssessmentSuggestion`, `SuggestionDecision` entities
- Application: Async ingestion pipeline, extraction adapter boundaries, suggestion workflow, human-in-the-loop review
- Application.Contracts: DTOs for jobs, suggestions, decisions
- EF Core: DbSet registrations, mappings
- Angular: AI review console with accept/reject/correct actions
- Permissions: `AIAssist.Default`, `AIAssist.RunJobs`, `AIAssist.ReviewSuggestions`, `AIAssist.ConfigureProviders`

### Priority 10 — Testing & Quality Infrastructure

**Why:** Current test coverage is thin; NFR-MAINT-001 requires automated tests for all major modules.

**What to do:**
- Add unit tests for domain entities (Contract lifecycle transitions, playbook evaluation, review workflow rules)
- Add integration tests for application services
- Add BDD scenarios per module (per implementation doc requirements)
- Add permission health checks and migration validation tests
- Add audit trail verification tests

## 3. Implementation Order

The implementation should follow the dependency chain:

1. **Permission tree completion** (blocks all other modules)
2. **Clauses & Playbooks** (Phase B, Slice 1 — independent of review/obligations)
3. **Review Workflow** (Phase B, Slice 2 — depends on contracts)
4. **Obligations & Renewals** (Phase B, Slice 3 — depends on contracts)
5. **Search, Reports & Dashboards** (Phase C, Slice 1 — depends on all Phase B modules)
6. **SaaS Administration** (Phase C, Slice 2 — independent but needed for packaging)
7. **File & Evidence Management** (Phase C, Slice 3 — depends on contracts)
8. **Identity & OAuth Administration** (Phase C, Slice 4 — host-only)
9. **AI Assist Foundation** (Phase D — depends on all prior modules)
10. **Testing & Quality** (throughout all phases)

## 4. Key Risks

| Risk | Mitigation |
|------|-----------|
| Permission key collisions across modules | Enforce `LegalTechPermissionGuard.ThrowIfDuplicateKeys` during provider definition |
| Tenant data leakage in new modules | Explicit tenant boundary checks in all app services and repositories |
| Expensive queries under growth | Indexed filters, pagination, async exports |
| AI false-positive suggestions | Confidence thresholds, human-in-the-loop review, audit trail |
| Plan changes breaking active tenants | Staged rollout, compatibility rules, feature flag gating |
| Migration failures in production | Rollback considerations, upgrade notes, staged migration rollout |

## 5. Validation Plan

- Each module must pass the Definition of Done from its implementation doc
- Permission discovery and assignment behavior validated
- Migrations apply cleanly on fresh database
- Test coverage includes unit, integration, and BDD scenarios per module
- Audit and operational logging validated
- Both product-level and implementation-level docs updated when scope changes