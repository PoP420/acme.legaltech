# 09 Identity and OAuth Administration

## Goal and MVP scope

Implement host-side OpenIddict administration workflows for client and scope governance.

## Current state delta

OpenIddict runtime exists through ABP modules, but no operational management UI exists for platform admins.

## Domain model and ownership

Host-owned administration surface over OpenIddict entities:

- Application records
- Scope records

## Vertical slices

### Slice 1: host application services

- Add list/create/update/activate/deactivate for client applications.
- Add strict redirect URI and logout URI validation.

### Slice 2: scope administration

- Add scope list and update workflows for approved platform scopes.

### Slice 3: host UI screens

- Add applications list and editor.
- Add scopes list and editor.

## Permissions and role checks

- OpenIddictAdmin.Default
- OpenIddictAdmin.Applications
- OpenIddictAdmin.Scopes
- OpenIddictAdmin.Secrets

## Data rules and failure modes

- Reject invalid or duplicate redirect URIs.
- Protect required system clients from accidental disable.

## Test and acceptance

- Validate host-only access.
- Validate client lifecycle operations and URI checks.

## Risk register

- Risk: accidental disruption of auth clients.
- Mitigation: protected client policy and confirmation gates.

## Observability

- Audit client and scope changes with actor and timestamp.

## Definition of done

- Host-side app and scope administration complete.
- Safety checks for protected clients complete.
- BDD host-only scenarios complete.
