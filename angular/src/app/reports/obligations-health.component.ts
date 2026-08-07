import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ObligationsService, ContractObligationDto, ObligationStatusBadgeClass } from '../services/obligations.service';

@Component({
  selector: 'app-obligations-health',
  template: `
    <div class="container mt-3">
      <h2>Obligations Health</h2>

      <div class="card mb-3">
        <div class="card-header d-flex justify-content-between align-items-center">
          <h5>Obligations Due Within 14 Days</h5>
          <div class="d-flex gap-2">
            <select class="form-select form-select-sm" style="width: auto;" [(ngModel)]="statusFilter" (change)="onFilterChange()">
              <option value="">All Statuses</option>
              <option value="Pending">Pending</option>
              <option value="Overdue">Overdue</option>
              <option value="Completed">Completed</option>
              <option value="InProgress">In Progress</option>
              <option value="Deferred">Deferred</option>
            </select>
          </div>
        </div>
        <div class="card-body">
          <table class="table table-striped table-sm">
            <thead>
              <tr>
                <th>Title</th>
                <th>Contract</th>
                <th>Status</th>
                <th>Due Date</th>
                <th>Priority</th>
                <th>Recurrence</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let o of filteredObligations">
                <td>{{ o.title }}</td>
                <td>{{ o.contractTitle || o.contractId }}</td>
                <td><span class="badge" [ngClass]="ObligationStatusBadgeClass[o.status] || 'bg-secondary'">{{ o.status }}</span></td>
                <td [class.text-danger]="isOverdue(o.dueDate)">{{ o.dueDate | date:'shortDate' || '-' }}</td>
                <td>{{ o.priority }}</td>
                <td>{{ o.recurrencePattern || 'None' }}</td>
              </tr>
              <tr *ngIf="filteredObligations.length === 0">
                <td colspan="6" class="text-muted">No obligations match the current filter.</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  `,
  imports: [CommonModule, FormsModule],
})
export class ObligationsHealthComponent implements OnInit {
  obligations: ContractObligationDto[] = [];
  filteredObligations: ContractObligationDto[] = [];
  statusFilter = '';

  ObligationStatusBadgeClass = ObligationStatusBadgeClass;

  constructor(private obligationsService: ObligationsService) {}

  ngOnInit(): void {
    this.loadObligations();
  }

  onFilterChange(): void {
    this.applyFilter();
  }

  private loadObligations(): void {
    this.obligationsService.getList({ maxResultCount: 100 }).subscribe((result) => {
      this.obligations = result.items || [];
      this.applyFilter();
    });
  }

  private applyFilter(): void {
    if (!this.statusFilter) {
      this.filteredObligations = this.obligations;
    } else {
      this.filteredObligations = this.obligations.filter(o => o.status === this.statusFilter);
    }
  }

  isOverdue(dueDate?: string | null): boolean {
    if (!dueDate) return false;
    return new Date(dueDate) < new Date(new Date().toISOString().split('T')[0]);
  }
}