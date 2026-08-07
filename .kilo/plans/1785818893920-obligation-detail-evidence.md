# Plan 2: Obligation Detail with Evidence & Completion

## Objective
Transform `obligation-detail.component.ts` (currently a 19-line stub) into a full obligation workspace with evidence gallery, complete/defer actions, and recurrence visualization.

## Backend Status
- `ContractObligationAppService` exposes: CRUD via `ICrudAppService`, plus `CompleteAsync(id)` and `DeferAsync(id)`
- `ContractObligationDto` contains: `evidenceCount` (count only, no nested evidence entities)
- **Gap:** No `CompletionEvidence` app service or controller exists. `CompletionEvidence` is a domain entity with DbSet in `LegalTechDbContext` but no exposed endpoints.
- **Implication:** Frontend cannot upload, list, or download completion evidence without backend changes.

## Frontend Scope (no backend changes)
Build the obligation detail UI, wire up available endpoints, and show evidence count with placeholder for missing upload/download functionality.

### Task 1: Extend ObligationsService
**File:** `angular/src/app/services/obligations.service.ts`
- Add `complete(id: string): Observable<ContractObligationDto>`
- Add `defer(id: string): Observable<ContractObligationDto>`
- Note: `getEvidence(obligationId)` would require new backend endpoint — **mark as placeholder.**

### Task 2: Rewrite obligation-detail.component.ts
**File:** `angular/src/app/obligations/obligation-detail.component.ts`

**Template structure:**
```html
<div class="container mt-3">
  <a class="btn btn-secondary mb-3" [routerLink]="['/obligations']">&larr; Back</a>
  
  <div class="card mb-4" *ngIf="obligation">
    <div class="card-header d-flex justify-content-between align-items-center">
      <h3>{{ obligation.title }}</h3>
      <span class="badge" [ngClass]="statusBadge(obligation.status)">{{ obligation.status }}</span>
    </div>
    <div class="card-body">
      <dl class="row">
        <dt class="col-sm-3">Contract</dt>
        <dd class="col-sm-9">{{ obligation.contractTitle || obligation.contractId }}</dd>
        <dt class="col-sm-3">Description</dt>
        <dd class="col-sm-9">{{ obligation.description || '-' }}</dd>
        <dt class="col-sm-3">Due Date</dt>
        <dd class="col-sm-9">{{ obligation.dueDate | date:'shortDate' || '-' }}</dd>
        <dt class="col-sm-3">Completed At</dt>
        <dd class="col-sm-9">{{ obligation.completedAt | date:'shortDateTime' || '-' }}</dd>
        <dt class="col-sm-3">Priority</dt>
        <dd class="col-sm-9">{{ obligation.priority }}</dd>
        <dt class="col-sm-3">Source Clause</dt>
        <dd class="col-sm-9">{{ obligation.sourceClauseReference || '-' }}</dd>
        <dt class="col-sm-3">Recurrence</dt>
        <dd class="col-sm-9">{{ recurrenceLabel }}</dd>
      </dl>

      <div class="mt-3" *ngIf="obligation.status !== 'Completed' && obligation.status !== 'Deferred'">
        <button class="btn btn-outline-success me-2" *abpPermission="'LegalTech.Obligations.Complete'" (click)="onComplete()">Mark Complete</button>
        <button class="btn btn-outline-warning" *abpPermission="'LegalTech.Obligations.Manage'" (click)="onDefer()">Defer</button>
      </div>
    </div>
  </div>

  <div class="card mb-4">
    <div class="card-header d-flex justify-content-between">
      <h4>Evidence</h4>
      <span class="badge bg-secondary">{{ obligation.evidenceCount }} files</span>
    </div>
    <div class="card-body">
      <div class="alert alert-warning">Evidence upload/download requires a new backend endpoint.</div>
      <div class="row g-2">
        <div class="col-md-3" *ngFor="let i of [].constructor(obligation.evidenceCount)">
          <div class="border rounded p-2 text-center text-muted">Evidence {{ i + 1 }}</div>
        </div>
      </div>
      <div *ngIf="obligation.evidenceCount === 0" class="text-muted">No evidence uploaded yet.</div>
    </div>
  </div>
</div>
```

**Component class additions:**
- `obligation: ContractObligationDto | null = null`
- `get recurrenceLabel(): string` — parse `obligation.recurrencePattern` (e.g., "Monthly", "Quarterly", "Weekly") or show "One-time"
- `statusBadge(status: string)` — map status to Bootstrap badge class
- Load obligation in constructor via `ActivatedRoute` paramMap + `obligationsService.get(id)`
- `onComplete()`: confirm dialog, call `obligationsService.complete(id)`, reload
- `onDefer()`: confirm dialog, call `obligationsService.defer(id)`, reload

### Task 3: Add Obligation Status Enums & Labels
**File:** `angular/src/app/services/obligations.service.ts`
- Add `ObligationStatus` type and `ObligationStatusLabels` map
- Common statuses: `Pending`, `InProgress`, `Completed`, `Deferred`, `Overdue`
- Update `ContractObligationDto.status` typing to use the enum

### Task 4: Update create-or-edit-obligation.component.ts
**File:** `angular/src/app/obligations/create-or-edit-obligation.component.ts`
- Add `isRecurring` toggle checkbox
- Add `recurrencePattern` dropdown (None, Daily, Weekly, Monthly, Quarterly, Annually)
- Add `sourceClauseReference` field
- Add `priority` validation (0-5)
- Add `dueDate` validation: must be future date for new obligations
- Show contract title lookup if possible

## Backend Work Required (out of scope for this frontend plan)
1. New app service `ICompletionEvidenceAppService` with endpoints:
   - `GET /api/app/completion-evidence?obligationId={id}`
   - `POST /api/app/completion-evidence` (multipart upload)
   - `DELETE /api/app/completion-evidence/{id}`
   - `GET /api/app/completion-evidence/{id}/download`
2. Update `ContractObligationDto` to include nested `evidence` list, or add separate list endpoint
3. Add `Overdue` status detection (e.g., domain method or query filter)

## Validation
- `ng build --configuration production` passes
- Obligation detail loads from URL param `:id`
- Complete and Defer buttons work with permission guards
- Evidence count displays; placeholder shown for upload/download
- Recurrence label renders correctly from pattern string
