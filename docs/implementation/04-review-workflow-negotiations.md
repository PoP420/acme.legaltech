# 04 Review Workflow and Negotiations

## Goal and MVP scope

Implement legal review assignments, approval stages, decision logging, and escalation handling for contract negotiation workflows.

## Current state delta

No review workflow module currently exists.

## Domain model and ownership

Primary aggregate:

- ReviewCase (tenant-owned)

Supporting entities:

- ReviewTask
- ApprovalStep
- ReviewComment
- EscalationEvent

## Vertical slices

### Slice 1: review orchestration

- Create review case from contract.
- Add assignable tasks and task statuses.
- Add approval step progression rules.

### Slice 2: negotiation records

- Add comment timeline and decision entries.
- Add escalation workflow with severity and owner.

### Slice 3: Angular review console

- Add queue and case detail screens.
- Add action controls for approve, request changes, escalate.

## Permissions and role checks

- Reviews.Default
- Reviews.Assign
- Reviews.Decide
- Reviews.Escalate
- Reviews.AuditView

## Data rules and failure modes

- Block approval if required steps are incomplete.
- Prevent case closure while open escalations remain unresolved.

## Test and acceptance

- Validate task assignment and stage progression.
- Validate escalation flow and closure rules.

## Risk register

- Risk: unclear accountability during handoffs.
- Mitigation: explicit owner fields and escalation SLA timers.

## Observability

- Audit every decision and escalation transition.

## Definition of done

- Review queue and workflow APIs complete.
- Decision timeline and escalation model complete.
- BDD scenarios for approvals and escalations complete.
