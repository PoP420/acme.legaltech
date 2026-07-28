import { Injectable } from '@angular/core';
import { RestService, PagedResultDto } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface ReviewCaseDto {
  id: string;
  tenantId?: string;
  title: string;
  contractId: string;
  contractTitle?: string;
  status: string;
  assignedUserId?: string;
  assignedUserName?: string;
  dueDate?: string | null;
  summary?: string | null;
  priority: number;
  taskCount: number;
  completedTaskCount: number;
  escalationCount: number;
}

export interface ReviewCaseCreateDto {
  title: string;
  contractId: string;
  assignedUserId?: string;
  priority?: number;
  summary?: string;
  dueDate?: string;
}

export interface ReviewCaseUpdateDto {
  title: string;
  assignedUserId?: string;
  priority?: number;
  summary?: string;
  dueDate?: string;
}

export interface ReviewCaseGetListInput {
  maxResultCount?: number;
  skipCount?: number;
  sorting?: string;
  filter?: string;
  status?: string;
  contractId?: string;
  assignedUserId?: string;
  priority?: number;
}

export interface ReviewTaskDto {
  id: string;
  reviewCaseId: string;
  reviewCaseTitle?: string;
  title: string;
  status: string;
  assignedUserId?: string;
  assignedUserName?: string;
  dueDate?: string | null;
  sortOrder: number;
}

export interface ApprovalStepDto {
  id: string;
  reviewCaseId: string;
  reviewCaseTitle?: string;
  name: string;
  stepOrder: number;
  status: string;
  approverUserId?: string;
  approverUserName?: string;
  completedAt?: string | null;
  comments?: string | null;
  isRequired: boolean;
}

export interface ReviewCommentDto {
  id: string;
  reviewCaseId: string;
  reviewCaseTitle?: string;
  authorUserId?: string;
  authorUserName?: string;
  content: string;
  creationTime: string;
}

export interface EscalationEventDto {
  id: string;
  reviewCaseId: string;
  reviewCaseTitle?: string;
  reason: string;
  severity: string;
  escalatedByUserId?: string;
  escalatedByUserName?: string;
  escalatedAt: string;
  resolvedAt?: string | null;
  resolution?: string | null;
  resolvedByUserId?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ReviewsService {
  private readonly apiName = 'default';

  constructor(private restService: RestService) {}

  getList(input: ReviewCaseGetListInput = {}): Observable<PagedResultDto<ReviewCaseDto>> {
    return this.restService.request<null, PagedResultDto<ReviewCaseDto>>({
      method: 'GET',
      url: '/api/app/review',
      params: {
        maxResultCount: input.maxResultCount ?? 10,
        skipCount: input.skipCount ?? 0,
        ...(input.sorting ? { sorting: input.sorting } : {}),
        ...(input.filter ? { filter: input.filter } : {}),
        ...(input.status ? { status: input.status } : {}),
        ...(input.contractId ? { contractId: input.contractId } : {}),
        ...(input.assignedUserId ? { assignedUserId: input.assignedUserId } : {}),
        ...(input.priority !== undefined ? { priority: input.priority } : {}),
      },
    }, {
      apiName: this.apiName,
    });
  }

  create(input: ReviewCaseCreateDto): Observable<ReviewCaseDto> {
    return this.restService.request<ReviewCaseCreateDto, ReviewCaseDto>({
      method: 'POST',
      url: '/api/app/review',
      body: input,
    }, {
      apiName: this.apiName,
    });
  }

  update(id: string, input: ReviewCaseUpdateDto): Observable<ReviewCaseDto> {
    return this.restService.request<ReviewCaseUpdateDto, ReviewCaseDto>({
      method: 'PUT',
      url: `/api/app/review/${id}`,
      body: input,
    }, {
      apiName: this.apiName,
    });
  }

  delete(id: string): Observable<void> {
    return this.restService.request<void, void>({
      method: 'DELETE',
      url: `/api/app/review/${id}`,
    }, {
      apiName: this.apiName,
    });
  }

  assign(id: string, userId: string): Observable<void> {
    return this.restService.request<void, void>({
      method: 'PUT',
      url: `/api/app/review/${id}/assign`,
      body: { userId },
    }, {
      apiName: this.apiName,
    });
  }

  escalate(id: string, reason: string, severity: string): Observable<void> {
    return this.restService.request<any, void>({
      method: 'PUT',
      url: `/api/app/review/${id}/escalate`,
      body: { reason, severity },
    }, {
      apiName: this.apiName,
    });
  }

  complete(id: string): Observable<void> {
    return this.restService.request<void, void>({
      method: 'PUT',
      url: `/api/app/review/${id}/complete`,
    }, {
      apiName: this.apiName,
    });
  }
}