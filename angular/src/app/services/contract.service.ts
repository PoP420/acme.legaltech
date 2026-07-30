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
  tags?: Array<{ id: string; name: string }>;
  counterparties?: Array<{ id: string; name: string; externalReference?: string }>;
  documentVersions?: ContractDocumentVersionDto[];
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

export type ContractStatus = 0 | 1 | 2 | 3;

export const ContractStatusLabels: Record<ContractStatus, string> = {
  0: 'Draft',
  1: 'Active',
  2: 'Expired',
  3: 'Terminated',
};

export interface GetContractsInput {
  maxResultCount?: number;
  skipCount?: number;
  sorting?: string;
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

  update(id: string, input: Partial<ContractCreateRequest>): Observable<ContractDto> {
    return this.restService.request<Partial<ContractCreateRequest>, ContractDto>({
      method: 'PUT',
      url: `/api/app/contract/${id}`,
      body: input,
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
    formData.append('file', file);
    if (changeNote) {
      formData.append('changeNote', changeNote);
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
      url: `/api/app/contract-document/${versionId}`,
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
    const url = `${this.apiBaseUrl}/api/app/contract-document/download/${versionId}`;
    window.open(url, '_blank');
  }
}
