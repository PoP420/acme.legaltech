import { Injectable } from '@angular/core';
import { RestService, PagedResultDto } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface PlaybookProfileDto {
  id: string;
  tenantId?: string;
  name: string;
  description?: string | null;
  isActive: boolean;
  version: number;
  rules?: PlaybookRuleDto[];
}

export interface PlaybookRuleDto {
  id: string;
  playbookId: string;
  playbookName: string;
  name: string;
  description: string;
  clausePattern: string;
  severity: number;
  rationale?: string | null;
  isPreferred: boolean;
  isFallback: boolean;
  isProhibited: boolean;
  sortOrder: number;
}

export interface PlaybookProfileCreateDto {
  name: string;
  description?: string;
}

export interface PlaybookProfileUpdateDto {
  name: string;
  description?: string;
}

export interface PlaybookEvaluateInput {
  contractId: string;
  clauseText: string;
  playbookId?: string;
}

export interface PlaybookEvaluationResultDto {
  ruleId: string;
  ruleName: string;
  severity: number;
  matched: boolean;
  matchSpan?: string | null;
  rationale?: string | null;
  isPreferred: boolean;
  isFallback: boolean;
  isProhibited: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class PlaybooksService {
  private readonly apiName = 'default';

  constructor(private restService: RestService) {}

  getList(input: { maxResultCount?: number; skipCount?: number; sorting?: string } = {}): Observable<PagedResultDto<PlaybookProfileDto>> {
    return this.restService.request<null, PagedResultDto<PlaybookProfileDto>>({
      method: 'GET',
      url: '/api/app/playbook',
      params: {
        maxResultCount: input.maxResultCount ?? 10,
        skipCount: input.skipCount ?? 0,
        ...(input.sorting ? { sorting: input.sorting } : {}),
      },
    }, {
      apiName: this.apiName,
    });
  }

  create(input: PlaybookProfileCreateDto): Observable<PlaybookProfileDto> {
    return this.restService.request<PlaybookProfileCreateDto, PlaybookProfileDto>({
      method: 'POST',
      url: '/api/app/playbook',
      body: input,
    }, {
      apiName: this.apiName,
    });
  }

  update(id: string, input: PlaybookProfileUpdateDto): Observable<PlaybookProfileDto> {
    return this.restService.request<PlaybookProfileUpdateDto, PlaybookProfileDto>({
      method: 'PUT',
      url: `/api/app/playbook/${id}`,
      body: input,
    }, {
      apiName: this.apiName,
    });
  }

  delete(id: string): Observable<void> {
    return this.restService.request<void, void>({
      method: 'DELETE',
      url: `/api/app/playbook/${id}`,
    }, {
      apiName: this.apiName,
    });
  }

  evaluate(input: PlaybookEvaluateInput): Observable<PlaybookEvaluationResultDto[]> {
    return this.restService.request<PlaybookEvaluateInput, PlaybookEvaluationResultDto[]>({
      method: 'POST',
      url: '/api/app/playbook/evaluate',
      body: input,
    }, {
      apiName: this.apiName,
    });
  }
}