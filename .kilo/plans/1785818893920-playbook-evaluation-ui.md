# Plan 6: Playbook Evaluation on Contract Clauses

## Objective
Build a UI to evaluate a contract's clauses against a selected playbook profile and display risk markers (preferred, fallback, prohibited) with severity indicators.

## Backend Status
- `PlaybookProfileAppService.EvaluateAsync(PlaybookEvaluateInput)` exists and returns `PlaybookEvaluationResultDto[]`
- `PlaybookEvaluateInput` fields: `contractId: string`, `clauseText: string`, `playbookId?: string`
- `PlaybookEvaluationResultDto` fields: `ruleId`, `ruleName`, `severity`, `matched`, `matchSpan`, `rationale`, `isPreferred`, `isFallback`, `isProhibited`
- **Gap:** No endpoint to get all clauses for a contract. `ClauseTemplateAppService` is a standalone clause library, not linked to contracts.
- **Gap:** No endpoint to get playbook rules with clause text for bulk evaluation. `EvaluateAsync` evaluates one clause text at a time.
- **Implication:** Frontend must orchestrate: fetch contract document versions → extract text (already done via AI extraction) → evaluate each clause against selected playbook. For MVP, evaluate user-pasted or AI-extracted clause text.

## Frontend Scope

### Task 1: Add Playbook Evaluation to Playbook Detail
**File:** `angular/src/app/playbooks/playbook-detail.component.ts`

**Current state:** 19-line stub.

**New UI sections:**
1. Playbook info (name, description, rules list)
2. "Evaluate on Contract" section:
   - Dropdown to select contract (fetch from `contractService.getList({ maxResultCount: 50 })`)
   - Button "Evaluate Clauses"
   - Results table with clause text, matched rules, severity badges, rationale

**Template additions:**
```html
<div class="card mb-4">
  <div class="card-header"><h4>Evaluate on Contract</h4></div>
  <div class="card-body">
    <div class="row g-2 mb-3">
      <div class="col-md-6">
        <select class="form-select" [(ngModel)]="selectedContractId">
          <option [ngValue]="undefined">Select a contract...</option>
          <option *ngFor="let c of contracts" [ngValue]="c.id">{{ c.title }}</option>
        </select>
      </div>
      <div class="col-md-3">
        <button class="btn btn-primary" (click)="onEvaluate()" [disabled]="!selectedContractId">Evaluate</button>
      </div>
    </div>
    <div *ngIf="evaluationResults.length">
      <table class="table">
        <thead>
          <tr><th>Clause Text</th><th>Rule</th><th>Severity</th><th>Status</th><th>Rationale</th></tr>
        </thead>
        <tbody>
          <tr *ngFor="let result of evaluationResults">
            <td>{{ result.matchSpan || '-' }}</td>
            <td>{{ result.ruleName }}</td>
            <td><span class="badge" [ngClass]="severityBadge(result.severity)">{{ result.severity }}</span></td>
            <td>
              <span class="badge bg-success" *ngIf="result.isPreferred">Preferred</span>
              <span class="badge bg-warning" *ngIf="result.isFallback">Fallback</span>
              <span class="badge bg-danger" *ngIf="result.isProhibited">Prohibited</span>
            </td>
            <td>{{ result.rationale || '-' }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</div>
```

**Component class additions:**
- `contracts: ContractDto[] = []`
- `selectedContractId: string | undefined`
- `evaluationResults: PlaybookEvaluationResultDto[] = []`
- `loadContracts()`: call `contractService.getList({ maxResultCount: 50 })`
- `onEvaluate()`: for each clause in playbook rules, call `playbooksService.evaluate({ contractId, clauseText: rule.clausePattern, playbookId })` — batch calls or evaluate one rule at a time. For MVP, evaluate each rule's `clausePattern` against the contract.

### Task 2: Add Evaluation Section to Playbook List
**File:** `angular/src/app/playbooks/playbooks-list.component.ts`
- Add "Evaluate" button per playbook row
- Clicking opens a modal or navigates to playbook detail with pre-selected playbook

### Task 3: Add Severity Badge Helper
```typescript
severityBadge(severity: number): string {
  if (severity >= 8) return 'bg-danger';
  if (severity >= 5) return 'bg-warning';
  return 'bg-success';
}
```

### Task 4: Add Playbook Evaluation Service Method
**File:** `angular/src/app/services/playbooks.service.ts`
- `evaluate(input: PlaybookEvaluateInput): Observable<PlaybookEvaluationResultDto[]>` — already exists. Verify it works.

## Backend Work Required (out of scope)
1. **Bulk evaluation endpoint:** `POST /api/app/playbook-profile/evaluate-bulk` accepting multiple clause texts and returning consolidated results.
2. **Contract clause extraction:** Endpoint to return all clauses extracted from a contract document, so frontend doesn't need to iterate rules manually.
3. **Rule-level clause text:** `PlaybookRuleDto` already has `clausePattern` — ensure it's populated correctly.

## Validation
- `ng build --configuration production` passes
- Playbook detail loads with existing rules list
- Contract dropdown populates from contract list
- Evaluate button triggers evaluation calls
- Results display with correct severity badges and preferred/fallback/prohibited labels
