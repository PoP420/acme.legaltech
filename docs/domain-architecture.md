# Domain and Architecture

## Current-state baseline

Current repository baseline includes:

- ABP layered monolith structure
- Multi-tenancy support enabled
- Angular shell with ABP modules
- Initial Contract aggregate in domain
- Template-level permissions and no full CLM modules yet

## Target bounded contexts

1. Contracts
2. Clauses and Playbooks
3. Review Workflow
4. Obligations and Renewals
5. Reporting and Insights
6. File Management
7. SaaS Administration
8. Identity and OAuth Administration
9. AI Assist Services

## Architecture model

- Backend: ABP layered monolith (.NET, EF Core)
- Frontend: Angular with ABP UI packages
- Database: PostgreSQL
- Storage: ABP blob abstraction
- Async processing: ABP background jobs
- Optional AI integration: provider adapters and orchestration services

## Layer responsibilities

- Domain.Shared: constants, enums, permission names, localization keys
- Domain: aggregates, value objects, domain services, business invariants
- Application.Contracts: DTOs and service interfaces
- Application: orchestration, authorization, use cases
- EntityFrameworkCore: mappings, indexes, migrations
- HttpApi: API endpoints via app services
- Angular: route-level modules and permission-aware UI

## Cross-cutting design rules

1. Keep tenant boundary checks explicit at app service and repository layers.
2. Keep long-running operations asynchronous.
3. Use immutable snapshots for high-impact published outputs.
4. Keep domain free from infrastructure-specific dependencies.

## Integration boundaries

- Email and notifications: ABP abstractions
- AI providers: adapter interfaces and background orchestration only
- External systems: integration module in later phase to avoid leaking concerns into core domain
