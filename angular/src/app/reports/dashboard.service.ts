import { Injectable } from '@angular/core';
import { RestService, PagedResultDto } from '@abp/ng.core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ContractService, ContractDto, ContractStatus, ContractStatusLabels } from '../services/contract.service';
import { ObligationsService, ContractObligationDto } from '../services/obligations.service';
import { ReviewsService, ReviewCaseDto } from '../services/reviews.service';

export interface ContractKPIs {
  total: number;
  active: number;
  draft: number;
  expired: number;
  terminated: number;
}

export interface ObligationKPIs {
  total: number;
  pending: number;
  overdue: number;
  completed: number;
}

export interface ReviewKPIs {
  total: number;
  pending: number;
  escalated: number;
  completed: number;
}

export interface ExpiringContract {
  id: string;
  title: string;
  expirationDate: string;
  status: ContractStatus;
  daysUntilExpiry: number;
}

export interface OverdueObligation {
  id: string;
  title: string;
  contractTitle: string;
  dueDate: string;
  status: string;
  priority: number;
  daysOverdue: number;
}

export interface ComplianceRiskItem {
  id: string;
  title: string;
  contractNumber?: string;
  riskType: 'missing-signatory' | 'missing-classification';
  severity: 'high' | 'medium' | 'low';
}

export interface RiskHeatmapBucket {
  status: string;
  riskBaseline: string;
  count: number;
}

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  private readonly apiName = 'default';

  constructor(
    private restService: RestService,
    private contractService: ContractService,
    private obligationsService: ObligationsService,
    private reviewsService: ReviewsService,
  ) {}

  getContractKPIs(): Observable<ContractKPIs> {
    return this.contractService.getList({ maxResultCount: 0 }).pipe(
      map((result: PagedResultDto<ContractDto>) => {
        const items = result.items || [];
        return {
          total: result.totalCount,
          active: items.filter(c => c.status === 1).length,
          draft: items.filter(c => c.status === 0).length,
          expired: items.filter(c => c.status === 2).length,
          terminated: items.filter(c => c.status === 3).length,
        };
      }),
    );
  }

  getObligationKPIs(): Observable<ObligationKPIs> {
    return this.obligationsService.getList({ maxResultCount: 0 }).pipe(
      map((result: PagedResultDto<ContractObligationDto>) => {
        const items = result.items || [];
        return {
          total: result.totalCount,
          pending: items.filter(o => o.status === 'Pending').length,
          overdue: items.filter(o => o.status === 'Overdue').length,
          completed: items.filter(o => o.status === 'Completed').length,
        };
      }),
    );
  }

  getReviewKPIs(): Observable<ReviewKPIs> {
    return this.reviewsService.getList({ maxResultCount: 0 }).pipe(
      map((result: PagedResultDto<ReviewCaseDto>) => {
        const items = result.items || [];
        return {
          total: result.totalCount,
          pending: items.filter(r => r.status === 'Pending' || r.status === 'InProgress').length,
          escalated: items.filter(r => r.status === 'Escalated').length,
          completed: items.filter(r => r.status === 'Completed').length,
        };
      }),
    );
  }

  getExpiringContracts(days: number): Observable<ExpiringContract[]> {
    const today = new Date();
    const cutoff = new Date(today);
    cutoff.setDate(cutoff.getDate() + days);

    return this.contractService.getList({ maxResultCount: 100 }).pipe(
      map((result: PagedResultDto<ContractDto>) => {
        const items = result.items || [];
        return items
          .filter(c => {
            if (!c.expirationDate) return false;
            const expDate = new Date(c.expirationDate);
            return expDate >= today && expDate <= cutoff;
          })
          .map(c => ({
            id: c.id,
            title: c.title,
            expirationDate: c.expirationDate || '',
            status: (c.status || 0) as ContractStatus,
            daysUntilExpiry: Math.ceil(
              (new Date(c.expirationDate || '').getTime() - today.getTime()) /
                (1000 * 60 * 60 * 24),
            ),
          }));
      }),
    );
  }

  getOverdueObligations(): Observable<OverdueObligation[]> {
    const today = new Date();
    const todayStr = today.toISOString().split('T')[0];

    return this.obligationsService.getList({ maxResultCount: 100, status: 'Overdue' }).pipe(
      map((result: PagedResultDto<ContractObligationDto>) => {
        const items = result.items || [];
        return items.map(o => ({
          id: o.id,
          title: o.title,
          contractTitle: o.contractTitle || o.contractId,
          dueDate: o.dueDate || '',
          status: o.status,
          priority: o.priority,
          daysOverdue: o.dueDate
            ? Math.ceil(
                (today.getTime() - new Date(o.dueDate).getTime()) /
                  (1000 * 60 * 60 * 24),
              )
            : 0,
        }));
      }),
    );
  }

  getComplianceRisks(): Observable<ComplianceRiskItem[]> {
    return this.contractService.getList({ maxResultCount: 100 }).pipe(
      map((result: PagedResultDto<ContractDto>) => {
        const items = result.items || [];
        const risks: ComplianceRiskItem[] = [];

        for (const c of items) {
          if (!c.signatories || c.signatories.length === 0) {
            risks.push({
              id: c.id,
              title: c.title,
              contractNumber: c.documentNumber || undefined,
              riskType: 'missing-signatory',
              severity: 'high',
            });
          }
          if (c.classification === undefined || c.classification === null) {
            risks.push({
              id: c.id,
              title: c.title,
              contractNumber: c.documentNumber || undefined,
              riskType: 'missing-classification',
              severity: 'medium',
            });
          }
        }

        return risks;
      }),
    );
  }

  getRiskHeatmap(): Observable<RiskHeatmapBucket[]> {
    return this.contractService.getList({ maxResultCount: 100 }).pipe(
      map((result: PagedResultDto<ContractDto>) => {
        const items = result.items || [];
        const bucketMap = new Map<string, number>();

        for (const c of items) {
          const statusLabel = ContractStatusLabels[c.status || 0] || 'Unknown';
          const riskBaseline = c.riskBaseline || 'None';
          const key = `${statusLabel}|${riskBaseline}`;
          bucketMap.set(key, (bucketMap.get(key) || 0) + 1);
        }

        const buckets: RiskHeatmapBucket[] = [];
        bucketMap.forEach((count, key) => {
          const [status, riskBaseline] = key.split('|');
          buckets.push({ status, riskBaseline, count });
        });

        return buckets;
      }),
    );
  }
}

