import { Injectable } from '@angular/core';
import { RestService, PagedResultDto } from '@abp/ng.core';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

export interface ContractCreateRequest {
  title: string;
  counterpartyName: string;
  category?: string;
  riskBaseline?: string;
  effectiveDate?: string;
  expirationDate?: string;
  ownerUserId?: string;
  documentNumber?: string;
  documentSeries?: string;
  documentYear?: number;
  classification?: DocumentClassification;
  retentionUntil?: string;
  contractValue?: number;
  tags?: Array<{ name: string }>;
  counterparties?: Array<{ name: string; externalReference?: string }>;
}

export interface ContractUpdateRequest {
  title: string;
  counterpartyName: string;
  category?: string;
  riskBaseline?: string;
  effectiveDate?: string;
  expirationDate?: string;
  ownerUserId?: string;
  documentNumber?: string;
  documentSeries?: string;
  documentYear?: number;
  classification?: DocumentClassification;
  retentionUntil?: string;
  contractValue?: number;
  tags?: Array<{ name: string }>;
  counterparties?: Array<{ name: string; externalReference?: string }>;
}

export interface ContractDto {
  id: string;
  title: string;
  counterpartyName: string;
  documentBlobName?: string | null;
  tenantId?: string | null;
  category?: string | null;
  status?: number;
  effectiveDate?: string | null;
  expirationDate?: string | null;
  ownerUserId?: string | null;
  riskBaseline?: string | null;
  documentNumber?: string | null;
  documentSeries?: string | null;
  documentYear?: number | null;
  classification?: DocumentClassification;
  retentionUntil?: string | null;
  contractValue?: number | null;
  tags?: Array<{ id: string; name: string }>;
  counterparties?: Array<{ id: string; name: string; externalReference?: string }>;
  documentVersions?: ContractDocumentVersionDto[];
  signatories?: ContractSignatoryDto[];
  variationOrders?: VariationOrderDto[];
  currentAuthority?: ApprovalAuthorityResultDto;
}

export interface ContractDocumentVersionDto {
  id: string;
  contractId: string;
  versionNumber: number;
  blobName: string;
  fileName: string;
  contentType: string;
  fileSize: number;
  uploadedById?: string | null;
  uploadedAt: string;
  isLatest: boolean;
  changeNote?: string | null;
  extractionStatus?: string | null;
  extractedTitle?: string | null;
  extractedCounterparty?: string | null;
  extractedEffectiveDate?: string | null;
  extractedExpirationDate?: string | null;
  extractedCategory?: string | null;
  extractedRiskBaseline?: string | null;
}

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
  approvedById?: string | null;
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

export interface ContractComplianceDto {
  documentNumber?: string | null;
  documentSeries?: string | null;
  documentYear?: number | null;
  classification: DocumentClassification;
  retentionUntil?: string | null;
  contractValue?: number | null;
  signatories: ContractSignatoryDto[];
  variationOrders: VariationOrderDto[];
  currentAuthority?: ApprovalAuthorityResultDto | null;
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

export interface ContractChangeStatusDto {
  targetStatus: ContractStatus;
  changeNote?: string;
}

export type ContractStatus = 0 | 1 | 2 | 3;

export const ContractStatusLabels: Record<ContractStatus, string> = {
  0: 'Draft',
  1: 'Active',
  2: 'Expired',
  3: 'Terminated',
};

export type DocumentClassification = 0 | 1 | 2 | 3;

export const DocumentClassificationLabels: Record<DocumentClassification, string> = {
  0: 'Unclassified',
  1: 'For Official Use Only',
  2: 'Confidential',
  3: 'Strictly Confidential',
};

export type GovernmentSignatoryRole = 0 | 1 | 2 | 3 | 4 | 5;

export const GovernmentSignatoryRoleLabels: Record<GovernmentSignatoryRole, string> = {
  0: 'Prepared By',
  1: 'Reviewed By',
  2: 'Endorsed By',
  3: 'Approved By',
  4: 'Authorized Signatory',
  5: 'Noted By',
};

export type DocumentPartyType = 0 | 1 | 2;

export const DocumentPartyTypeLabels: Record<DocumentPartyType, string> = {
  0: 'Government Unit',
  1: 'Individual',
  2: 'External',
};

export interface GetContractsInput {
  maxResultCount?: number;
  skipCount?: number;
  sorting?: string;
  filter?: string;
  status?: ContractStatus;
  category?: string;
  ownerUserId?: string;
}

@Injectable({
  providedIn: 'root',
})
export class ContractService {
  private readonly apiName = 'default';
  private readonly apiBaseUrl = 'https://localhost:44334';

