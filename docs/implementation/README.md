# Acme LegalTech Implementation Index

## Purpose

This folder contains feature-by-feature execution plans for implementing Acme LegalTech as a business-ready CLM platform.

## Global decisions

- Keep ABP layered monolith architecture.
- Keep strict tenant isolation for all tenant-owned data.
- Use background jobs for extraction, scoring, and heavy reporting workflows.
- Use ABP blob abstractions for file storage interactions.
- Use explicit permission trees per module.
- Keep AI assist human-approved before data promotion.

## Mandatory sections in each module

- Goal and MVP scope
- Current state delta
- Domain model and ownership
- Vertical slice implementation
- Permissions and role checks
- Data rules and failure modes
- Test cases and acceptance criteria
- Risk register
- Observability and operational checks
- Definition of done
- BDD scenarios

## Recommended implementation order

1. [01-foundation.md](./01-foundation.md)
2. [02-contract-intake-repository.md](./02-contract-intake-repository.md)
3. [03-clause-library-playbooks.md](./03-clause-library-playbooks.md)
4. [04-review-workflow-negotiations.md](./04-review-workflow-negotiations.md)
5. [05-obligations-renewals.md](./05-obligations-renewals.md)
6. [06-search-reports-dashboards.md](./06-search-reports-dashboards.md)
7. [07-saas-administration-packaging.md](./07-saas-administration-packaging.md)
8. [08-file-evidence-management.md](./08-file-evidence-management.md)
9. [09-identity-oauth-administration.md](./09-identity-oauth-administration.md)
10. [10-ai-assist-foundation.md](./10-ai-assist-foundation.md)

## Shared quality gates

A module is complete only when:

1. Domain and application behavior implemented and reviewed.
2. Required permission checks are enforced server-side.
3. Migrations are validated in local and staged environments.
4. Test coverage includes unit, integration, and one BDD scenario.
5. Audit and operational logging are validated.
6. Documentation is updated in both this folder and product-level docs.
