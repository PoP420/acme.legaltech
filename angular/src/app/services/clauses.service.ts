import { Injectable } from '@angular/core';
import { RestService, PagedResultDto } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface ClauseTemplateDto {
  id: string;
  tenantId?: string;
  title: string;
  content: string;
  taxonomyId?: string;
  taxonomyName?: string;
  isActive: boolean;
  version: number;
  jurisdiction?: string | null;
  category?: string | null;
  tags?: string | null;
  riskLevel?: string | null;
}

export interface ClauseTemplateCreateDto {
  title: string;
  content: string;
  taxonomyId?: string;
  jurisdiction?: string;
  category?: string;
  tags?: string;
  riskLevel?: string;
}

export interface ClauseTemplateUpdateDto {
  title: string;
  content: string;
  taxonomyId?: string;
  jurisdiction?: string;
  category?: string;
  tags?: string;
  riskLevel?: string;
}

export interface ClauseGetListInput {
  maxResultCount?: number;
  skipCount?: number;
  sorting?: string;
  filter?: string;
  isActive?: boolean;
  taxonomyId?: string;
  category?: string;
  jurisdiction?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ClausesService {
  private readonly apiName = 'default';

  constructor(private restService: RestService) {}

  getList(input: ClauseGetListInput = {}): Observable<PagedResultDto<ClauseTemplateDto>> {
    return this.restService.request<null, PagedResultDto<ClauseTemplateDto>>({
      method: 'GET',
      url: '/api/app/clause-template',
      params: {
        maxResultCount: input.maxResultCount ?? 10,
        skipCount: input.skipCount ?? 0,
        ...(input.sorting ? { sorting: input.sorting } : {}),
        ...(input.filter ? { filter: input.filter } : {}),
        ...(input.isActive !== undefined ? { isActive: input.isActive } : {}),
        ...(input.taxonomyId ? { taxonomyId: input.taxonomyId } : {}),
        ...(input.category ? { category: input.category } : {}),
        ...(input.jurisdiction ? { jurisdiction: input.jurisdiction } : {}),
      },
    }, {
      apiName: this.apiName,
    });
  }

  create(input: ClauseTemplateCreateDto): Observable<ClauseTemplateDto> {
    return this.restService.request<ClauseTemplateCreateDto, ClauseTemplateDto>({
      method: 'POST',
      url: '/api/app/clause-template',
      body: input,
    }, {
      apiName: this.apiName,
    });
  }

  update(id: string, input: ClauseTemplateUpdateDto): Observable<ClauseTemplateDto> {
    return this.restService.request<ClauseTemplateUpdateDto, ClauseTemplateDto>({
      method: 'PUT',
      url: `/api/app/clause-template/${id}`,
      body: input,
    }, {
      apiName: this.apiName,
    });
  }

  delete(id: string): Observable<void> {
    return this.restService.request<void, void>({
      method: 'DELETE',
      url: `/api/app/clause-template/${id}`,
    }, {
      apiName: this.apiName,
    });
  }
}