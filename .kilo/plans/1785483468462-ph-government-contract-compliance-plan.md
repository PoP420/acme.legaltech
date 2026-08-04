# Plan: PH Government Roles & Rules for Contract Data

**Plan ID:** 1785483468462-ph-government-contract-compliance-plan
**Goal:** Extend the `Contract` aggregate so government contracts store the
Philippine Government **roles** (signatory/approval parties) and honor the PH
government **rules** for contract managing, while fitting the existing ABP
layered monolith conventions.

> **This is a planning artifact. No source is changed until an implementation
> agent executes it.**

---

## 1. Goal & Scope

Enable Philippine Government contracts to be stored with the roles and rules
prescribed for government documents, so the CLM can record **who** is authorized
to sign/approve and **what** the legal rules (approval tier, amendment limits,
document series, classification, retention) require.

- **In scope (v1):** Philippine **procurement contracts** governed by
  - **RA 12009** (New Government Procurement Act, 2024) — governing principles;
    BAC/observer roles; PhilGEPS/ABC (§4). (Anti-splitting §39 is **out of scope**
    — see §8.)
  - **RA 9184** + **2016 Revised IRR** (as updated 2024) — Head of Procuring
    Entity (HoPE), Bids & Awards Committee (BAC) composition & functions,
    BAC Secretariat, Technical Working Group (TWG), End-User/Implementing Unit.
  - **EO 109-A** — signature/approval authority to bind the Government;
    contracts ≥ ₱300M alternate-method needing NEDA review; contracts < ₱300M
    HoPE may delegate authority in writing; contract execution within 10 days
    of Notice of Award.
  - **Annex D (IRR Contract Implementation Guidelines)** — "amendment to order"
    tier: ≤5% cumulative by duly authorized rep, >5%≤10% by HoPE, >10% treated
    as a new procurement.
- **Out of scope (v1):** MOAs/MOUs, loan agreements, lease contracts, goods-only
  procurements without a contract, anti-splitting (RA 12009 §39 — needs
  cross-contract analysis, tracked separately), the pre-award bidding/
  post-qualification workflow, PhilGEPS integration, COA auditing exports,
  and the Module 03 AI-Assist orchestration layer.

### Assumptions (recommended; correct if wrong)
1. "Philippines Government Roles for documents" = the **signatory/approval
   roles** on government contracts, plus the **procurement-committee roles**
   (HoPE, BAC Chair/Member, TWG, End-User) needed for audit traceability.
2. Legal amount thresholds (₱300M NEDA review, 5%/10% amendment tiers) are
   encoded as **configurable reference data**, not hard-coded law — the owning
   agency confirms exact figures from its IRR. We ship sane defaults.
3. Document numbering/series and classification (Unclassified / Confidential /
   Strictly Confidential / FOUO per the official-documents classification
   guidelines) are captured as contract attributes.
4. **Retention** is provisioned (v1 default = 5 years from `EffectiveDate`), but
   the authoritative triggering event (contract completion / final payment /
   audit clearance) per the COA Government Records Manual / Records Disposal
   Manual is **to be confirmed with legal before enforcement** (reference only;
   not legal advice).

---

## 2. Context from codebase
- `Domain/Contracts/Contract.cs` — `FullAuditedAggregateRoot<Guid>, IMultiTenant`;
  fields: Title, CounterpartyName, DocumentBlobName, Status (`ContractStatus`
  enum in `Domain.Shared/Common/ContractStatus.cs`), EffectiveDate, ExpirationDate,
  OwnerUserId, Category, RiskBaseline; lifecycle `Activate/Expire/Terminate`.
- `Domain/Contracts/ContractConsts.cs` — max-length constants (pattern to follow).
- EF Core: `LegalTechDbContext.cs`, migrations under
  `EntityFrameworkCore/Migrations/` (convention `*_<yyyyMMddHHmmss>_ModuleNN_*`
  e.g. `20260730010326_Module03_DocumentExtraction`), `App` table prefix
  (`LegalTechConsts.DbTablePrefix`).
- Permissions: `LegalTechPermissions.cs` (7 groups + sub-permissions),
  `LegalTechPermissionDefinitionProvider.cs`, duplicate-key guard
  `LegalTechPermissionGuard.ThrowIfDuplicateKeys`. Roles seeded by
  `LegalTechRoleDataSeedContributor.cs`; baseline roles in `LegalTechRoles.cs`.
