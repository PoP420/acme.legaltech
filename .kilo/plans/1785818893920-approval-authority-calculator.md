# Plan 7: Approval Authority Calculator & Tier Escalation

## Objective
Build a "what-if" approval authority calculator on the contract detail page where legal ops can input a proposed variation amount and instantly see which authority tier applies, whether NEDA/President approval is required, and if the variation exceeds the allowable limit.

## Backend Status
- `ContractAppService.GetApprovalAuthorityAsync(id, amount)` exists
- Returns `ApprovalAuthorityResultDto`: `authorityTitle`, `requiresNedaReview`, `requiresPresident`, `allowableVariationPercent`, `lastApprovalAuthorityTitle`, `lastApprovalRequiresNeda`, `lastApprovalRequiresPresident`
- `ContractAppService.GetContractComplianceAsync(id)` exists and includes `CurrentAuthority`
- **Gap:** No endpoint to compute authority for an arbitrary amount without persisting. The existing endpoint is read-only and works perfectly for this use case.

## Frontend Scope

### Task 1: Add Approval Calculator to Contract Detail
**File:** `angular/src/app/contracts/contract-detail.component.ts`

**New state:**
```typescript
proposedAmount: number | null = null;
approvalResult: ApprovalAuthorityResultDto | null = null;
computingAuthority = false;
```

**UI (insert after Current Approval Authority section, or replace it with interactive calculator):**
```html
<div class="card mb-4" *ngIf="contract">
  <div class="card-header"><h4>Approval Authority Calculator</h4></div>
  <div class="card-body">
    <div class="row g-2 mb-3">
      <div class="col-md-4">
        <label class="form-label">Proposed Variation Amount</label>
        <input type="number" step="0.01" class="form-control" [(ngModel)]="proposedAmount" placeholder="0.00" />
      </div>
      <div class="col-md-2 d-flex align-items-end">
        <button class="btn btn-primary" (click)="computeAuthority()" [disabled]="proposedAmount === null || computingAuthority">
          {{ computingAuthority ? 'Computing...' : 'Compute' }}
        </button>
      </div>
    </div>

    <div *ngIf="approvalResult" class="row g-3">
      <div class="col-md-6">
        <div class="card border-primary">
          <div class="card-body">
            <h5 class="card-title">New Authority</h5>
            <dl class="row">
              <dt class="col-sm-6">Authority</dt>
              <dd class="col-sm-6">{{ approvalResult.authorityTitle }}</dd>
              <dt class="col-sm-6">NEDA Review</dt>
              <dd class="col-sm-6">{{ approvalResult.requiresNedaReview ? 'Yes' : 'No' }}</dd>
              <dt class="col-sm-6">President Approval</dt>
              <dd class="col-sm-6">{{ approvalResult.requiresPresident ? 'Yes' : 'No' }}</dd>
              <dt class="col-sm-6">Allowable Variation</dt>
              <dd class="col-sm-6">{{ approvalResult.allowableVariationPercent }}%</dd>
            </dl>
          </div>
        </div>
      </div>
      <div class="col-md-6" *ngIf="approvalResult.lastApprovalAuthorityTitle">
        <div class="card border-secondary">
          <div class="card-body">
            <h5 class="card-title">Last Approved Authority</h5>
            <dl class="row">
              <dt class="col-sm-6">Authority</dt>
              <dd class="col-sm-6">{{ approvalResult.lastApprovalAuthorityTitle }}</dd>
              <dt class="col-sm-6">NEDA Review</dt>
              <dd class="col-sm-6">{{ approvalResult.lastApprovalRequiresNeda ? 'Yes' : 'No' }}</dd>
              <dt class="col-sm-6">President Approval</dt>
              <dd class="col-sm-6">{{ approvalResult.lastApprovalRequiresPresident ? 'Yes' : 'No' }}</dd>
            </dl>
          </div>
        </div>
      </div>
    </div>

    <div class="alert alert-warning mt-3" *ngIf="approvalResult && contract.contractValue && proposedAmount">
      {{
        (proposedAmount / contract.contractValue * 100) >= approvalResult.allowableVariationPercent
          ? 'Warning: Proposed variation exceeds allowable limit for this authority tier.'
          : 'Proposed variation is within allowable limits.'
      }}
    </div>
  </div>
</div>
```

**Component method:**
```typescript
computeAuthority(): void {
  if (!this.contract || this.proposedAmount === null) return;
  this.computingAuthority = true;
  this.contractService.getApprovalAuthority(this.contract.id, this.proposedAmount).subscribe({
    next: result => { this.approvalResult = result; },
    error: () => { alert('Failed to compute authority.'); },
    complete: () => { this.computingAuthority = false; },
  });
}
```

### Task 2: Add Variation Limit Warning to Add Variation Form
**File:** `angular/src/app/contracts/contract-detail.component.ts`

In the variation orders section, when user enters an amount:
- Compute `(amount / contractValue * 100)` in real-time
- Compare against `currentAuthority.allowableVariationPercent`
- Show inline warning: "This variation exceeds the allowable X% limit for current authority tier"
- Disable "Add Variation" button if limit exceeded

### Task 3: Enhance Current Authority Display
Keep the existing `Current Approval Authority` card but make it read from `contract.currentAuthority` (already populated by `GetAsync`). Display:
- Authority title
- NEDA/President flags
- Allowable variation percent

### Task 4: Add Tier History Table
**New state:** `tierHistory: ApprovalAuthorityResultDto[] = []`
Since backend doesn't have a tier history endpoint, for MVP, show only the last approved authority from `currentAuthority.lastApprovalAuthorityTitle`. Add a note: "Tier history requires additional backend endpoint."

## Backend Work Required (out of scope)
1. **Tier history endpoint:** `GET /api/app/contract/approval-tier-history/{id}` returning all historical authority tiers for the contract.
2. **Simulation endpoint:** Already exists (`GetApprovalAuthorityAsync`). No changes needed.
3. **Variation limit pre-check:** Currently enforced server-side in `AddVariationOrderAsync`. Frontend pre-check is advisory only.

## Validation
- `ng build --configuration production` passes
- Contract detail loads with current authority card
- User enters proposed amount → clicks Compute → results appear
- Warning shows when variation exceeds allowable percent
- Add Variation form shows real-time limit warning
