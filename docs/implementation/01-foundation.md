# 01 Foundation and Platform Conventions

## Goal and MVP scope

Establish project-wide implementation standards, shared primitives, and first migration foundations required by all downstream modules.

## Current state delta

Current repository contains starter ABP scaffolding with partial Contract entity and template-level permissions. This module formalizes module boundaries and shared conventions.

## Domain model and ownership

- Shared constants and enums in Domain.Shared
- Permission tree root and module constants
- Route constants for Angular feature groups

## Vertical slices

### Slice 1: baseline refactor

- Retire starter placeholders not needed for CLM production scope.
- Introduce bounded-context folders for Contracts, Clauses, Reviews, Obligations, Reports, Files, and Administration.

### Slice 2: shared governance primitives

- Define permission constants and localization keys.
- Define common status enums and metadata value objects.

### Slice 3: first persistence baseline

- Register initial DbSet set for Contract context entities.
- Add migration naming and rollout conventions.

## Permissions and role checks

- Establish top-level permission groups for each module.
- Define baseline roles: HostAdmin, TenantAdmin, LegalOpsManager, LawyerReviewer, Auditor.

## Data rules and failure modes

- Reject startup if duplicate permission keys are defined.
- Block migration application when model drift is detected.

## Test and acceptance

- Validate permission discovery and assignment behavior.
- Validate migration applies cleanly on fresh database.

## Risk register

- Risk: foundation conventions drift across modules.
- Mitigation: enforce module templates and review checklist.

## Observability

- Add startup diagnostics for permission and migration health.

## Definition of done

- Shared constants merged.
- Permission tree committed.
- Baseline migration validated.
- Foundation BDD scenario drafted.
