# Plan 8: Critical Dates & Renewal Timeline

## Objective
Build a timeline/calendar view of all critical dates across contracts, obligations, and renewals so legal ops can see upcoming deadlines in one place.

## Backend Status
- Contracts: `expirationDate`, `retentionUntil`, `effectiveDate`
- Obligations: `dueDate`, `isRecurring`, `recurrencePattern`
- **Gap:** No dedicated `Renewal` entity or endpoint exists in the backend. The domain has no `Renewal` aggregate. Renewals are implied by contract expiration dates.
- **Gap:** No date-range query endpoints for cross-module date searches.

## Frontend Scope

### Task 1: Create Critical Dates Dashboard
**File:** `angular/src/app/reports/critical-dates.component.ts`

**Route addition in `reports.routes.ts`:**
```typescript
{ path: 'critical-dates', loadComponent: () => import('./critical-dates.component').then(c => c.CriticalDatesComponent) },
```

**State:**
```typescript
contractDates: Array<{ id: string; title: string; date: Date; type: string; url: string }> = [];
obligationDates: Array<{ id: string; title: string; date: Date; type: string; url: string }> = [];
loading = false;
filterDays = 30; // default: next 30 days
```

### Task 2: Data Aggregation Logic
Since no backend endpoint exists, aggregate client-side:

```typescript
loadCriticalDates(): void {
  this.loading = true;
  const cutoff = new Date();
  cutoff.setDate(cutoff.getDate() + this.filterDays);

  // Load contracts
  this.contractService.getList({ maxResultCount: 100 }).subscribe(cr => {
    cr.items?.forEach(c => {
      if (c.expirationDate) {
        const d = new Date(c.expirationDate);
        if (d <= cutoff) this.contractDates.push({ id: c.id, title: c.title, date: d, type: 'Expiration', url: `/contracts/${c.id}` });
      }
      if (c.retentionUntil) {
        const d = new Date(c.retentionUntil);
        if (d <= cutoff) this.contractDates.push({ id: c.id, title: c.title, date: d, type: 'Retention', url: `/contracts/${c.id}` });
      }
    });

    // Load obligations
    this.obligationsService.getList({ maxResultCount: 100 }).subscribe(ob => {
      ob.items?.forEach(o => {
        if (o.dueDate) {
          const d = new Date(o.dueDate);
          if (d <= cutoff) this.obligationDates.push({ id: o.id, title: o.title, date: d, type: 'Obligation Due', url: `/obligations/${o.id}` });
        }
      });
      this.loading = false;
    });
  });
}
```

**Note:** Loading 100 records per module is acceptable for MVP but will need backend aggregation for scale.

### Task 3: Timeline UI
Use a simple grouped list grouped by date:

```html
<div class="container mt-3">
  <div class="row g-2 mb-3">
    <div class="col-md-3">
      <select class="form-select" [(ngModel)]="filterDays" (ngModelChange)="loadCriticalDates()">
        <option [ngValue]="7">Next 7 days</option>
        <option [ngValue]="30">Next 30 days</option>
        <option [ngValue]="90">Next 90 days</option>
      </select>
    </div>
  </div>

  <div class="row">
    <div class="col-md-6">
      <div class="card">
        <div class="card-header"><h5>Contract Dates</h5></div>
        <div class="card-body">
          <div *ngFor="let item of contractDates | orderBy:'date'" class="border-bottom py-2">
            <div class="d-flex justify-content-between">
              <div>
                <a [routerLink]="item.url">{{ item.title }}</a>
                <span class="badge bg-secondary ms-2">{{ item.type }}</span>
              </div>
              <span class="text-muted">{{ item.date | date:'shortDate' }}</span>
            </div>
          </div>
          <div *ngIf="!contractDates.length" class="text-muted">No upcoming dates.</div>
        </div>
      </div>
    </div>
    <div class="col-md-6">
      <div class="card">
        <div class="card-header"><h5>Obligations</h5></div>
        <div class="card-body">
          <div *ngFor="let item of obligationDates | orderBy:'date'" class="border-bottom py-2">
            <div class="d-flex justify-content-between">
              <div>
                <a [routerLink]="item.url">{{ item.title }}</a>
                <span class="badge bg-secondary ms-2">{{ item.type }}</span>
              </div>
              <span class="text-muted">{{ item.date | date:'shortDate' }}</span>
            </div>
          </div>
          <div *ngIf="!obligationDates.length" class="text-muted">No upcoming obligations.</div>
        </div>
      </div>
    </div>
  </div>
</div>
```

### Task 4: Add OrderBy Pipe (or use array sort)
Since Angular doesn't have built-in `orderBy`, either:
- Use `Array.sort()` in component: `this.contractDates.sort((a, b) => a.date.getTime() - b.date.getTime())`
- Or create a simple `orderBy` pipe

### Task 5: Color Coding
- Expired / Overdue: red text
- Due within 7 days: orange/yellow
- Due within 30 days: blue
- Due beyond 30 days: default

```typescript
dateColor(date: Date): string {
  const now = new Date();
  const diff = date.getTime() - now.getTime();
  const days = diff / (1000 * 60 * 60 * 24);
  if (days < 0) return 'text-danger';
  if (days < 7) return 'text-warning';
  if (days < 30) return 'text-primary';
  return '';
}
```

## Backend Work Required (out of scope)
1. **Renewal entity:** Create `ContractRenewal` aggregate with `ContractId`, `renewalDate`, `noticePeriodDays`, `autoRenew`, `status`.
2. **Cross-module date endpoint:** `GET /api/app/reports/critical-dates?days=30` returning unified list of contract, obligation, and renewal dates.
3. **Recurrence expansion endpoint:** For recurring obligations, expand the recurrence pattern into individual due dates for the queried range.

## Validation
- `ng build --configuration production` passes
- Critical dates page loads with 7/30/90 day filter
- Contracts and obligations are grouped by date
- Color coding applies correctly based on urgency
- Links navigate to correct contract/obligation detail
