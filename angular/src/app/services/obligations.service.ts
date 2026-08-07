import { Injectable } from '@angular/core';
import { RestService, PagedResultDto } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface ContractObligationDto {
  id: string;
  tenantId?: string;
  contractId: string;
  contractTitle?: string;
  title: string;
  description: string;
  status: ObligationStatus;
  dueDate?: string | null;
  completedAt?: string | null;
  sourceClauseReference?: string | null;
  isRecurring: boolean;
  recurrencePattern?: string | null;
  priority: number;
  evidenceCount: number;
}

export interface ContractObligationCreateDto {
  contractId: string;
  title: string;
  description: string;
  dueDate?: string;
  sourceClauseReference?: string;
  isRecurring?: boolean;
  recurrencePattern?: string;
  priority?: number;
}

export interface ContractObligationUpdateDto {
  title: string;
  description: string;
  dueDate?: string;
  priority?: number;
}

export interface ContractObligationGetListInput {
  maxResultCount?: number;
  skipCount?: number;
  sorting?: string;
  filter?: string;
  status?: string;
  contractId?: string;
  dueDateFrom?: string;
  dueDateTo?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ObligationsService {
  private readonly apiName = 'default';

  constructor(private restService: RestService) {}

  getList(input: ContractObligationGetListInput = {}): Observable<PagedResultDto<ContractObligationDto>> {
    return this.restService.request<null, PagedResultDto<ContractObligationDto>>({
      method: 'GET',
      url: '/api/app/contract-obligation',
      params: {
        maxResultCount: input.maxResultCount ?? 10,
        skipCount: input.skipCount ?? 0,
        ...(input.sorting ? { sorting: input.sorting } : {}),
        ...(input.filter ? { filter: input.filter } : {}),
        ...(input.status ? { status: input.status } : {}),
        ...(input.contractId ? { contractId: input.contractId } : {}),
        ...(input.dueDateFrom ? { dueDateFrom: input.dueDateFrom } : {}),
        ...(input.dueDateTo ? { dueDateTo: input.dueDateTo } : {}),
      },
    }, {
      apiName: this.apiName,
    });
  }

  get(id: string): Observable<ContractObligationDto> {
    return this.restService.request<null, ContractObligationDto>({
      method: 'GET',
      url: `/api/app/contract-obligation/${id}`,
    }, {
      apiName: this.apiName,
    });
  }

  create(input: ContractObligationCreateDto): Observable<ContractObligationDto> {
    return this.restService.request<ContractObligationCreateDto, ContractObligationDto>({
      method: 'POST',
      url: '/api/app/contract-obligation',
      body: input,
    }, {
      apiName: this.apiName,
    });
  }

  update(id: string, input: ContractObligationUpdateDto): Observable<ContractObligationDto> {
    return this.restService.request<ContractObligationUpdateDto, ContractObligationDto>({
      method: 'PUT',
      url: `/api/app/contract-obligation/${id}`,
      body: input,
    }, {
      apiName: this.apiName,
    });
  }

  delete(id: string): Observable<void> {
    return this.restService.request<void, void>({
      method: 'DELETE',
      url: `/api/app/contract-obligation/${id}`,
    }, {
      apiName: this.apiName,
    });
  }

  complete(id: string): Observable<ContractObligationDto> {
    return this.restService.request<null, ContractObligationDto>({
      method: 'PUT',
      url: `/api/app/contract-obligation/${id}/complete`,
    }, {
      apiName: this.apiName,
    });
  }

  defer(id: string): Observable<ContractObligationDto> {
    return this.restService.request<null, ContractObligationDto>({
      method: 'PUT',
      url: `/api/app/contract-obligation/${id}/defer`,
    }, {
      apiName: this.apiName,
    });
  }
}

export type ObligationStatus = 'Pending' | 'InProgress' | 'Completed' | 'Deferred' | 'Overdue';

export const ObligationStatusLabels: Record<string, string> = {
  Pending: 'Pending',
  InProgress: 'In Progress',
  Completed: 'Completed',
  Deferred: 'Deferred',
  Overdue: 'Overdue',
};

export const ObligationStatusBadgeClass: Record<string, string> = {
  Pending: 'bg-secondary',
  InProgress: 'bg-info',
  Completed: 'bg-success',
  Deferred: 'bg-warning',
  Overdue: 'bg-danger',
};

export type RecurrencePattern = 'None' | 'Daily' | 'Weekly' | 'Monthly' | 'Quarterly' | 'Annually';

export const RecurrencePatternLabels: Record<string, string> = {
  None: 'One-time',
  Daily: 'Daily',
  Weekly: 'Weekly',
  Monthly: 'Monthly',
  Quarterly: 'Quarterly',
  Annually: 'Annually',
};