# Plan 4: Unified Portfolio Dashboard

## Objective
Build a new dashboard module under `angular/src/app/reports/` that provides a single-pane view of contract portfolio health, obligation deadlines, review pipeline, and compliance risk.

## Backend Status
All modules expose list endpoints with filtering. No dedicated dashboard/aggregate endpoints exist.
- Contracts: `GET /api/app/contract` with `status`, `category`, `filter`
- Obligations: `GET /api/app/contract-obligation` with `status`, `dueDateFrom`, `dueDateTo`
- Reviews: `GET /api/app/review` with `status`, `priority`, `assignedUserId`
- **Gap:** No backend endpoints for aggregate counts, KPI calculations, or date-range queries across modules.

## Frontend Scope

### Task 1: Create Reports Module Structure
```
angular/src/app/reports/
  reports.component.ts          # shell with router-outlet
  reports.routes.ts             # lazy-loaded routes
  dashboard.component.ts        # main dashboard
  dashboard.service.ts          # aggregates list endpoint calls
  obligations-health.component.ts
  reviews-pipeline.component.ts
```

### Task 2: reports.component.ts
**File:** `angular/src/app/reports/reports.component.ts`
- Shell component with `<router-outlet>`
- Route guard: `permissionGuard` with `LegalTech.Dashboards.ViewRisk` or `LegalTech.Reports.Export`

### Task 3: reports.routes.ts
**File:** `angular/src/app/reports/reports.routes.ts`
```typescript
export const REPORTS_ROUTES: Routes = [
  {
    path: '',
    component: ReportsComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'LegalTech.Reports' },
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', loadComponent: () => import('./dashboard.component').then(c => c.DashboardComponent) },
      { path: 'obligations-health', loadComponent: () => import('./obligations-health.component').then(c => c.ObligationsHealthComponent) },
      { path: 'reviews-pipeline', loadComponent: () => import('./reviews-pipeline.component').then(c => c.ReviewsPipelineComponent) },
    ],
  },
];
```

### Task 4: Dashboard Service
**File:** `angular/src/app/reports/dashboard.service.ts`
- `getContractKPIs(): Observable<{ total: number; active: number; draft: number; expired: number; terminated: number }>`
- `getObligationKPIs(): Observable<{ total: number; pending: number; overdue: number; completed: number }>`
- `getReviewKPIs(): Observable<{ total: number; pending: number; escalated: number; completed: number }>`
- Implementation: call existing list endpoints with `maxResultCount: 0` or small page, derive counts from `totalCount` in `PagedResultDto`. This is efficient enough for MVP.
- Add `getExpiringContracts(days: number)`: call `contractService.getList` with filter + parse expiration dates client-side (or add date filter to backend later).
- Add `getOverdueObligations()`: call `obligationsService.getList` with `dueDateTo` = today.

### Task 5: Dashboard Component
**File:** `angular/src/app/reports/dashboard.component.ts`

**KPI Cards Row:**
- Total Contracts, Active Contracts, Overdue Obligations, Open Reviews, Escalated Reviews
- Each card shows count with trend indicator (optional for MVP)

**Critical Dates Section:**
- Table of contracts expiring in next 30 days
- Table of obligations due in next 7 days
- Color-coded urgency: red (>7 days overdue), yellow (due within 7 days), green (>30 days)

**Compliance Risk Section:**
- Contracts missing signatories (check `signatories.length === 0` from contract detail — requires loading full contract data or adding count endpoint)
- Contracts missing classification (check `classification` is undefined)
- **Workaround:** Load full contract list with small page size and filter client-side, or add dedicated dashboard endpoints later.

**Risk Heatmap:**
- Grid: X-axis = contract status, Y-axis = risk baseline
- Cell count = number of contracts in that bucket
- Implement as HTML table with background color intensity based on count

### Task 6: Obligations Health Component
**File:** `angular/src/app/reports/obligations-health.component.ts`
- Table of obligations with `dueDate` in next 14 days
- Group by contract
- Show priority, status, recurrence pattern
- Filter by status (Pending, Overdue, Completed)

### Task 7: Reviews Pipeline Component
**File:** `angular/src/app/reports/reviews-pipeline.component.ts`
- Kanban-style columns: Draft, InProgress, Completed, Escalated
- Cards show: title, contract, assignee, priority, due date
- Drag-and-drop is out of scope for MVP; simple column tables are fine.

### Task 8: Add Route to app.routes.ts
**File:** `angular/src/app/app.routes.ts`
```typescript
{
  path: 'reports',
  canActivate: [permissionGuard],
  data: { requiredPolicy: 'LegalTech.Reports' },
  loadChildren: () => import('./reports/reports.routes').then(c => c.REPORTS_ROUTES),
},
```

### Task 9: Add Localization Keys
**File:** `src/Acme.LegalTech.Domain.Shared/Localization/LegalTech/en.json`
Add keys for dashboard labels, KPI titles, date range labels.

## Backend Work Required (out of scope)
1. `GET /api/app/reports/contract-kpis` — aggregate counts by status, risk, owner
2. `GET /api/app/reports/obligation-kpis` — aggregate counts by status, due date range
3. `GET /api/app/reports/review-kpis` — aggregate counts by status, priority
4. `GET /api/app/reports/expiring-contracts?days=30`
5. `GET /api/app/reports/overdue-obligations`
6. `GET /api/app/reports/compliance-risk` — contracts missing signatories, classification, etc.

**Until then, the frontend derives aggregates from list endpoints.**

## Validation
- `ng build --configuration production` passes
- Dashboard loads without errors
- KPI cards display counts derived from list endpoints
- Expiring contracts table shows future dates correctly
- Overdue obligations are highlighted in red