  constructor(private restService: RestService) {}

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
    }, {
      apiName: this.apiName,
    });
  }

  create(input: ContractCreateRequest): Observable<ContractDto> {
    return this.restService.request<ContractCreateRequest, ContractDto>({
      method: 'POST',
      url: '/api/app/contract',
      body: input,
    }, {
      apiName: this.apiName,
    });
  }

  get(id: string): Observable<ContractDto> {
    return this.restService.request<null, ContractDto>({
      method: 'GET',
      url: `/api/app/contract/${id}`,
    }, {
      apiName: this.apiName,
    });
  }

  update(id: string, input: ContractUpdateRequest): Observable<ContractDto> {
    return this.restService.request<ContractUpdateRequest, ContractDto>({
      method: 'PUT',
      url: `/api/app/contract/${id}`,
      body: input,
    }, {
      apiName: this.apiName,
    });
  }

  changeStatus(id: string, input: ContractChangeStatusDto): Observable<ContractDto> {
    return this.restService.request<ContractChangeStatusDto, ContractDto>({
      method: 'POST',
      url: `/api/app/contract/change-status?id=${id}`,
      body: input,
    }, {
      apiName: this.apiName,
    }).pipe(
      catchError(error => {
        console.error('Failed to change contract status', error);
        return throwError(() => error);
      }),
    );
  }

  addSignatory(id: string, input: AddSignatoryDto): Observable<ContractSignatoryDto> {
    return this.restService.request<AddSignatoryDto, ContractSignatoryDto>({
      method: 'POST',
      url: `/api/app/contract/add-signatory?id=${id}`,
      body: input,
    }, {
      apiName: this.apiName,
    }).pipe(
      catchError(error => {
        console.error('Failed to add signatory', error);
        return throwError(() => error);
      }),
    );
  }

  addVariationOrder(id: string, input: AddVariationOrderDto): Observable<VariationOrderDto> {
    return this.restService.request<AddVariationOrderDto, VariationOrderDto>({
      method: 'POST',
      url: `/api/app/contract/add-variation-order?id=${id}`,
      body: input,
    }, {
      apiName: this.apiName,
    }).pipe(
      catchError(error => {
        console.error('Failed to add variation order', error);
        return throwError(() => error);
      }),
    );
  }

  getApprovalAuthority(id: string, amount: number): Observable<ApprovalAuthorityResultDto> {
    return this.restService.request<null, ApprovalAuthorityResultDto>({
      method: 'GET',
      url: `/api/app/contract/get-approval-authority?id=${id}&amount=${amount}`,
    }, {
      apiName: this.apiName,
    });
  }

  getCompliance(id: string): Observable<ContractComplianceDto> {
    return this.restService.request<null, ContractComplianceDto>({
      method: 'GET',
      url: `/api/app/contract/get-contract-compliance?id=${id}`,
    }, {
      apiName: this.apiName,
    });
  }

  getVersions(contractId: string) {
    return this.restService.request<null, { items: ContractDocumentVersionDto[] }>({
      method: 'GET',
      url: `/api/app/contract-document/versions/${contractId}`,
    }, {
      apiName: this.apiName,
    }).pipe(
      catchError(error => {
        console.error('Failed to load document versions', error);
        return throwError(() => error);
      }),
    );
  }

  upload(contractId: string, file: File, changeNote?: string) {
    const formData = new FormData();
    formData.append('File', file);
    if (changeNote) {
      formData.append('ChangeNote', changeNote);
    }

    return this.restService.request<FormData, ContractDocumentVersionDto>({
      method: 'POST',
      url: `/api/app/contract-document/upload/${contractId}`,
      body: formData,
    }, {
      apiName: this.apiName,
    }).pipe(
      catchError(error => {
        console.error('Failed to upload document', error);
        return throwError(() => error);
      }),
    );
  }

  deleteVersion(versionId: string) {
    return this.restService.request<null, null>({
      method: 'DELETE',
      url: `/api/app/contract-document/versions/${versionId}`,
    }, {
      apiName: this.apiName,
    }).pipe(
      catchError(error => {
        console.error('Failed to delete document version', error);
        return throwError(() => error);
      }),
    );
  }

  download(versionId: string): void {
    const url = `${this.apiBaseUrl}/api/app/contract-document/versions/download/${versionId}`;
    window.open(url, '_blank');
  }

  getDocumentDownloadUrl(versionId: string): string {
    return `${this.apiBaseUrl}/api/app/contract-document/versions/download/${versionId}`;
  }

  getBlob(versionId: string) {
    return this.restService.request<null, Blob>({
      method: 'GET',
      url: `/api/app/contract-document/versions/download/${versionId}`,
      responseType: 'blob' as 'json',
    }, {
      apiName: this.apiName,
    }).pipe(
      catchError(error => {
        console.error('Failed to fetch document blob', error);
        return throwError(() => error);
      }),
    );
  }

  createIngestionJob(contractDocumentVersionId: string, jobType: string, providerName?: string) {
    return this.restService.request<{ jobType: string; providerName?: string }, any>({
      method: 'POST',
      url: `/api/app/ai/ingestion-jobs/${contractDocumentVersionId}`,
      body: { jobType, providerName },
    }, {
      apiName: this.apiName,
    });
  }

  runIngestionJob(jobId: string) {
    return this.restService.request<null, any>({
      method: 'POST',
      url: `/api/app/ai/ingestion-jobs/${jobId}/run`,
    }, {
      apiName: this.apiName,
    });
  }

  getExtractionSuggestions(ingestionJobId: string) {
    return this.restService.request<null, { items: any[] }>({
      method: 'GET',
      url: `/api/app/ai/suggestions/extraction/${ingestionJobId}`,
    }, {
      apiName: this.apiName,
    });
  }

  decideExtractionSuggestion(suggestionId: string, decision: string, correctedValue?: string, comment?: string) {
    return this.restService.request<{ decision: string; correctedValue?: string; comment?: string }, any>({
      method: 'POST',
      url: `/api/app/ai/suggestions/extraction/${suggestionId}/decide`,
      body: { decision, correctedValue, comment },
    }, {
      apiName: this.apiName,
    });
  }

  getRiskSuggestions(ingestionJobId: string) {
    return this.restService.request<null, { items: any[] }>({
      method: 'GET',
      url: `/api/app/ai/suggestions/risk/${ingestionJobId}`,
    }, {
      apiName: this.apiName,
    });
  }

  decideRiskSuggestion(suggestionId: string, decision: string, correctedValue?: string, comment?: string) {
    return this.restService.request<{ decision: string; correctedValue?: string; comment?: string }, any>({
      method: 'POST',
      url: `/api/app/ai/suggestions/risk/${suggestionId}/decide`,
      body: { decision, correctedValue, comment },
    }, {
      apiName: this.apiName,
    });
  }
}
