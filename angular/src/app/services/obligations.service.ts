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
  status: string;
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