# Frontend Implementation Plan — Module 04 Government Compliance (Angular)

## Current State
- Backend fully implements Module 04: `DocumentClassification`, `ContractValue`, `RetentionUntil`, `DocumentNumber/Series/Year`, `ContractSignatory`, `VariationOrder`, `GovernmentApprovalTier`, approval authority computation, status transitions.
- Angular frontend has basic CRUD only. Zero government compliance UI.
- `contract.service.ts` `ContractDto` interface is missing 10+ fields. `ContractCreateRequest` is used for both create and update, excluding gov fields.
- No filtering on list page. No status actions. No signatories/variation orders/compliance views.

## Decision: Inline-Template Components
The existing Angular components use inline templates (no `.component.html` files). **This plan follows that convention.** All templates stay inline in the `.component.ts` files.

## Decision: Manual Service Calls (No Proxy Generation)
The project uses hand-written `RestService.request()` calls rather than ABP generated proxies. **This plan extends the existing `contract.service.ts` manually.**

## Decision: No Confirmation/Toaster Services Yet
No `ConfirmationService` or `ToasterService` usage exists in the codebase. **Use native `confirm()` and console/alert for now.** These can be swapped to ABP services later.

## Task 1: Extend `contract.service.ts`

**File:** `angular/src/app/services/contract.service.ts`

Add new interfaces:
```typescript
export enum DocumentClassification {
  Unclassified = 0,
  ForOfficialUseOnly = 1,
  Confidential = 2,
  StrictlyConfidential = 3,
}

export const DocumentClassificationLabels: Record<DocumentClassification, string> = {
  [DocumentClassification.Unclassified]: 'Unclassified',
  [DocumentClassification.ForOfficialUseOnly]: 'For Official Use Only',
  [DocumentClassification.Confidential]: 'Confidential',
  [DocumentClassification.StrictlyConfidential]: 'Strictly Confidential',
};

export enum GovernmentSignatoryRole {
  PreparedBy = 0,
  ReviewedBy = 1,
  EndorsedBy = 2,
  ApprovedBy = 3,
  AuthorizedSignatory = 4,
  NotedBy = 5,
}

export const GovernmentSignatoryRoleLabels: Record<GovernmentSignatoryRole, string> = {
  [GovernmentSignatoryRole.PreparedBy]: 'Prepared By',
  [GovernmentSignatoryRole.ReviewedBy]: 'Reviewed By',
  [GovernmentSignatoryRole.EndorsedBy]: 'Endorsed By',
  [GovernmentSignatoryRole.ApprovedBy]: 'Approved By',
  [GovernmentSignatoryRole.AuthorizedSignatory]: 'Authorized Signatory',
  [GovernmentSignatoryRole.NotedBy]: 'Noted By',
};

export enum DocumentPartyType {
  GovernmentUnit = 0,
  Individual = 1,
  External = 2,
}

export const DocumentPartyTypeLabels: Record<DocumentPartyType, string> = {
  [DocumentPartyType.GovernmentUnit]: 'Government Unit',
  [DocumentPartyType.Individual]: 'Individual',
  [DocumentPartyType.External]: 'External',
};

export interface ContractSignatoryDto {
  id: string;
  contractId: string;
  role: GovernmentSignatoryRole;
  partyType: DocumentPartyType;
  partyId: string;
  governmentAgency: string;
  capacity: string;
  order: number;
  signedOn?: string | null;
}

export interface VariationOrderDto {
  id: string;
  contractId: string;
  description: string;
  amount: number;
  cumulativeAmount: number;
  approvedBy?: string | null;
  approvedOn?: string | null;
}

export interface ApprovalAuthorityResultDto {
  authorityTitle: string;
  requiresNedaReview: boolean;
  requiresPresident: boolean;
  allowableVariationPercent: number;
  lastApprovalAuthorityTitle?: string | null;
  lastApprovalRequiresNeda?: boolean;
  lastApprovalRequiresPresident?: boolean;
}

export interface AddSignatoryDto {
  role: GovernmentSignatoryRole;
  partyType: DocumentPartyType;
  partyId: string;
  governmentAgency: string;
  capacity: string;
  order: number;
  signedOn?: string | null;
}

export interface AddVariationOrderDto {
  description: string;
  amount: number;
}

// Extend ContractDto with all backend fields
export interface ContractDto {
  id: string;
  title: string;
  counterpartyName: string;
  documentBlobName?: string | null;
  tenantId?: string | null;
  category?: string | null;
  status: ContractStatus;
  effectiveDate?: string | null;
  expirationDate?: string | null;
  ownerUserId?: string | null;
  riskBaseline?: string | null;
  documentNumber?: string | null;
  documentSeries?: string | null;
  documentYear?: number | null;
  classification: DocumentClassification;
  retentionUntil?: string | null;
  contractValue?: number | null;
  tags?: Array<{ id: string; name: string }>;
  counterparties?: Array<{ id: string; name: string; externalReference?: string }>;
  documentVersions?: ContractDocumentVersionDto[];
  signatories?: ContractSignatoryDto[];
  variationOrders?: VariationOrderDto[];
  currentAuthority?: ApprovalAuthorityResultDto;
}
```