- Localization: `Domain.Shared/Localization/LegalTech/en.json` (keys like
  `Permission:…`, `Enum:ContractStatus:…`).
- Tests live in `test/Acme.LegalTech.EntityFrameworkCore.Tests/` (SQLite + OpenIddict
  fixture); current suite = **16 passed**. Build is green (0 errors/0 warnings).
- Observability: `LegalTechPermissionHealthContributor.cs` registered in
  `LegalTechHttpApiHostModule.OnApplicationInitialization`.
- Architecture rules: tenant checks explicit at app-service/repo boundaries;
  long-running work via background jobs; domain free of infra deps.

---

## 3. Design Decisions
- **Roles** modeled as a `ContractSignatory` value object (signatories follow the
  contract's lifecycle and tenant, so an owned collection on `Contract`, not a
  separate aggregate).
  - Members: `Role` (`GovernmentSignatoryRole` enum), `PartyType`
    (`GovernmentUnit` | `Individual` | `External`), `PartyId` (system identity or
    free-text for external), `GovernmentAgency` (free text / org unit),
    `SignedOn` (DateTime), `Capacity` (free text, e.g. "Head, Procurement Service"),
    `Order` (display order).
- **Rules** encoded as invariants on `Contract`:
  - `ComputeApprovingAuthority(amount)` returns the required approving-authority
    tier from seeded reference data `GovernmentApprovalTier` (AmountFrom, AmountTo,
    AuthorityTitle, RequiresNedaReview, RequiresPresident).
  - **R1 — `AddVariationOrder(amountDelta)` null-value guard:**
    `if (ContractValue is null) throw new BusinessException("LegalTech:Contract:ValueRequiredForVariation");`
    Only after this guard does the cumulative-amt vs. tier-percentage check
    (`LegalTech:Contract:ApprovedVariationLimitExceeded`) apply.
  - **R2 — signatory uniqueness:** a unique constraint on
    `GovernmentSignatoryRole.AuthorizedSignatory` only (the single binding
    signature). `ApprovedBy` / `NotedBy` remain multi-value (legitimately shared
    across BAC members). Enforce in `AddSignatory`.
  - `DocumentNumber`, `DocumentSeries` (e.g. "Series of 2026"), `DocumentYear`,
    `Classification` (`DocumentClassification` enum) added to `Contract`.
  - **R4 — `RetentionUntil` (provisional):** `EffectiveDate + 5 years` as a
    provisional default. The statutory triggering event is **not confirmed**;
    document as informational/estimate, "reference only; not legal advice", with
    a code note to switch to `CompletionDate`/audit-clearance once legal confirms.
  - **R8 — `RequiresPresident` / `RequiresNedaReview`:** informational-only in v1
    (surfaced on the tier for manual process routing; not system-enforced).
    Full enforcement (Presidential signatory workflow + notifications) is a
    follow-up slice. `ApplyApproval` therefore records the resolved tier and the
    required authority **title** but does not yet block approval on these flags.
- **Permissions** extend the existing `Contracts` group (no new top-level group,
  to respect the 7-group baseline): `Contracts.ManageSignatories`,
  `Contracts.Amend`, `Contracts.Terminate`, `Contracts.ViewGovFields`.
- **Multi-tenancy:** signatories and variation orders are tenant-owned; approval
  tiers are reference data seeded once per tenant.
- **Localization keys** added for each role, classification, authority tier,
  and error code under `en.json`.

### Data-model additions
- `Domain.Shared/Common/GovernmentSignatoryRole.cs` — enum: `PreparedBy`,
  `ReviewedBy`, `EndorsedBy`, `ApprovedBy`, `AuthorizedSignatory`, `NotedBy`.
- `Domain.Shared/Common/DocumentPartyType.cs` — enum: `GovernmentUnit`,
  `Individual`, `External`.
- `Domain.Shared/Common/DocumentClassification.cs` — enum: `Unclassified`,
  `ForOfficialUseOnly`, `Confidential`, `StrictlyConfidential`.
- `Domain.Shared/Contracts/ContractGovConsts.cs` — max-length constants for new
  text fields (`MaxDocumentNumberLength`, `MaxPartyNameLength`, `MaxCapacityLength`).
- `Domain/Contracts/ContractSignatory.cs` — value object above.
- `Domain/Contracts/VariationOrder.cs` — `Entity<Guid>, IMultiTenant`
  (OrderId, ContractId, Description, Amount, CumulativeAmount, ApprovedBy,
  ApprovedOn).
