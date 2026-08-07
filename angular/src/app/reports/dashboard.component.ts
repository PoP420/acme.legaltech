import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService, ContractKPIs, ObligationKPIs, ReviewKPIs, ExpiringContract, OverdueObligation, ComplianceRiskItem, RiskHeatmapBucket } from './dashboard.service';
import { ContractStatusLabels } from '../services/contract.service';
import { ObligationStatusBadgeClass } from '../services/obligations.service';

@Component({
  selector: 'app-dashboard',
  template: `
    <div class="container mt-3">
      <h2>Portfolio Dashboard</h2>

      <div class="row mb-4">
        <div class="col-md-2">
          <div class="card bg-primary text-white">
            <div class="card-body text-center">
              <h6>Total Contracts</h6>
              <h3>{{ contractKPIs?.total || 0 }}</h3>
            </div>
          </div>
        </div>
        <div class="col-md-2">
          <div class="card bg-success text-white">
            <div class="card-body text-center">
              <h6>Active Contracts</h6>
              <h3>{{ contractKPIs?.active || 0 }}</h3>
            </div>
          </div>
        </div>
        <div class="col-md-2">
          <div class="card bg-danger text-white">
            <div class="card-body text-center">
              <h6>Overdue Obligations</h6>
              <h3>{{ obligationKPIs?.overdue || 0 }}</h3>
            </div>
          </div>
        </div>
        <div class="col-md-2">
          <div class="card bg-warning text-white">
            <div class="card-body text-center">
              <h6>Open Reviews</h6>
              <h3>{{ reviewKPIs?.pending || 0 }}</h3>
            </div>
          </div>
        </div>
        <div class="col-md-2">
          <div class="card bg-dark text-white">
            <div class="card-body text-center">
              <h6>Escalated Reviews</h6>
              <h3>{{ reviewKPIs?.escalated || 0 }}</h3>
            </div>
          </div>
        </div>
        <div class="col-md-2">
          <div class="card bg-info text-white">
            <div class="card-body text-center">
              <h6>Completed Reviews</h6>
              <h3>{{ reviewKPIs?.completed || 0 }}</h3>
            </div>
          </div>
        </div>
      </div>

      <div class="row mb-4">
        <div class="col-md-6">
          <div class="card">
            <div class="card-header"><h5>Contracts Expiring in 30 Days</h5></div>
            <div class="card-body">
              <table class="table table-sm">
                <thead>
                  <tr>
                    <th>Title</th>
                    <th>Expiration Date</th>
                    <th>Days Left</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let c of expiringContracts">
                    <td>{{ c.title }}</td>
                    <td>{{ c.expirationDate | date:'shortDate' }}</td>
                    <td [class.text-danger]="c.daysUntilExpiry <= 7" [class.text-warning]="c.daysUntilExpiry > 7 && c.daysUntilExpiry <= 14">{{ c.daysUntilExpiry }}</td>
                    <td><span class="badge" [ngClass]="getContractStatusBadge(c.status)">{{ getContractStatusLabel(c.status) }}</span></td>
                  </tr>
                  <tr *ngIf="expiringContracts.length === 0">
                    <td colspan="4" class="text-muted">No contracts expiring soon.</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
        <div class="col-md-6">
          <div class="card">
            <div class="card-header"><h5>Overdue Obligations</h5></div>
            <div class="card-body">
              <table class="table table-sm">
                <thead>
                  <tr>
                    <th>Title</th>
                    <th>Contract</th>
                    <th>Due Date</th>
                    <th>Days Overdue</th>
                    <th>Priority</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let o of overdueObligations">
                    <td>{{ o.title }}</td>
                    <td>{{ o.contractTitle }}</td>
                    <td>{{ o.dueDate | date:'shortDate' }}</td>
                    <td [class.text-danger]="o.daysOverdue > 7">{{ o.daysOverdue }}</td>
                    <td>{{ o.priority }}</td>
                  </tr>
                  <tr *ngIf="overdueObligations.length === 0">
                    <td colspan="5" class="text-muted">No overdue obligations.</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>

      <div class="row mb-4">
        <div class="col-md-6">
          <div class="card">
            <div class="card-header"><h5>Compliance Risk</h5></div>
            <div class="card-body">
              <table class="table table-sm">
                <thead>
                  <tr>
                    <th>Contract</th>
                    <th>Risk Type</th>
                    <th>Severity</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let risk of complianceRisks">
                    <td>{{ risk.title }}</td>
                    <td>{{ risk.riskType === 'missing-signatory' ? 'Missing Signatory' : 'Missing Classification' }}</td>
                    <td><span class="badge" [ngClass]="getRiskBadgeClass(risk.severity)">{{ risk.severity }}</span></td>
                  </tr>
                  <tr *ngIf="complianceRisks.length === 0">
                    <td colspan="3" class="text-muted">No compliance risks found.</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
        <div class="col-md-6">
          <div class="card">
            <div class="card-header"><h5>Risk Heatmap</h5></div>
            <div class="card-body">
              <table class="table table-sm">
                <thead>
                  <tr>
                    <th>Status / Risk Baseline</th>
                    <th *ngFor="let baseline of heatmapBaselines">{{ baseline }}</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let status of heatmapStatuses">
                    <td><strong>{{ status }}</strong></td>
                    <td *ngFor="let baseline of heatmapBaselines" [style.background-color]="getHeatmapColor(getHeatmapCount(status, baseline))">
                      {{ getHeatmapCount(status, baseline) || '-' }}
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  imports: [CommonModule],
})
export class DashboardComponent implements OnInit {
  contractKPIs: ContractKPIs | null = null;
  obligationKPIs: ObligationKPIs | null = null;
  reviewKPIs: ReviewKPIs | null = null;
  expiringContracts: ExpiringContract[] = [];
  overdueObligations: OverdueObligation[] = [];
  complianceRisks: ComplianceRiskItem[] = [];
  heatmapBuckets: RiskHeatmapBucket[] = [];

  heatmapStatuses = ['Draft', 'Active', 'Expired', 'Terminated'];
  heatmapBaselines = ['None', 'Low', 'Medium', 'High', 'Critical'];

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.dashboardService.getContractKPIs().subscribe((kpis: ContractKPIs) => {
      this.contractKPIs = kpis;
    });
    this.dashboardService.getObligationKPIs().subscribe((kpis: ObligationKPIs) => {
      this.obligationKPIs = kpis;
    });
    this.dashboardService.getReviewKPIs().subscribe((kpis: ReviewKPIs) => {
      this.reviewKPIs = kpis;
    });
    this.dashboardService.getExpiringContracts(30).subscribe((contracts: ExpiringContract[]) => {
      this.expiringContracts = contracts;
    });
    this.dashboardService.getOverdueObligations().subscribe((obligations: OverdueObligation[]) => {
      this.overdueObligations = obligations;
    });
    this.dashboardService.getComplianceRisks().subscribe((risks: ComplianceRiskItem[]) => {
      this.complianceRisks = risks;
    });
    this.dashboardService.getRiskHeatmap().subscribe((buckets: RiskHeatmapBucket[]) => {
      this.heatmapBuckets = buckets;
    });
  }

  getContractStatusLabel(status: number): string {
    return ContractStatusLabels[status as keyof typeof ContractStatusLabels] || 'Unknown';
  }

  getContractStatusBadge(status: number): string {
    const labels: Record<number, string> = {
      0: 'bg-secondary',
      1: 'bg-success',
      2: 'bg-danger',
      3: 'bg-dark',
    };
    return labels[status] || 'bg-secondary';
  }

  getRiskBadgeClass(severity: string): string {
    const labels: Record<string, string> = {
      high: 'bg-danger',
      medium: 'bg-warning',
      low: 'bg-info',
    };
    return labels[severity] || 'bg-secondary';
  }

  getHeatmapCount(status: string, baseline: string): number {
    const bucket = this.heatmapBuckets.find(b => b.status === status && b.riskBaseline === baseline);
    return bucket?.count || 0;
  }

  getHeatmapColor(count: number): string {
    if (count === 0) return 'transparent';
    const intensity = Math.min(count / 10, 1);
    const red = Math.round(255 * intensity);
    const green = Math.round(255 * (1 - intensity));
    return `rgb(${red}, ${green}, 100)`;
  }
}