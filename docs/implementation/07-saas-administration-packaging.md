# 07 SaaS Administration and Packaging

## Goal and MVP scope

Implement host-side tenant provisioning, feature packaging, and baseline governance controls.

## Current state delta

ABP tenant management exists, but CLM-specific provisioning and plan model are not implemented.

## Domain model and ownership

Host-owned entities:

- SubscriptionPlan
- PlanFeatureToggle
- TenantProvisioningProfile

## Vertical slices

### Slice 1: tenant provisioning

- Add provisioning workflow for tenant defaults: roles, permissions, starter settings.

### Slice 2: plan and feature gating

- Define Starter, Professional, Enterprise packages.
- Map features to plan entitlements.

### Slice 3: host admin UI

- Add plan matrix and tenant provisioning status views.

## Permissions and role checks

- SaaSAdministration.Default
- SaaSAdministration.Tenants
- SaaSAdministration.PlanManagement

## Data rules and failure modes

- Ensure provisioning is idempotent and retry-safe.
- Block destructive plan changes without compatibility checks.

## Test and acceptance

- Validate provisioning defaults for new tenants.
- Validate feature gating by plan.

## Risk register

- Risk: plan changes break active tenant workflows.
- Mitigation: staged rollout and compatibility rules.

## Observability

- Track provisioning success/failure and feature-gate deny events.

## Definition of done

- Tenant provisioning flow complete.
- Plan feature gates enforced in backend and UI.
- Host administration screens complete.
