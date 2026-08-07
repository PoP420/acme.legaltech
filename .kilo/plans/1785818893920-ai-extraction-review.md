# Plan 3: AI Extraction Review & Accept/Reject Workflow

## Objective
Add a post-upload extraction review panel to `contract-detail.component.ts` where users can review AI-extracted metadata, accept or reject suggestions per field, and submit corrections. This implements FR-AI-003 (human-in-the-loop).

## Backend Status
- `ContractDocumentVersionDto` includes extraction fields:
  - `extractionStatus`: string (`Pending`, `Success`, `Failed`, `Error`)
  - `extractedTitle`, `extractedCounterparty`, `extractedEffectiveDate`, `extractedExpirationDate`, `extractedCategory`, `extractedRiskBaseline`
- Extraction is triggered automatically on upload (async background job).
- **Gap:** No endpoint to accept/reject extraction or update contract metadata from extracted values. The extraction fields are read-only on the document version DTO. Updating the contract with extracted data requires calling `ContractService.update()`.
- **Implication:** The "accept" workflow is a frontend-side orchestration: read extracted values from the document version, then patch the contract via `update()`. No new backend endpoint needed.

## Frontend Scope

### Task 1: Add Extraction State to contract-detail.component.ts
**File:** `angular/src/app/contracts/contract-detail.component.ts`

**New state:**
```typescript
extractionReviewVisible = false;
selectedVersionForReview: ContractDocumentVersionDto | null = null;
```

**UI additions inside Document Versions card:**
Add a row action button "Review" for versions where `extractionStatus === 'Success'`:
```html
<button class="btn btn-sm btn-outline-info"
        (click)="openExtractionReview(version)"
        *ngIf="version.extractionStatus === 'Success'"
        *abpPermission="'LegalTech.Contracts.Edit'">
  Review Extraction
</button>
```

### Task 2: Build Extraction Review Modal
Add a modal to the template:
```html
<div class="modal-backdrop fade show" *ngIf="extractionReviewVisible" (click)="closeExtractionReview()"></div>
<div class="modal fade show d-block" tabindex="-1" *ngIf="extractionReviewVisible" role="dialog" aria-modal="true">
  <div class="modal-dialog modal-lg modal-dialog-centered">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title">Review AI Extraction: {{ selectedVersionForReview?.fileName }}</h5>
        <button type="button" class="btn-close" (click)="closeExtractionReview()" aria-label="Close"></button>
      </div>
      <div class="modal-body">
        <div class="alert alert-info">
          AI extracted the following values. Accept to update the contract, or edit and accept.
        </div>
        <form [formGroup]="extractionForm">
          <div class="mb-3">
            <label class="form-label">Title</label>
            <input class="form-control" formControlName="title" />
          </div>
          <div class="mb-3">
            <label class="form-label">Counterparty</label>
            <input class="form-control" formControlName="counterparty" />
          </div>
          <div class="row">
            <div class="col-md-6 mb-3">
              <label class="form-label">Effective Date</label>
              <input type="date" class="form-control" formControlName="effectiveDate" />
            </div>
            <div class="col-md-6 mb-3">
              <label class="form-label">Expiration Date</label>
              <input type="date" class="form-control" formControlName="expirationDate" />
            </div>
          </div>
          <div class="row">
            <div class="col-md-6 mb-3">
              <label class="form-label">Category</label>
              <input class="form-control" formControlName="category" />
            </div>
            <div class="col-md-6 mb-3">
              <label class="form-label">Risk Baseline</label>
              <input class="form-control" formControlName="riskBaseline" />
            </div>
          </div>
        </form>
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-secondary" (click)="closeExtractionReview()">Cancel</button>
        <button type="button" class="btn btn-primary" (click)="acceptExtraction()" [disabled]="extractionForm.invalid">Accept & Update Contract</button>
      </div>
    </div>
  </div>
</div>
```

**Component class additions:**
- `extractionForm: FormGroup` initialized in constructor
- `openExtractionReview(version)` — sets `selectedVersionForReview`, populates `extractionForm` with extracted values + current contract values (prefer current contract values as defaults), shows modal
- `closeExtractionReview()` — hides modal, clears form
- `acceptExtraction()` — calls `contractService.update(contract.id, extractionForm.value)`, then reloads contract and versions

**Form initialization:**
```typescript
this.extractionForm = this.fb.group({
  title: ['', Validators.required],
  counterparty: ['', Validators.required],
  effectiveDate: [''],
  expirationDate: [''],
  category: [''],
  riskBaseline: [''],
});
```

**Populate logic in `openExtractionReview`:**
```typescript
this.extractionForm.patchValue({
  title: this.selectedVersionForReview!.extractedTitle || this.contract?.title || '',
  counterparty: this.selectedVersionForReview!.extractedCounterparty || this.contract?.counterpartyName || '',
  effectiveDate: this.selectedVersionForReview!.extractedEffectiveDate || this.contract?.effectiveDate || '',
  expirationDate: this.selectedVersionForReview!.extractedExpirationDate || this.contract?.expirationDate || '',
  category: this.selectedVersionForReview!.extractedCategory || this.contract?.category || '',
  riskBaseline: this.selectedVersionForReview!.extractedRiskBaseline || this.contract?.riskBaseline || '',
});
```

### Task 3: Add Re-upload on Failure
For versions with `extractionStatus === 'Failed'` or `'Error'`, show a "Re-upload for Extraction" button that re-triggers the existing `upload()` method (the backend re-processes on upload).

### Task 4: Extraction Status Badge Enhancement
Update `extractionLabel` and `extractionBadgeClass`:
- `Success` → green, show "Review" button
- `Failed`/`Error` → red, show "Retry" button
- `Pending` → yellow, show spinner or "Processing..."

## Backend Work Required (out of scope)
1. **Accept extraction endpoint:** `POST /api/app/contract/accept-extraction?contractId={id}&versionId={id}` — server-side atomic update of contract metadata from document version extraction results. Currently frontend must manually call `update()`.
2. **Extraction confidence scores:** Add `extractionConfidence` to `ContractDocumentVersionDto` so UI can show per-field confidence.
3. **Extraction history:** Track which fields were accepted vs. rejected for audit.

## Validation
- `ng build --configuration production` passes
- Upload a document → wait for extraction → see "Review" button
- Click Review → modal opens with extracted values pre-filled
- Accept → contract metadata updates
- Cancel → modal closes, no changes
- Failed extraction shows retry option
