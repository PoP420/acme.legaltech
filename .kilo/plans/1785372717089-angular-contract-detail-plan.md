# Angular Contract Detail Page — Implementation Plan

**Scope:** Update `contract-detail.component.ts` (inline template + class) to display contract details, document versions with extraction results, and support document upload. Ensure consistency with existing ABP Angular patterns.

## Current State

- `contract-detail.component.ts` is a stub component (19 lines)
- `ContractService` provides CRUD but no document methods
- `ContractDto` in frontend is missing fields present in backend DTO
- Document upload/download/delete API exists backend (`/api/app/contract-document`)
- Extraction metadata is returned in `ContractDocumentVersionDto` from `GetVersionsAsync`
- All components use inline templates (no separate `.html` files)
- List pages use `ListService`, permission directives, and Bootstrap classes

## Target State

Detail page shows:
- Contract metadata (title, counterparty, status, category, risk, dates, owner)
- Tags and counterparties
- Document versions table with filename, size, date, extraction status, and extracted fields
- Upload button triggering a file picker and POST to upload API
- Download and delete actions per version

## Decisions Required

**Question 1:** Where should document-upload live?

- **A.** Inline form on the detail page (consistent with current inline-form pattern; fastest)
- **B.** Separate create-or-edit style modal (requires `ModalService`, not used in current codebase)
- **C.** Navigate to a dedicated upload route/component (adds routing complexity)

**Recommended: A**

Inline upload with a simple `<input type="file">` and upload button requires the least new infrastructure and matches the project's current "all-in-one" component style.

## Implementation Steps

1. **Extend `ContractDto` interface** in `angular/src/app/services/contract.service.ts`
   - Add: `documentBlobName`, `tags`, `counterparties`, `tenantId`
   - Add `status` as `number` (already present, verify)
   - Add `uploadedAt` to document version type if creating a new DTO

2. **Add document service methods** (either extend `ContractService` or create `ContractDocumentService`)
   - `getVersions(contractId)`: GET `/api/app/contract-document/versions/{contractId}`
   - `upload(contractId, file, changeNote)`: POST `/api/app/contract-document/upload/{contractId}` as multipart
   - `download(versionId)`: GET `/api/app/contract-document/download/{versionId}`
   - `delete(versionId)`: DELETE `/api/app/contract-document/delete/{versionId}`
   - Handle multipart body with `FormData` via `RestService`

3. **Update `contract-detail.component.ts`**
   - Inject `ActivatedRoute`, `ContractService`, and document service
   - Load contract by `id` param on init using `contractService.get(id)`
   - Load document versions using `getVersions(contractId)`
   - Add upload handler: read file via `File` API, `FormData`, call upload endpoint, refresh versions
   - Add format helpers: status enum display, date formatting

4. **Rewrite inline template** to display:
   - **Header:** contract title + status badge
   - **Info section:** table or definition list for metadata fields
   - **Documents section:** table listing versions
     - Columns: Version, Filename, Size, Uploaded, Extraction Status, Extracted Title, Extracted Counterparty, Extracted Dates, Category, Risk
     - Actions: Download (anchor with blob URL or direct API), Delete (button with permission directive)
   - **Upload section:** file input + change note input + upload button gated by `*abpPermission="'LegalTech.Contracts.AttachDocument'"`

5. **Extract extraction status presentation**
   - Show "Success" badge on green, "Failed"/"Error" on red, blank when no extraction
   - Display extracted fields in a collapsible "Extraction Details" sub-row or card per version

6. **Route and permission check**
   - Detail route already protected by `LegalTech.Contracts` at parent level
   - Add `*abpPermission` directives on upload/delete for `LegalTech.Contracts.AttachDocument`
   - Add `*abpPermission` on download for `LegalTech.Contracts.Default`

7. **Add minimal styling**
   - Use Bootstrap classes (`.table`, `.badge`, `.btn`, `.card`, `.form-control`, `.form-label`) consistent with LeptonX
   - No new SCSS required

## Files Changed

- `angular/src/app/services/contract.service.ts` — extend DTO + add document methods (or new service)
- `angular/src/app/contracts/contract-detail.component.ts` — full rewrite of template + class
- `angular/src/app/contracts/contracts-list.component.ts` — optional: update link text/hover to show status
- `angular/src/app/contracts/contracts.routes.ts` — no change needed

## Validation

- Navigate to `/contracts/:id` from list link
- Verify contract metadata renders correctly from seeded/created data
- Upload a PDF/docx/doc; verify file appears in versions list with extraction status
- If Azure DI is configured, verify extracted fields populate
- Verify download returns file stream
- Verify delete removes version
- Verify guards: user without `AttachDocument` cannot see upload/delete controls

## Out of Scope (do not implement)

- Angular proxy generation (`abp generate-proxy -t ng`)
- Modal upload flow
- Separate upload component
- Editing contract details from detail page (already has `/edit/:id`)
- Extraction status polling / background jobs UI

## Open Question

None. Proceed with implementation.
