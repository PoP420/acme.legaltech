# Non-Functional Requirements

## Requirement ID convention

- NFR-PERF: performance
- NFR-AVAIL: availability
- NFR-SCALE: scalability
- NFR-SEC: security and privacy
- NFR-AUD: auditability
- NFR-OPS: operations and observability
- NFR-MAINT: maintainability

## Performance

- NFR-PERF-001: Standard CRUD API requests should target p95 response time under 500 ms at normal load.
- NFR-PERF-002: Search queries should target p95 under 1200 ms for baseline portfolio sizes.
- NFR-PERF-003: Long-running extraction and report jobs must execute asynchronously.

## Availability and recovery

- NFR-AVAIL-001: Production service target availability shall be at least 99.5 percent for v1.
- NFR-AVAIL-002: Backup and restore procedures shall be documented and tested quarterly.
- NFR-AVAIL-003: Critical failures shall produce operator alerts within 5 minutes.

## Scalability

- NFR-SCALE-001: Tenant isolation architecture shall support horizontal scale without tenant data mixing.
- NFR-SCALE-002: Background jobs shall be queue-based and independently scalable.

## Security and privacy

- NFR-SEC-001: Every protected endpoint shall enforce authentication and authorization.
- NFR-SEC-002: Tenant data access shall be isolated by tenant identifier checks at repository and service boundaries.
- NFR-SEC-003: Secrets shall be stored outside source code and rotated by policy.
- NFR-SEC-004: Data retention and deletion policy controls shall be configurable per tenant policy profile.

## Auditability and compliance readiness

- NFR-AUD-001: Contract, review, obligation, and result-changing actions shall create audit trail entries.
- NFR-AUD-002: Audit records shall include actor, timestamp, tenant context, action type, and affected entity.
- NFR-AUD-003: Legal hold mode shall block destructive operations on held records.

## Operations and observability

- NFR-OPS-001: Application logs shall include correlation identifiers for request tracing.
- NFR-OPS-002: Metrics shall include API latency, job queue depth, failure rate, and tenant-level usage counters.
- NFR-OPS-003: Error alerts shall be configured for sustained failure patterns.

## Maintainability

- NFR-MAINT-001: All major modules shall include automated tests and architecture documentation updates.
- NFR-MAINT-002: Public contracts for APIs and DTOs shall be versioned and backward compatibility reviewed.
- NFR-MAINT-003: Database migrations shall include rollback considerations and upgrade notes.