Update `getList` to pass filter/status/category/ownerUserId:
```typescript
getList(input: GetContractsInput = {}): Observable<PagedResultDto<ContractDto>> {
  return this.restService.request<null, PagedResultDto<ContractDto>>({
    method: 'GET',
    url: '/api/app/contract',
    params: {
      maxResultCount: input.maxResultCount ?? 10,
      skipCount: input.skipCount ?? 0,
      ...(input.sorting ? { sorting: input.sorting } : {}),
      ...(input.filter ? { filter: input.filter } : {}),
      ...(input.status !== undefined ? { status: input.status } : {}),
      ...(input.category ? { category: input.category } : {}),
      ...(input.ownerUserId ? { ownerUserId: input.ownerUserId } : {}),
    },
  }, { apiName: this.apiName });
}
```

Add new API methods:
```typescript
changeStatus(id: string, targetStatus: ContractStatus) {
  return this.restService.request<{ targetStatus: ContractStatus }, ContractDto>({
    method: 'POST',
    url: `/api/app/contract/${id}/change-status`,
    body: { targetStatus },
  }, { apiName: this.apiName });
}

addSignatory(id: string, input: AddSignatoryDto) {
  return this.restService.request<AddSignatoryDto, ContractSignatoryDto>({
    method: 'POST',
    url: `/api/app/contract/${id}/signatory`,
    body: input,
  }, { apiName: this.apiName });
}

addVariationOrder(id: string, input: AddVariationOrderDto) {
  return this.restService.request<AddVariationOrderDto, VariationOrderDto>({
    method: 'POST',
    url: `/api/app/contract/${id}/variation-order`,
    body: input,
  }, { apiName: this.apiName });
}

getApprovalAuthority(id: string, amount: number) {
  return this.restService.request<null, ApprovalAuthorityResultDto>({
    method: 'GET',
    url: `/api/app/contract/${id}/approval-authority?amount=${amount}`,
  }, { apiName: this.apiName });
}

getCompliance(id: string) {
  return this.restService.request<null, any>({
    method: 'GET',
    url: `/api/app/contract/${id}/compliance`,
  }, { apiName: this.apiName });
}
```

---

## Task 2: Update Contracts List — Add Filtering

**File:** `angular/src/app/contracts/contracts-list.component.ts`

Add filter controls above the table:
- Text filter (`input.filter`)
- Status dropdown (`input.status`) using `ContractStatusLabels`
- Category dropdown (dynamically populated or text input)

Wire the `ListService` query to pass these filters through the updated `getList` input type.

**UI pattern:** Use the existing `ListService` hook. Add `<input>` and `<select>` controls in the card-header or card-body that call `this.list.setQuery({...})` on change.

---

## Task 3: Update Create/Edit Form — Add Gov Fields

**File:** `angular/src/app/contracts/create-or-edit-contract.component.ts`

Add form controls:
- `classification` — dropdown (`DocumentClassification` enum)
- `contractValue` — number input
- `documentNumber` — text input
- `documentSeries` — text input
- `documentYear` — number input
- `retentionUntil` — date input (read-only display, computed from effectiveDate + 5y, but allow override)

**Critical:** Change `ContractCreateRequest` to a proper `ContractUpdateRequest` (or just use `Partial<ContractDto>`) so update sends all fields including gov fields. The backend `ContractUpdateDto` accepts all fields.

Update `patchForm` to populate the new fields from `ContractDto`.
Update `onSubmit` to send all form values on both create and update.

---

## Task 4: Update Detail Page — Add Gov Fields, Signatories, Variation Orders, Status Actions

**File:** `angular/src/app/contracts/contract-detail.component.ts`

### 4a. Add gov fields display
Add a new card section below the existing info `dl`:
- Document Number, Series, Year
- Classification (with `DocumentClassificationLabels`)
- Retention Until
- Contract Value (formatted as currency)
- Current Authority (if present): Authority title, NEDA/President flags, allowable variation %

### 4b. Add Signatories section
New card with table:
- Columns: Order, Role, Party Type, Party ID, Government Agency, Capacity, Signed On
- "Add Signatory" button (`*abpPermission="'LegalTech.Contracts.ManageSignatories'"`) opening a simple inline form or prompt
- Form fields: Role (select), PartyType (select), PartyId (text), GovernmentAgency (text), Capacity (text), Order (number), SignedOn (date)

