# Plan 9: Compliance Audit Trail & Export

## Objective
Add a compliance audit view per contract showing changes to government fields (classification, signatories, variation orders, status changes) and enable export to PDF/CSV.

## Backend Status
- All entities have `CreationTime`, `CreatorId`, `LastModificationTime`, `LastModifierId`
- **Gap:** No `AuditLog` entity or endpoint exists. ABP's built-in `EntityChangeHistory` is available via `AbpEntityHistory` but requires configuration and is not exposed via API by default.
- **Gap:** No export endpoints for reports (CSV/PDF).
- **Implication:** True field-level audit trail requires enabling ABP Entity History or building a custom audit log. For MVP, we can show the current state of gov fields and document the change history limitation.

## Frontend Scope

### Task 1: Add Compliance View Route
**File:** `angular/src/app/contracts/contracts.routes.ts`
```typescript
{
  path: ':id/compliance',
  loadComponent: () => import('./compliance-view.component').then(c => c.ComplianceViewComponent),
},
```

### Task 2: Create Compliance View Component
**File:** `angular/src/app/contracts/compliance-view.component.ts`

**UI sections:**
1. **Document Classification Timeline:** Show current `classification`, `documentNumber`, `documentSeries`, `documentYear`, `retentionUntil`
2. **Signatory Audit:** Table of signatories with `role`, `partyType`, `governmentAgency`, `capacity`, `signedOn`
3. **Variation Order History:** Table of variation orders with `description`, `amount`, `cumulativeAmount`, `approvedOn`
4. **Status Change History:** Since `ContractChangeStatusDto` is not persisted as an entity, show current status and last modification time. Note: status change history is not tracked unless ABP Entity History is enabled.
5. **Approval Authority:** Current authority display (already in contract detail)

**Template skeleton:**
```html
<div class="container mt-3">
  <a class="btn btn-secondary mb-3" [routerLink]="['/contracts', id]">&larr; Back to Contract</a>
  <div class="d-flex justify-content-between mb-3" *abpPermission="'LegalTech.Reports.Export'">
    <h3>Compliance Report: {{ contract?.title }}</h3>
    <button class="btn btn-outline-secondary" (click)="exportPdf()">Export PDF</button>
    <button class="btn btn-outline-secondary" (click)="exportCsv()">Export CSV</button>
  </div>

  <div class="card mb-4">
    <div class="card-header"><h4>Document Classification</h4></div>
    <div class="card-body">
      <dl class="row">
        <dt class="col-sm-3">Document Number</dt><dd class="col-sm-9">{{ contract?.documentNumber || '-' }}</dd>
        <dt class="col-sm-3">Series</dt><dd class="col-sm-9">{{ contract?.documentSeries || '-' }}</dd>
        <dt class="col-sm-3">Year</dt><dd class="col-sm-9">{{ contract?.documentYear ?? '-' }}</dd>
        <dt class="col-sm-3">Classification</dt><dd class="col-sm-9">{{ classificationLabel(contract?.classification) }}</dd>
        <dt class="col-sm-3">Retention Until</dt><dd class="col-sm-9">{{ contract?.retentionUntil || '-' }}</dd>
      </dl>
    </div>
  </div>

  <!-- Signatories, Variation Orders, Authority sections similar -->
</div>
```

### Task 3: Add Export Functions
**File:** `angular/src/app/contracts/compliance-view.component.ts`

**CSV Export:**
```typescript
exportCsv(): void {
  if (!this.contract) return;
  const rows = [
    ['Field', 'Value'],
    ['Document Number', this.contract.documentNumber || ''],
    ['Document Series', this.contract.documentSeries || ''],
    ['Document Year', String(this.contract.documentYear ?? '')],
    ['Classification', this.classificationLabel(this.contract.classification)],
    ['Retention Until', this.contract.retentionUntil || ''],
    ['Contract Value', String(this.contract.contractValue ?? '')],
    ['Current Authority', this.contract.currentAuthority?.authorityTitle || ''],
  ];
  const csv = rows.map(r => r.join(',')).join('\n');
  const blob = new Blob([csv], { type: 'text/csv' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `compliance-${this.contract.id}.csv`;
  a.click();
  URL.revokeObjectURL(url);
}
```

**PDF Export:**
- Use `window.print()` triggered on a print-friendly stylesheet for MVP
- Or use `jspdf` if already in dependencies (check `package.json`)
- For MVP, `window.print()` with `@media print` CSS is sufficient

### Task 4: Add Print Stylesheet
Create `angular/src/app/contracts/compliance-view.component.ts` with inline print styles or a separate CSS file:
```css
@media print {
  .btn, .no-print { display: none !important; }
  .card { border: 1px solid #ddd !important; break-inside: avoid; }
}
```

### Task 5: Add Compliance Service Methods
**File:** `angular/src/app/services/contract.service.ts`
- `getCompliance(id: string): Observable<ContractComplianceDto>` — already added in Module 04 plan
- Use this to fetch the full compliance snapshot for the view

### Task 6: Add Route Guard
**File:** `angular/src/app/contracts/contracts.routes.ts`
```typescript
{
  path: ':id/compliance',
  canActivate: [permissionGuard],
  data: { requiredPolicy: 'LegalTech.Contracts.ViewGovFields' },
  loadComponent: () => import('./compliance-view.component').then(c => c.ComplianceViewComponent),
},
```

## Backend Work Required (out of scope)
1. **Enable ABP Entity History:** Configure `AbpEntityHistory` to track `Contract`, `ContractSignatory`, `VariationOrder` changes. Expose via `GET /api/app/entity-history`.
2. **Custom Audit Log:** Build `ContractAuditLog` entity and app service to explicitly track gov field changes (classification changes, signatory additions, status transitions).
3. **PDF export endpoint:** `GET /api/app/reports/contract-compliance/{id}/pdf` returning binary PDF.
4. **CSV export endpoint:** `GET /api/app/reports/contract-compliance/{id}/csv`.

## Validation
- `ng build --configuration production` passes
- Compliance view loads for contract `:id`
- All gov fields display correctly
- Signatory and variation order tables render
- CSV export downloads valid file
- Print view hides buttons and formats cleanly
