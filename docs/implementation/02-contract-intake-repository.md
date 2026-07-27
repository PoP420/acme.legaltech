# 02 Contract Intake and Repository

## Goal and MVP scope

Implement contract registration, metadata management, document association, lifecycle transitions, and retrieval operations.

## Current state delta

A minimal Contract aggregate exists. This module expands to production-grade fields, persistence, and user workflows.

## Domain model and ownership

Primary aggregate: Contract (tenant-owned)

Supporting entities:

- ContractDocumentVersion
- CounterpartyReference
- ContractTag

## Vertical slices

### Slice 1: domain and persistence

- Expand Contract fields (status, effective dates, owner, category, risk baseline).
- Add document version entity and indexes for retrieval.
- Register DbSet and EF mappings.

### Slice 2: application services and APIs

- Create contract CRUD plus lifecycle transition commands.
- Add document attach and version actions.
- Add paginated repository search API.

### Slice 3: Angular contract workspace

- Add routes for list, create, detail, and versions.
- Add metadata form and version timeline component.

## Permissions and role checks

- Contracts.Default
- Contracts.Create
- Contracts.Edit
- Contracts.ChangeStatus
- Contracts.AttachDocument

## Data rules and failure modes

- Prevent invalid lifecycle transitions.
- Reject document upload if unsupported file type.
- Ensure latest version marker remains unique per contract.

## Test and acceptance

- Validate contract creation and status transitions.
- Validate search filters and pagination.
- Validate version history integrity.

## Risk register

- Risk: contract metadata inconsistency.
- Mitigation: server-side validation and required field policy.

## Observability

- Emit audit events for create, update, status change, and document version add.

## Definition of done

- Contract CRUD and lifecycle APIs complete.
- Versioned document workflow complete.
- UI list/detail/version flows complete.
- BDD coverage for intake lifecycle complete.
