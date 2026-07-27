# Functional Requirements

## Requirement ID convention

- FR-CI: Contract intake and repository
- FR-CL: Clause library and playbooks
- FR-RW: Review workflow and negotiations
- FR-OR: Obligations and renewals
- FR-SR: Search, reporting, dashboards
- FR-SA: SaaS administration and packaging
- FR-FM: File and evidence management
- FR-ID: Identity and OAuth administration
- FR-AI: AI assist foundation

## Contract intake and repository

- FR-CI-001: The system shall allow authorized tenant users to create contract records with required metadata.
- FR-CI-002: The system shall support file upload and versioned document history per contract.
- FR-CI-003: The system shall support contract lifecycle status transitions with server-side validation.
- FR-CI-004: The system shall provide search by title, counterparty, category, owner, and status.

## Clause library and playbooks

- FR-CL-001: The system shall maintain reusable clause templates grouped by taxonomy.
- FR-CL-002: The system shall allow playbook definitions for preferred, fallback, and prohibited wording.
- FR-CL-003: The system shall evaluate contract clauses against playbook rules and return risk markers.

## Review workflow and negotiations

- FR-RW-001: The system shall create review tasks assignable by role and user.
- FR-RW-002: The system shall support approval chains with status checkpoints.
- FR-RW-003: The system shall store comments, decisions, and escalation events in an auditable timeline.

## Obligations and renewals

- FR-OR-001: The system shall track obligations linked to contract and clause source references.
- FR-OR-002: The system shall support one-time and recurring due-date schedules.
- FR-OR-003: The system shall notify responsible users before due dates and renewals.
- FR-OR-004: The system shall capture completion evidence and completion timestamps.

## Search, reporting, dashboards

- FR-SR-001: The system shall provide portfolio dashboards by lifecycle status and risk class.
- FR-SR-002: The system shall provide obligation and renewal health reports.
- FR-SR-003: The system shall export reports in spreadsheet format.

## SaaS administration and packaging

- FR-SA-001: The host shall provision new tenants with default roles and permissions.
- FR-SA-002: The host shall manage plan-based feature entitlements.
- FR-SA-003: Tenant users shall be restricted from host-only administration screens.

## File and evidence management

- FR-FM-001: The system shall store uploaded files via managed storage abstraction.
- FR-FM-002: The system shall enforce tenant boundary checks for file metadata and content access.
- FR-FM-003: The system shall support file category validation by type and size.

## Identity and OAuth administration

- FR-ID-001: The host shall manage OpenIddict client applications via UI-backed workflows.
- FR-ID-002: The system shall validate redirect and logout URI configuration before saving clients.
- FR-ID-003: The system shall support client activation and deactivation controls.

## AI assist foundation

- FR-AI-001: The system shall run extraction and scoring tasks asynchronously.
- FR-AI-002: The system shall present AI outputs with confidence and source references.
- FR-AI-003: The system shall require human accept or reject action before AI outputs become operational data.
- FR-AI-004: The system shall retain versioned history for accepted and corrected AI suggestions.

## Acceptance baseline

Each requirement must map to at least one of the following before release:

- Domain or application test
- Integration or API test
- UI interaction or E2E flow
- BDD scenario
