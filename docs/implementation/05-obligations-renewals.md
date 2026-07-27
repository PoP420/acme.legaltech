# 05 Obligations and Renewals

## Goal and MVP scope

Implement obligation tracking, due-date management, completion evidence capture, and renewal reminders.

## Current state delta

No obligations or renewals module currently exists.

## Domain model and ownership

Primary aggregates:

- ContractObligation (tenant-owned)
- RenewalSchedule (tenant-owned)

Supporting entities:

- ObligationReminder
- CompletionEvidence

## Vertical slices

### Slice 1: obligation records

- Create obligations linked to contract and source clause references.
- Support one-time and recurring schedules.

### Slice 2: reminder and completion flow

- Generate reminder tasks before due dates.
- Capture completion status and evidence metadata.

### Slice 3: renewal workspace

- Show upcoming renewals and overdue obligations.
- Provide list actions for complete, defer, and escalate.

## Permissions and role checks

- Obligations.Default
- Obligations.Manage
- Obligations.Complete
- Renewals.Default
- Renewals.Manage

## Data rules and failure modes

- Prevent completion without required minimal evidence fields.
- Prevent renewal close if approval requirement is configured and not met.

## Test and acceptance

- Validate schedule generation.
- Validate reminder state progression.
- Validate completion and audit history.

## Risk register

- Risk: missed obligations due to reminder gaps.
- Mitigation: multi-stage reminders and overdue monitoring dashboard.

## Observability

- Track due soon, overdue, and completed metrics.

## Definition of done

- Obligation and renewal CRUD complete.
- Reminder and completion workflows complete.
- Dashboard tiles and BDD flow coverage complete.
