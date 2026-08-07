# Plan 1: Review Workflow Detail & Task Execution

## Objective
Transform `review-detail.component.ts` (currently a 19-line stub) into a functional review workspace where lawyers can view tasks, approval steps, add comments, assign reviews, escalate, and complete cases.

## Backend Status
- `ReviewAppService` exposes: `AssignAsync(id, userId)`, `EscalateAsync(id, reason, severity)`, `CompleteAsync(id)`
- `ReviewCaseDto` contains: `taskCount`, `completedTaskCount`, `escalationCount` (counts only, no nested entities)
- **Gap:** No CRUD endpoints for `ReviewTask`, `ApprovalStep`, `ReviewComment`, or `EscalationEvent`. These entities exist in domain/DbContext but are not exposed via any app service or controller.
- **Implication:** Frontend cannot load individual tasks, comments, or approval steps without backend changes. See "Backend Work Required" section.

## Frontend Scope (no backend changes)
Build the UI shell and wire up available endpoints. Display counts from `ReviewCaseDto`. Add comment and escalation inputs that call existing endpoints where possible, and gracefully disable features requiring missing backend endpoints.

### Task 1: Extend ReviewsService
**File:** `angular/src/app/services/reviews.service.ts`
- Add `getComments(reviewCaseId: string)` — currently impossible without backend endpoint. **Mark as placeholder.**
- Add `addComment(reviewCaseId: string, content: string)` — currently impossible. **Mark as placeholder.**
- Note: `assign()`, `escalate()`, `complete()` already exist.

### Task 2: Rewrite review-detail.component.ts
**File:** `angular/src/app/contracts/reviews/review-detail.component.ts` (move from current location if needed)

**Template structure:**
```html
<div class="container mt-3">
  <a class="btn btn-secondary mb-3" [routerLink]="['/reviews']">&larr; Back</a>
  
  <div class="card mb-4" *ngIf="review">
    <div class="card-header d-flex justify-content-between">
      <h3>{{ review.title }}</h3>
      <span class="badge">{{ review.status }}</span>
    </div>
    <div class="card-body">
      <dl class="row">
        <dt class="col-sm-3">Contract</dt>
        <dd class="col-sm-9">{{ review.contractTitle || review.contractId }}</dd>
        <dt class="col-sm-3">Assigned To</dt>
        <dd class="col-sm-9">{{ review.assignedUserName || '-' }}</dd>
        <dt class="col-sm-3">Due Date</dt>
        <dd class="col-sm-9">{{ review.dueDate | date:'shortDate' || '-' }}</dd>
        <dt class="col-sm-3">Priority</dt>
        <dd class="col-sm-9">{{ review.priority }}</dd>
        <dt class="col-sm-3">Summary</dt>
        <dd class="col-sm-9">{{ review.summary || '-' }}</dd>
      </dl>

      <div class="mt-3 d-flex gap-2" *abpPermission="'LegalTech.Reviews.Assign'">
        <button class="btn btn-outline-primary" (click)="onAssign()">Assign to Me</button>
        <button class="btn btn-outline-warning" (click)="onEscalate()">Escalate</button>
        <button class="btn btn-outline-success" (click)="onComplete()">Mark Complete</button>
      </div>
    </div>
  </div>

  <div class="row mb-4">
    <div class="col-md-4">
      <div class="card">
        <div class="card-header"><h5>Tasks</h5></div>
        <div class="card-body">
          <p class="text-muted">{{ review?.taskCount || 0 }} total, {{ review?.completedTaskCount || 0 }} completed</p>
          <p class="small text-warning">Task details require backend endpoint.</p>
        </div>
      </div>
    </div>
    <div class="col-md-4">
      <div class="card">
        <div class="card-header"><h5>Approval Steps</h5></div>
        <div class="card-body">
          <p class="small text-warning">Approval step details require backend endpoint.</p>
        </div>
      </div>
    </div>
    <div class="col-md-4">
      <div class="card">
        <div class="card-header"><h5>Escalations</h5></div>
        <div class="card-body">
          <p class="text-muted">{{ review?.escalationCount || 0 }} escalations</p>
          <p class="small text-warning">Escalation details require backend endpoint.</p>
        </div>
      </div>
    </div>
  </div>

  <div class="card mb-4">
    <div class="card-header"><h4>Comments</h4></div>
    <div class="card-body">
      <div class="alert alert-warning">Comments require a new backend endpoint.</div>
      <div class="mb-3" *abpPermission="'LegalTech.Reviews.Default'">
        <textarea class="form-control mb-2" [(ngModel)]="newComment" placeholder="Add a comment..."></textarea>
        <button class="btn btn-primary" [disabled]="!newComment">Post Comment</button>
      </div>
    </div>
  </div>
</div>
```

**Component class additions:**
- `review: ReviewCaseDto | null = null`
- `newComment = ''`
- `onAssign()`: call `reviewsService.assign(review.id, currentUser.id)` then reload
- `onEscalate()`: prompt for reason/severity, call `reviewsService.escalate(review.id, reason, severity)`
- `onComplete()`: confirm, call `reviewsService.complete(review.id)`
- Load review in constructor via `ActivatedRoute` paramMap + `reviewsService.get(id)`
- Permission guards: `Assign` for assign/escalate/complete buttons; `Decide` for complete; `Escalate` for escalate

### Task 3: Update create-or-edit-review.component.ts
**File:** `angular/src/app/reviews/create-or-edit-review.component.ts`
- Add `contractTitle` display by fetching contract title via `contractsService.get(contractId)` after create/update
- Add validation: `contractId` must be a valid GUID format
- Add `dueDate` validation: must be future date
- Add `priority` range validation (0-3 or 0-5)
- Add loading/saving state management

## Backend Work Required (out of scope for this frontend plan)
To make the review detail fully functional, the backend needs:
1. New app service `IReviewTaskAppService` with CRUD for review tasks
2. New app service `IReviewCommentAppService` with CRUD for comments
3. New app service `IApprovalStepAppService` with CRUD for approval steps
4. New app service `IEscalationEventAppService` with CRUD for escalation events
5. Update `ReviewCaseDto` to include nested `tasks`, `comments`, `approvalSteps`, `escalations` (or add separate GET endpoints)
6. Update `ReviewAppService.MapToGetOutputDto` to load nested collections

**Until then, the frontend shows placeholder warnings for missing data.**

## Validation
- `ng build --configuration production` passes
- Review detail loads a review case from URL param `:id`
- Status badge displays correctly
- Assign/Esclate/Complete buttons show/hide based on permissions
- Comment textarea exists (disabled until backend added)