- `Domain/Contracts/GovernmentApprovalTier.cs` — reference-data aggregate root
  (AmountFrom, AmountTo, AuthorityTitle, RequiresNedaReview, RequiresPresident,
  AllowableVariationPercent).
- Extend `Contract`: `DocumentNumber`, `DocumentSeries`, `DocumentYear`,
  `Classification`, `RetentionUntil`, `decimal? ContractValue` (for tier/variation
  computation; nullable for non-monetary contracts),
  `IReadOnlyCollection<ContractSignatory> Signatories`, `Status` unchanged.
  Invariants: `AddSignatory`, `AddVariationOrder`, `ComputeApprovingAuthority`,
  `ApplyApproval`.

---

## 4. Ordered task list

1. **Shared primitives** (`Domain.Shared`): add the 3 enums, `ContractGovConsts`,
   and localization keys in `en.json` (`GovernmentSignatoryRole:*`, `DocumentClassification:*`,
   `ApprovalAuthority:*`, error codes `LegalTech:Contract:ValueRequiredForVariation`,
   `LegalTech:Contract:ApprovedVariationLimitExceeded`, `LegalTech:Contract:GovSignatoryNotFound`).
2. **Domain model** (`Domain/Contracts`): add `ContractSignatory`, `VariationOrder`,
   `GovernmentApprovalTier` entities; extend `Contract` with new fields +
   invariants (`AddSignatory` with AuthorizedSignatory uniqueness, `AddVariationOrder`
   with the R1 null guard + R2 variation-tier check, `ComputeApprovingAuthority`,
   `ApplyApproval` recording tier title). Keep `IMultiTenant`; follow
   `LegalTechPermissionGuard` error-code conventions.
3. **EF Core** (`EntityFrameworkCore`): register `DbSet<VariationOrder>`,
   `DbSet<GovernmentApprovalTier>`; configure `Contract.Signatories` as an owned
   collection with a unique index on `(ContractId, Role)` for
   `AuthorizedSignatory`; add migration
   `<yyyyMMddHHmmss>_Module04_GovCompliance` (via `dotnet ef migrations add`,
   matching the `ModuleNN_Description` suffix convention) and update the snapshot.
4. **Permissions** (`Application.Contracts`): add the 4 `Contracts.*` sub-permissions
   to `LegalTechPermissions.cs`, register in `LegalTechPermissionDefinitionProvider.cs`,
   update `LegalTechRoleDataSeedContributor.cs` grants; run the duplicate-key guard
   test (`LegalTechPermissionsTests.cs`).
5. **Application services** (`Application`): `ContractAppService` gains
   `AddSignatoryAsync`, `AddVariationOrderAsync`, `GetApprovalAuthorityAsync`,
   `GetContractComplianceAsync`; inject `IRepository<VariationOrder>`,
   `IRepository<GovernmentApprovalTier>`. Extend mappers
   (`LegalTechApplicationMappers.cs`) and DTOs in `Application.Contracts/Contracts/`
   (signatory/VariationOrder/ApprovalTier DTOs).
6. **HTTP API** — covered by existing app-service exposure (ABP routeable); no new
   controller needed unless a custom route is required (keep `ControllerBase` parity
   with `ContractDocumentController` style — optional).
7. **Data seed** — `Domain/Seeding/` contributor seeds default PH approval tiers
   (e.g. <₱500K→Agency Head, ₱500K–₱5M→, ≥₱300M→NEDA review flag, ≥₱4B→President
   flag) and example signatory roles per the BAC composition (HoPE, BAC Chair,
   TWG, End-User). **Legal: confirm exact threshold amounts from the agency IRR.**
8. **Localization** — ensure new `Permission:*` and role/enum keys resolve.
9. **Tests** — add `LegalTechModule04GovComplianceTests.cs`. **Committed minimum
   = 6 tests** (no more "+N"), all on the SQLite+OpenIddict fixture:
   a. tier computation by value boundary (e.g. ₱499,999 vs ₱500,000 vs ₱300M vs
      ₱4B resolves to the right `AuthorityTitle`);
   b. variation ≤5% succeeds (auto-approved by delegated authority);
   c. variation >5% and ≤10% resolves tier requiring HoPE-level approval;
   d. variation >10% is rejected with `LegalTech:Contract:ApprovedVariationLimitExceeded`;
   e. `AuthorizedSignatory` uniqueness enforced (second insert throws, by role);
   f. **R1:** `AddVariationOrder` on `ContractValue == null` throws
      `LegalTech:Contract:ValueRequiredForVariation`.
   Foundation suite stays **16/16**.
