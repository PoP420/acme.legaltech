# Addendum: Review Redlines — PH Government Roles & Rules for Contract Data

**Applies to:** `1785483468462-ph-government-contract-compliance-plan.md`
**Type:** Pre-implementation review notes — resolve before an implementation agent executes the plan.

---

## R1. `ContractValue` nullability vs. amendment-guard logic

**Plan section:** §3 Design Decisions → Data-model additions → `Contract` extension (`decimal? ContractValue`)

**Issue:** `ComputeApprovingAuthority(amount)` and `AddVariationOrder(amountDelta)` both require a concrete base value to compute tier/percentage. `ContractValue` is nullable "for non-monetary contracts," but the plan doesn't define behavior when `AddVariationOrder` is called on a contract with `ContractValue == null`.

**Redline:** Add an explicit guard in `AddVariationOrder`:
```
if (ContractValue is null)
    throw new BusinessException("LegalTech:Contract:ValueRequiredForVariation");
```
Add this as an invariant in §3 and as a new BDD scenario in task 9.

---

## R2. `AuthorizedSignatory` uniqueness constraint is under-specified

**Plan section:** §4 task 9(c) — "signatory role uniqueness per role (`AuthorizedSignatory` unique constraint)"

**Issue:** Real BAC composition has multiple people holding the same functional role concurrently (Chair + Members all "approve"). A blanket per-role uniqueness constraint will reject valid BAC compositions.

**Redline:** Scope the uniqueness constraint explicitly to `GovernmentSignatoryRole.AuthorizedSignatory` only (the single binding signature), not to `ApprovedBy` or `NotedBy`, which are legitimately multi-value. Update the invariant description in §3 and the test scenario in task 9 to name the specific enum value the constraint applies to.

---

## R3. Anti-splitting rule (RA 12009 §39) cited but not designed

**Plan section:** §1 In scope — lists RA 12009 §39 (anti-splitting) as governing law; §3 has no corresponding invariant.

**Issue:** Every other cited rule (NEDA thresholds, amendment tiers, retention) has a design decision. Anti-splitting doesn't, and it isn't listed in §8 Out of scope either — so a reviewer can't tell if it was deliberately deferred or missed.

**Redline:** Either (a) add a minimal invariant/detection hook (e.g., flag same-counterparty contracts within a rolling window that cumulatively approach a threshold), or (b) move the §39 citation out of "In scope" governing law and into §8 Out of scope with a one-line rationale ("detection requires cross-contract analysis outside this aggregate; tracked separately").

---

## R4. `RetentionUntil` triggering event needs verification

**Plan section:** §3 Design Decisions — `RetentionUntil` (computed = `EffectiveDate + RetentionPeriod`)

**Issue:** RA 12009 §38's 5-year retention period is typically measured from contract completion / final payment / audit clearance — not `EffectiveDate`. If this is wrong, `RetentionUntil` becomes an incorrect compliance/COA-audit field shipped with false confidence.

**Redline:** Before implementation, confirm the statute's actual triggering event. If it isn't `EffectiveDate`, either compute from `ExpirationDate`/a new `CompletionDate` field, or explicitly document `RetentionUntil` as an estimate pending legal confirmation (matching the existing "reference only, not legal advice" comment convention in §5 Risks).

---

## R5. No stated interaction with Module 03 (AI document extraction)

**Plan section:** §7 Open questions (candidate addition)

**Issue:** Module 03 already auto-extracts `Title`, `Counterparty`, `EffectiveDate`, `ExpirationDate`, `Category` from uploaded documents via Azure Document Intelligence. The new gov fields (`ContractValue`, `DocumentNumber`, signatories) have no stated relationship to that pipeline — are they manual-entry-only in v1, or should extraction attempt to populate them too?

**Redline:** Add as open question #4: "Should Module 03's extraction provider attempt to populate `ContractValue`/`DocumentNumber`/signatory names from `prebuilt-contract` output, or are these manual-entry-only in v1?" *(Recommended: manual-entry v1, extraction as a follow-up — keeps this slice's scope from expanding into the AI-Assist orchestration work Module 03 already flagged as not-started.)*

---

## R6. Migration naming should match established convention exactly

**Plan section:** §4 task 3

**Issue:** Plan uses placeholder `_*_Module04_GovCompliance`. Module 03's actual migration was `20260730010326_Module03_DocumentExtraction` — a full timestamp prefix, not a wildcard.

**Redline:** Task 3 should read: migration named `<yyyyMMddHHmmss>_Module04_GovCompliance` generated via `dotnet ef migrations add`, matching the exact `ModuleNN_Description` suffix convention already in use.

---

## R7. Test count target should be a hard number, not "+N"

**Plan section:** §4 task 9, §6 Validation plan

**Issue:** Module 03 shipped with a green build and zero new tests — explicitly flagged in that module's own report as a gap ("green build hides unverified behavior"). Module 04 touches compliance-critical logic (approval tiers, variation limits); repeating an open-ended test target risks repeating that gap.

**Redline:** Replace "+N" with a committed minimum, e.g.: "Module 04 suite: minimum 6 tests covering (a) tier computation by value boundary, (b) variation ≤5%, (c) variation >5%≤10% requires HoPE, (d) variation >10% rejected, (e) `AuthorizedSignatory` uniqueness, (f) `ContractValue == null` variation guard (see R1)." Foundation suite stays 16/16.

---

## R8. `RequiresPresident` flag has no enforcing invariant

**Plan section:** §3 — `GovernmentApprovalTier` (`RequiresPresident` field)

**Issue:** The field is stored but nothing in `ApplyApproval` or elsewhere consumes it. As written it's decorative.

**Redline:** Either wire `RequiresPresident` into `ApplyApproval` (e.g., require a presidential signatory record before allowing approval when the tier flag is set), or explicitly note in §3 that `RequiresPresident`/`RequiresNedaReview` are informational-only in v1 (surfaced for manual process, not system-enforced) and revisit enforcement in a follow-up slice.

---

## Summary — before handing to implementation agent

| # | Item | Action needed |
|---|------|----------------|
| R1 | `ContractValue` null + variation guard | Add invariant + test |
| R2 | Signatory uniqueness scope | Narrow to `AuthorizedSignatory` only |
| R3 | Anti-splitting (§39) | Design or move to Out of scope |
| R4 | `RetentionUntil` trigger date | Verify statute, adjust computation |
| R5 | Module 03 interaction | Add as open question #4 |
| R6 | Migration naming | Match exact timestamp convention |
| R7 | Test count | Commit to hard minimum, not "+N" |
| R8 | `RequiresPresident` enforcement | Wire into `ApplyApproval` or mark informational-only |
