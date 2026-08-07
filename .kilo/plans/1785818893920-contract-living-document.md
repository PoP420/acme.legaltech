# Plan 5: Contract-Centric Living Document View

## Objective
Enhance `contract-detail.component.ts` to become the single source of truth for a contract, integrating linked reviews, obligations, document timeline, and gov compliance in one scrollable page with collapsible sections.

## Backend Status
- `ContractDto` already includes: `signatories`, `variationOrders`, `currentAuthority`, `documentVersions`
- `ContractAppService.GetAsync` loads all nested collections
- **Gap:** No direct endpoint to get reviews by contract ID with nested data (only `ReviewCaseGetListInput` with `contractId` filter, but `ReviewCaseDto` only has counts, not tasks/comments).
- **Gap:** No direct endpoint to get obligations by contract ID with evidence nested (only `ContractObligationGetListInput` with `contractId` filter, but no evidence list).

## Frontend Scope

### Task 1: Add Linked Reviews Section
**File:** `angular/src/app/contracts/contract-detail.component.ts`

**New state:**
```typescript
linkedReviews: ReviewCaseDto[] = [];
loadingReviews = false;
```

**New service method needed:**
- `reviewsService.getList({ contractId: this.contract.id, maxResultCount: 10 })` — already supported by `ReviewCaseGetListInput`

**UI:**
```html
<div class="card mb-4">
  <div class="card-header d-flex justify-content-between">
    <h4>Linked Reviews</h4>
    <a class="btn btn-sm btn-primary" [routerLink]="['/reviews/create']" [queryParams]="{ contractId: contract.id }">New Review</a>
  </div>
  <div class="card-body">
    <table class="table" *ngIf="linkedReviews.length; else noReviews">
      <thead>
        <tr><th>Title</th><th>Status</th><th>Assigned</th><th>Due</th><th>Priority</th></tr>
      </thead>
      <tbody>
        <tr *ngFor="let review of linkedReviews">
          <td><a [routerLink]="['/reviews', review.id]">{{ review.title }}</a></td>
          <td>{{ review.status }}</td>
          <td>{{ review.assignedUserName || '-' }}</td>
          <td>{{ review.dueDate | date:'shortDate' || '-' }}</td>
          <td>{{ review.priority }}</td>
        </tr>
      </tbody>
    </table>
    <ng-template #noReviews><p class="text-muted">No reviews linked.</p></ng-template>
  </div>
</div>
```

**Load logic:** Call `reviewsService.getList({ contractId: contract.id, maxResultCount: 10 })` after contract loads.

### Task 2: Add Linked Obligations Section
**File:** `angular/src/app/contracts/contract-detail.component.ts`

**New state:**
```typescript
linkedObligations: ContractObligationDto[] = [];
loadingObligations = false;
```

**UI:**
```html
<div class="card mb-4">
  <div class="card-header d-flex justify-content-between">
    <h4>Obligations</h4>
    <a class="btn btn-sm btn-primary" [routerLink]="['/obligations/create']" [queryParams]="{ contractId: contract.id }">New Obligation</a>
  </div>
  <div class="card-body">
    <table class="table" *ngIf="linkedObligations.length; else noObligations">
      <thead>
        <tr><th>Title</th><th>Status</th><th>Due</th><th>Priority</th><th>Evidence</th></tr>
      </thead>
      <tbody>
        <tr *ngFor="let obl of linkedObligations">
          <td><a [routerLink]="['/obligations', obl.id]">{{ obl.title }}</a></td>
          <td><span class="badge" [ngClass]="obligationStatusBadge(obl.status)">{{ obl.status }}</span></td>
          <td>{{ obl.dueDate | date:'shortDate' || '-' }}</td>
          <td>{{ obl.priority }}</td>
          <td>{{ obl.evidenceCount }}</td>
        </tr>
      </tbody>
    </table>
    <ng-template #noObligations><p class="text-muted">No obligations linked.</p></ng-template>
  </div>
</div>
```

**Load logic:** Call `obligationsService.getList({ contractId: contract.id, maxResultCount: 10 })` after contract loads.

### Task 3: Collapsible Section Layout
Wrap each major section in a collapsible panel:
- Metadata & Gov Fields (always expanded)
- Document Versions
- Signatories & Variation Orders
- Approval Authority
- Linked Reviews
- Linked Obligations

Use Angular `*ngIf` toggle or simple CSS-based collapse. For MVP, use `details`/`summary` HTML elements for zero-dependency collapsibles.

### Task 4: Add Quick Actions Bar
At the top of the page, below the title:
- "New Review" → links to `/reviews/create?contractId={id}`
- "New Obligation" → links to `/obligations/create?contractId={id}`
- "Upload Document" → focuses file input in document versions section
- "Change Status" → opens status dropdown (already implemented)

### Task 5: Update Contract Routes to Pass Contract ID
**File:** `angular/src/app/contracts/contracts.routes.ts`
No changes needed — `:id` param already available.

### Task 6: Update Reviews Create Form to Accept ContractId
**File:** `angular/src/app/reviews/create-or-edit-review.component.ts`
- Read `contractId` from `ActivatedRoute.queryParamMap`
- Pre-populate `contractId` field if present
- Navigate back to contract detail after create: `this.router.navigate(['/contracts', contractId])`

### Task 7: Update Obligations Create Form to Accept ContractId
**File:** `angular/src/app/obligations/create-or-edit-obligation.component.ts`
- Same pattern as reviews: read `contractId` from query params, pre-populate, navigate back to contract detail.

### Task 8: Add Inline Add-Variation / Add-Signatory Forms
Already implemented in the gov compliance plan. Ensure they are placed within their collapsible sections.

## Backend Work Required (out of scope)
1. **Nested DTOs in ReviewCaseDto:** Include `tasks`, `comments`, `approvalSteps`, `escalations` arrays so the contract detail can show review depth without separate calls.
2. **Nested evidence in ContractObligationDto:** Include `evidence` array.
3. **Contract summary endpoint:** `GET /api/app/contract/summary/{id}` returning counts of linked reviews, obligations, documents, signatories for fast dashboard rendering.

## Validation
- `ng build --configuration production` passes
- Contract detail page loads with all sections
- Linked reviews and obligations load via query param filters
- New Review/Obligation buttons pass contractId via query params
- Create form pre-populates contractId and navigates back to contract detail after save
- Collapsible sections work with `details`/`summary`