### 4c. Add Variation Orders section
New card with table:
- Columns: Description, Amount, Cumulative Amount, Approved By, Approved On
- "Add Variation Order" button (`*abpPermission="'LegalTech.Contracts.Amend'"`) opening a simple inline form
- Form fields: Description (text), Amount (number)
- Show error if backend returns `ValueRequiredForVariation` or `ApprovedVariationLimitExceeded`

### 4d. Add Status Actions
Buttons in the card-header or a new action bar:
- "Activate" (`*abpPermission="'LegalTech.Contracts.ChangeStatus'"`) — only when status is Draft
- "Expire" — only when Active
- "Terminate" — only when Active or Expired
- Use native `confirm()` before calling `changeStatus`

---

## Task 5: Add Localization Keys

**File:** `src/Acme.LegalTech.Domain.Shared/Localization/LegalTech/en.json`

Add missing enum labels and UI strings:
```json
"Enum:DocumentClassification:0": "Unclassified",
"Enum:DocumentClassification:1": "For Official Use Only",
"Enum:DocumentClassification:2": "Confidential",
"Enum:DocumentClassification:3": "Strictly Confidential",
"Enum:GovernmentSignatoryRole:0": "Prepared By",
"Enum:GovernmentSignatoryRole:1": "Reviewed By",
"Enum:GovernmentSignatoryRole:2": "Endorsed By",
"Enum:GovernmentSignatoryRole:3": "Approved By",
"Enum:GovernmentSignatoryRole:4": "Authorized Signatory",
"Enum:GovernmentSignatoryRole:5": "Noted By",
"Enum:DocumentPartyType:0": "Government Unit",
"Enum:DocumentPartyType:1": "Individual",
"Enum:DocumentPartyType:2": "External",
"Contract:DocumentNumber": "Document Number",
"Contract:DocumentSeries": "Document Series",
"Contract:DocumentYear": "Document Year",
"Contract:Classification": "Classification",
"Contract:RetentionUntil": "Retention Until",
"Contract:ContractValue": "Contract Value",
"Contract:Signatories": "Signatories",
"Contract:VariationOrders": "Variation Orders",
"Contract:CurrentAuthority": "Current Approval Authority",
"Contract:AddSignatory": "Add Signatory",
"Contract:AddVariationOrder": "Add Variation Order",
"Contract:Activate": "Activate",
"Contract:Expire": "Expire",
"Contract:Terminate": "Terminate",
"Contract:StatusChangeConfirm": "Are you sure you want to change status to {Status}?",
"Contract:ApprovalAuthorityTitle": "Authority",
"Contract:RequiresNedaReview": "Requires NEDA Review",
"Contract:RequiresPresident": "Requires Presidential Approval",
"Contract:AllowableVariation": "Allowable Variation",
```

---

## Task 6: Route Guard Permission Tightening

**File:** `angular/src/app/contracts/contracts.routes.ts`

The detail route currently uses `LegalTech.Contracts` for access. Consider splitting:
- Viewing contract detail: `LegalTech.Contracts` (keep as-is)
- The new sub-features (signatories, variation orders, status change) use their own permissions via `*abpPermission` directives in the template.

No route changes strictly required, but the signatory/variation order API calls will fail server-side if the user lacks the specific permissions.

---

## Validation Steps

1. **Build:** `dotnet build` passes (0 errors, 0 warnings) — already verified.
2. **Angular build:** `cd angular && ng build` passes.
3. **List page:** Loads with filters. New columns (Classification, ContractValue, RetentionUntil, CurrentAuthority) visible.
4. **Create flow:** New gov fields save correctly. `ContractValue` set → variation orders can be added.
5. **Detail page:** Signatories table renders. Variation orders table renders. Status action buttons work.
6. **Permissions:** Buttons hidden when user lacks permission. API calls return 403 if accessed directly.
7. **Edge cases:**
   - `ContractValue` null → variation order add shows error toast/message.
   - Existing contracts without gov fields → form shows empty/default values gracefully.
   - Status transition violations → backend returns error, frontend displays it.

---

## Out of Scope (Deferred)
- ABP `ConfirmationService` / `ToasterService` integration (can be done in a follow-up)
- Separate compliance summary page (the `getCompliance` endpoint is available but not wired to a dedicated route)
- Edit signatories/delete signatories (only add is supported backend-side currently)
- Edit/delete variation orders (only add is supported backend-side currently)
- Approval tier admin UI (no CRUD endpoints exist for tiers yet — they are seed-only)
