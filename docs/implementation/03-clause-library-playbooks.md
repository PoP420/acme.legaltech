# 03 Clause Library and Playbooks

## Goal and MVP scope

Implement reusable clause catalog and policy playbooks for rule-based contract review guidance.

## Current state delta

No clause/playbook module currently exists.

## Domain model and ownership

Primary aggregates:

- ClauseTemplate (tenant-owned, host-owned optional catalog)
- PlaybookProfile (tenant-owned)

Supporting entities:

- ClauseTaxonomy
- PlaybookRule
- RuleSeverity

## Vertical slices

### Slice 1: clause repository

- Add clause CRUD with taxonomy and versioning.
- Add search by taxonomy and jurisdiction.

### Slice 2: playbook engine

- Add playbook profile with preferred/fallback/prohibited rules.
- Add clause evaluation service returning risk markers.

### Slice 3: Angular policy workspace

- Add clause list and playbook editor screens.
- Add contract-side comparison view for review preparation.

## Permissions and role checks

- Clauses.Default
- Clauses.Manage
- Playbooks.Default
- Playbooks.Manage
- Playbooks.Evaluate

## Data rules and failure modes

- Prevent duplicate active playbook names per tenant.
- Reject prohibited rule configuration without severity and rationale.

## Test and acceptance

- Validate clause CRUD and taxonomy retrieval.
- Validate playbook evaluation outputs for sample clauses.

## Risk register

- Risk: inconsistent rule outcomes across similar clauses.
- Mitigation: deterministic matching strategy and test fixtures.

## Observability

- Log evaluation requests and output severity counts.

## Definition of done

- Clause repository and playbook management complete.
- Evaluation API and contract comparison view complete.
- BDD scenarios for rule outcomes complete.
