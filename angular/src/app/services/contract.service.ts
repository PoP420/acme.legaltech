import { Injectable } from '@angular/core';
import { RestService, PagedResultDto } from '@abp/ng.core';
import { Observable } from 'rxjs';

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
  category?: string | null;
  status?: number;
  effectiveDate?: string | null;
  expirationDate?: string | null;
  ownerUserId?: string | null;
  riskBaseline?: string | null;
  tags?: Array<{ id: string; name: string }>;
  counterparties?: Array<{ id: string; name: string; externalReference?: string }>;
}

export interface GetContractsInput {
  maxResultCount?: number;
  skipCount?: number;
  sorting?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ContractService {
  private readonly apiName = 'default';

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
}
