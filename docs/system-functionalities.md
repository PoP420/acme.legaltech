# System Functionalities Catalog

## Scope baseline

This catalog defines target v1 system capabilities for Acme LegalTech. It includes core CLM operations, platform administration, and AI-assist workflows.

## Functional modules

## 1. Contract intake and repository

- Create contract record and metadata profile
- Upload source files and maintain document versions
- Associate counterparties and contract categories
- Maintain lifecycle statuses from draft to archived
- Full-text and metadata-based retrieval

## 2. Clause library and policy playbooks

- Standard clause catalog by type and jurisdiction profile
- Playbook rules for preferred, fallback, and prohibited language
- Clause comparison workflow against standards
- Risk indicators by deviation severity

## 3. Review workflow and negotiations

- Review queue and assignment model
- Multi-step approval chains
- Internal comments and decision logging
- Version-aware redline and review states
- Escalation handling for high-risk contracts

## 4. Obligations and renewals

- Obligation registration from contracts and clauses
- Due-date scheduling and recurrence support
- SLA tracking and completion workflows
- Renewal timeline management and reminders

## 5. Search, reporting, and dashboards

- Contract portfolio dashboards by status, risk, and owner
- Renewal pipeline and obligation health views
- Operational exports for audit and management
- KPI trend reporting per tenant and time period

## 6. Tenant administration and packaging

- Tenant setup and baseline role provisioning
- Plan and feature entitlement controls
- Host-managed defaults and policy templates

## 7. File and evidence management

- Secure file upload and controlled download
- Metadata and ownership tracking
- Category-based validation and retention behavior

## 8. Identity and OAuth administration

- Host-side OpenIddict application management
- Scope configuration and URI validation
- Client lifecycle controls for internal integrations

## 9. AI assist foundation

- Asynchronous document ingestion and extraction jobs
- Clause extraction suggestions with confidence scores
- Risk scoring suggestions against playbooks
- Human-in-the-loop acceptance and correction workflow
- Retrieval-augmented search over approved content

## User roles

- Host Admin
- Tenant Admin
- Legal Ops Manager
- Lawyer / Reviewer
- Read-only Auditor

## Deliverables produced by these modules

- Working CLM workflows across intake, review, and renewals
- Controlled security and administration model
- Report and KPI outputs for operational management
- AI assist features that accelerate work while preserving legal oversight
