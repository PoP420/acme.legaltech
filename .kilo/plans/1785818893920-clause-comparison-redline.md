# Plan 10: Clause Comparison / Redline Preview

## Objective
Build a split-pane diff view comparing a contract clause against a selected playbook's preferred clause, showing exact text differences with track-changes style markup.

## Backend Status
- `ClauseTemplateDto` has `content` (full clause text)
- `PlaybookRuleDto` has `clausePattern` (the text pattern to match against), `isPreferred`, `isFallback`, `isProhibited`, `severity`
- `PlaybookEvaluationResultDto` has `matched`, `matchSpan`, `rationale`
- `PlaybookProfileAppService.EvaluateAsync` evaluates one clause text against one playbook
- **Gap:** No endpoint to get the full clause text from a contract document. Extraction only provides metadata (title, counterparty, dates), not structured clause extraction.
- **Gap:** No diff/compare API exists.

## Frontend Scope

### Task 1: Create Clause Comparison Component
**File:** `angular/src/app/clauses/clause-compare.component.ts`

**Route addition in `clauses.routes.ts`:**
```typescript
{
  path: 'compare',
  loadComponent: () => import('./clause-compare.component').then(c => c.ClauseCompareComponent),
},
```

### Task 2: Comparison UI
```html
<div class="container mt-3">
  <h3>Clause Comparison</h3>
  <div class="row g-2 mb-3">
    <div class="col-md-5">
      <label class="form-label">Contract Clause (Original)</label>
      <textarea class="form-control" rows="12" [(ngModel)]="originalText" placeholder="Paste or load contract clause text..."></textarea>
    </div>
    <div class="col-md-2 d-flex align-items-center justify-content-center">
      <div class="text-center">
        <button class="btn btn-primary mb-2" (click)="compare()">Compare</button>
        <div *ngIf="diffResult" class="mt-2">
          <span class="badge bg-success" *ngIf="diffResult.matchScore > 80">High Match</span>
          <span class="badge bg-warning" *ngIf="diffResult.matchScore > 50 && diffResult.matchScore <= 80">Partial</span>
          <span class="badge bg-danger" *ngIf="diffResult.matchScore <= 50">Low Match</span>
        </div>
      </div>
    </div>
    <div class="col-md-5">
      <label class="form-label">Playbook Standard (Preferred)</label>
      <textarea class="form-control" rows="12" [(ngModel)]="standardText" placeholder="Preferred clause from playbook..."></textarea>
    </div>
  </div>

  <div class="card" *ngIf="diffResult">
    <div class="card-header"><h5>Redline Result</h5></div>
    <div class="card-body">
      <div class="alert alert-info">
        <strong>Match Score:</strong> {{ diffResult.matchScore }}%<br>
        <strong>Rationale:</strong> {{ diffResult.rationale || '-' }}
      </div>
      <div class="row">
        <div class="col-md-6">
          <h6>Original with Markup</h6>
          <div class="border p-3 bg-light" [innerHTML]="originalMarkup"></div>
        </div>
        <div class="col-md-6">
          <h6>Suggested Replacement</h6>
          <div class="border p-3 bg-light" [innerHTML]="standardMarkup"></div>
        </div>
      </div>
      <div class="mt-3" *abpPermission="'LegalTech.Clauses.Manage'">
        <button class="btn btn-success me-2">Accept Suggestion</button>
        <button class="btn btn-secondary">Reject</button>
      </div>
    </div>
  </div>
</div>
```

### Task 3: Simple Diff Algorithm
Since no backend diff API exists, implement a simple line-by-line or word-by-word diff in the frontend:

```typescript
compare(): void {
  if (!this.originalText || !this.standardText) return;
  
  const originalLines = this.originalText.split('\n');
  const standardLines = this.standardText.split('\n');
  
  // Simple LCS-based diff or use a lightweight library
  // For MVP, use basic string similarity (Levenshtein) and highlight differences
  const result = this.computeDiff(this.originalText, this.standardText);
  this.diffResult = result;
  this.originalMarkup = this.highlightDiff(this.originalText, result.removals);
  this.standardMarkup = this.highlightDiff(this.standardText, result.additions);
}
```

**Simpler MVP approach:** Use `diff` library if available in package.json, otherwise implement basic word-level comparison:
- Split both texts into words
- Find longest common subsequence
- Mark words not in LCS as removed (red) or added (green)

### Task 4: Load Data from Contract and Playbook
Allow users to:
1. Select a contract → load its AI-extracted text (from `extractedTitle`, `extractedCategory`, etc., or from document blob if possible)
2. Select a playbook → load its rules' `clausePattern` as the standard text
3. Paste text manually (primary MVP workflow)

**Service additions in `playbooks.service.ts`:**
- `getRules(playbookId: string): Observable<PlaybookRuleDto[]>` — not currently exposed. Add a method that calls `getList({ maxResultCount: 100 })` and filters by `playbookId` client-side, or note that `getList` returns all playbooks, not rules.

**Actually:** `PlaybookProfileDto.rules` is already populated in `getList` response. So:
```typescript
this.playbooksService.getList({ maxResultCount: 100 }).subscribe(result => {
  const playbook = result.items?.find(p => p.id === selectedPlaybookId);
  this.standardText = playbook?.rules?.find(r => r.isPreferred)?.clausePattern || '';
});
```

### Task 5: Add Evaluate Button Integration
Instead of manual paste, add an "Evaluate against Playbook" button that calls `playbooksService.evaluate({ contractId, clauseText: originalText, playbookId })` and displays the `PlaybookEvaluationResultDto` alongside the diff.

## Backend Work Required (out of scope)
1. **Clause extraction endpoint:** `GET /api/app/contract/{id}/extracted-clauses` returning all clauses extracted from the contract document.
2. **Diff/compare API:** `POST /api/app/clause/compare` returning structured diff with additions, removals, and match score.
3. **Rule retrieval endpoint:** `GET /api/app/playbook-profile/{id}/rules` returning only rules for a specific playbook (currently rules are nested in profile DTO).

## Validation
- `ng build --configuration production` passes
- Compare page loads with two text panes
- Compare button computes diff and displays markup
- Playbook rules load into standard text pane
- Match score and rationale display correctly
- Red/green highlights show differences
