# 06 Search Reports and Dashboards

## Goal and MVP scope

Implement operational search, reporting exports, and management dashboards for legal operations visibility.

## Current state delta

Current platform has no CLM analytics or reporting workflows.

## Domain model and ownership

Reporting in v1 is query-model based over existing transactional modules.

Possible supporting entity:

- ReportJob (for asynchronous export tracking)

## Vertical slices

### Slice 1: search and query layer

- Add contract and clause search endpoints with combined metadata filters.
- Add saved filter presets for key operational views.

### Slice 2: reporting exports

- Add exports for portfolio summary, obligations health, renewal pipeline, and risk distribution.
- Use asynchronous export path for large datasets.

### Slice 3: Angular dashboards

- Add dashboards by role with KPI cards and drill-down views.

## Permissions and role checks

- Reports.Default
- Reports.Export
- Dashboards.Default
- Dashboards.ViewRisk

## Data rules and failure modes

- Reject report generation when required filters are missing.
- Prevent internal-only report exposure to read-only roles without permission.

## Test and acceptance

- Validate search filter correctness.
- Validate report export payload integrity.
- Validate dashboard counters against query results.

## Risk register

- Risk: expensive queries under growth.
- Mitigation: indexed filters, pagination, and asynchronous exports.

## Observability

- Track export durations, failures, and queue depth.

## Definition of done

- Search API and UI flows complete.
- Core exports complete.
- KPI dashboard complete with validated metrics.