10. **Observability** — extend `LegalTechPermissionHealthContributor` log line to
    include the 4 new permission counts; emit audit events for
    `AddVariationOrder`/`AddSignatory` (PMR-relevant) via ABP audit logging.
11. **Build + test verification:** `dotnet build Acme.LegalTech.slnx -c Debug`
    (0 errors, 0 warnings) then `dotnet test ...EntityFrameworkCore.Tests`
    (16 foundation + 6 Module 04).

---

## 5. Risks & Mitigations
| Risk | Mitigation |
|---|---|
| PH legal amounts/thresholds change by IRR | Default tiers as seeded reference data editable per tenant; "validate amounts against the agency's IRR — not legal advice" code comment |
| Signatory roles differ per document type | Role as enum + free-text `Capacity`; restrict enum to the common government roles in v1 |
| `RetentionUntil` triggering event wrong | Provisional from `EffectiveDate` +5y, clearly marked "reference only; not legal advice"; revisit with legal/CompletionDate |
| Signatory uniqueness too broad | R2: scoped to `AuthorizedSignatory` only |
| Variation on non-monetary contracts | R1 explicit null-value guard |
| Migration drift guard | Reuse `LegalTechMigrationDriftGuard`; disabled in Development per existing convention |
| Permission-key collisions | Reuse `LegalTechPermissionGuard.ThrowIfDuplicateKeys`; new keys covered by existing uniqueness test |
| `RequiresPresident`/`RequiresNedaReview` decorative | R8: informational-only in v1; wire enforcement in a follow-up slice |
| `ContractValue` null default on existing contracts | New field is nullable; existing rows unaffected; variation ops require it and fail loudly |

---

## 6. Validation plan
- Build: `dotnet build Acme.LegalTech.slnx -c Debug` → 0 errors, 0 warnings.
- Foundation tests unchanged: **16/16**.
- New Module 04 suite: **6 tests**, all pass (see §4 task 9).
- Seeded approval tiers resolve for sample amounts:
  ₱100K→Agency Head tier, ₱5M→NEDA-review tier, ₱4B→President-flagged tier.
- `AddVariationOrder` throws `LegalTech:Contract:ValueRequiredForVariation`
  when `ContractValue == null`; throws `LegalTech:Contract:ApprovedVariationLimitExceeded`
  when over the tier's allowable percentage.
- `AuthorizedSignatory` uniqueness enforced (second insert rejected).
- `Contract` JSON serialization includes `DocumentNumber`, `Classification`,
  `ContractValue`, `Signatories`, `RetentionUntil`.

---

## 7. Open questions (owner decision)
1. **Document-type scope** — v1 targets procurement contracts only. Should
   MOAs and loan/lease agreements be included in the same slice?
   *(Recommended: defer to a follow-up slice.)*
2. **Authority-tier amounts** — confirm exact PH-government threshold amounts per
   the agency's IRR (defaults proposed in `GovernmentApprovalTier` seed).
3. **UI treatment** — expose signatories/variance in the Angular contract detail
   view (ties to open `legendary-leopon` contract-detail work) or API-only for v1?
   *(Recommended: API + data first; UI in a follow-up.)*
4. **Module 03 interaction** — should Module 03's extraction provider attempt to
   populate `ContractValue`/`DocumentNumber`/signatory names from
   `prebuilt-contract` output, or are these manual-entry-only in v1?
   *(Recommended: manual-entry v1; extraction population as a follow-up — keeps
   this slice from expanding into the AI-Assist orchestration Module 03 flagged as
   not-started.)*
5. **Retention trigger** — confirm the COA/Government Records Manual triggering
   event for contract records (provisional default = `EffectiveDate + 5y`).

---

## 8. Out of scope
- Anti-splitting (RA 12009 §39) — requires cross-contract analysis outside this
  aggregate; tracked separately.
- Procurement bidding/post-qualification (pre-award), PhilGEPS integration,
  COA auditing exports, full document-classification mandatory labeling beyond the
  enum, and the Module 03 AI-Assist ingestion/suggestion layer (see
  `docs/modules-reports/implemented-slices-plans/03-document-extraction.md`).
- Enforcement of `RequiresPresident`/`RequiresNedaReview` (informational-only v1).
