# 08 File and Evidence Management

## Goal and MVP scope

Implement secure managed file workflows for contracts, review evidence, and generated artifacts.

## Current state delta

A simple blob setup approach is planned, but no full managed file model is implemented.

## Domain model and ownership

Primary aggregate:

- ManagedFile (tenant-owned by default, host-owned optional)

Supporting metadata:

- file category
- owner type and owner id
- retention profile

## Vertical slices

### Slice 1: storage and metadata services

- Define container strategy and naming conventions.
- Add metadata persistence and integrity checks.

### Slice 2: access-controlled APIs

- Upload, list, download, and delete operations.
- Owner and tenant boundary validation.

### Slice 3: UI integrations

- Add file picker and upload flows in contracts and review modules.

## Permissions and role checks

- Files.Default
- Files.Upload
- Files.Download
- Files.Delete
- Files.ManageAll

## Data rules and failure modes

- Block unsupported type and oversize uploads.
- Prevent delete for files referenced by active records unless policy allows replacement.

## Test and acceptance

- Validate access control by tenant and owner.
- Validate upload/download lifecycle and metadata integrity.

## Risk register

- Risk: orphaned blobs or metadata drift.
- Mitigation: transactional coordination and cleanup jobs.

## Observability

- Track upload failures, denied downloads, and retention actions.

## Definition of done

- Managed file APIs complete.
- Contract and review integration complete.
- Permission and retention checks validated.
