import { Component } from '@angular/core';
import { RouterLink, ActivatedRoute, ParamMap } from '@angular/router';
import { switchMap, EMPTY } from 'rxjs';
import { PermissionDirective } from '@abp/ng.core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ObligationsService, ContractObligationDto, ObligationStatusLabels, ObligationStatusBadgeClass, RecurrencePatternLabels } from '../services/obligations.service';

@Component({
  selector: 'app-obligation-detail',
  template: `
    <div class="container mt-3">
      <a class="btn btn-secondary mb-3" [routerLink]="['/obligations/list']">&larr; Back to List</a>

      <div class="card mb-4" *ngIf="obligation; else loading">
        <div class="card-header d-flex justify-content-between align-items-center">
          <h3>{{ obligation.title }}</h3>
          <span class="badge" [ngClass]="statusBadge(obligation.status)">{{ obligationStatusLabel(obligation.status) }}</span>
        </div>
        <div class="card-body">
          <dl class="row">
            <dt class="col-sm-3">Contract</dt>
            <dd class="col-sm-9">{{ obligation.contractTitle || obligation.contractId }}</dd>
            <dt class="col-sm-3">Description</dt>
            <dd class="col-sm-9">{{ obligation.description || '-' }}</dd>
            <dt class="col-sm-3">Due Date</dt>
            <dd class="col-sm-9">{{ obligation.dueDate || '-' }}</dd>
            <dt class="col-sm-3">Completed At</dt>
            <dd class="col-sm-9">{{ obligation.completedAt || '-' }}</dd>
            <dt class="col-sm-3">Priority</dt>
            <dd class="col-sm-9">{{ obligation.priority }}</dd>
            <dt class="col-sm-3">Source Clause</dt>
            <dd class="col-sm-9">{{ obligation.sourceClauseReference || '-' }}</dd>
            <dt class="col-sm-3">Recurrence</dt>
            <dd class="col-sm-9">{{ recurrenceLabel }}</dd>
          </dl>

          <div class="mt-3" *ngIf="obligation.status !== 'Completed' && obligation.status !== 'Deferred'">
            <button class="btn btn-outline-success me-2" *abpPermission="'LegalTech.Obligations.Complete'" (click)="onComplete()" [disabled]="completing">Mark Complete</button>
            <button class="btn btn-outline-warning" *abpPermission="'LegalTech.Obligations.Manage'" (click)="onDefer()" [disabled]="deferring">Defer</button>
          </div>
        </div>
      </div>

      <ng-template #loading>
        <p class="text-muted">Loading obligation...</p>
      </ng-template>

      <div class="card mb-4">
        <div class="card-header d-flex justify-content-between align-items-center">
          <h4>Evidence</h4>
          <span class="badge bg-secondary">{{ obligation?.evidenceCount || 0 }} files</span>
        </div>
        <div class="card-body">
          <div class="alert alert-warning" *ngIf="obligation">Evidence upload/download requires a new backend endpoint.</div>
          <div class="row g-2">
            <div class="col-md-3" *ngFor="let i of [].constructor(obligation?.evidenceCount || 0)">
              <div class="border rounded p-2 text-center text-muted">Evidence {{ i + 1 }}</div>
            </div>
          </div>
          <div *ngIf="obligation && (obligation.evidenceCount === 0)" class="text-muted">
            No evidence uploaded yet.
          </div>
        </div>
      </div>
    </div>
  `,
  imports: [CommonModule, RouterLink, PermissionDirective, FormsModule],
})
export class ObligationDetailComponent {
  obligation: ContractObligationDto | null = null;
  completing = false;
  deferring = false;

  constructor(
    private route: ActivatedRoute,
    private obligationsService: ObligationsService,
  ) {
    this.route.paramMap.pipe(
      switchMap((params: ParamMap) => {
        const id = params.get('id');
        if (!id) return EMPTY;
        return this.obligationsService.get(id);
      }),
    ).subscribe((obligation: ContractObligationDto) => {
      this.obligation = obligation;
    });
  }

  get recurrenceLabel(): string {
    if (!this.obligation?.isRecurring || !this.obligation.recurrencePattern) {
      return 'One-time';
    }
    return RecurrencePatternLabels[this.obligation.recurrencePattern] || this.obligation.recurrencePattern;
  }

  obligationStatusLabel(status: string): string {
    return ObligationStatusLabels[status] || status || '-';
  }

  statusBadge(status: string): string {
    return ObligationStatusBadgeClass[status] || 'bg-secondary';
  }

  onComplete(): void {
    if (!this.obligation) return;
    if (!confirm('Mark this obligation as complete?')) return;
    this.completing = true;
    this.obligationsService.complete(this.obligation.id).subscribe({
      next: (updated) => {
        this.obligation = updated;
        this.completing = false;
      },
      error: () => {
        this.completing = false;
        alert('Failed to complete obligation.');
      },
    });
  }

  onDefer(): void {
    if (!this.obligation) return;
    if (!confirm('Defer this obligation?')) return;
    this.deferring = true;
    this.obligationsService.defer(this.obligation.id).subscribe({
      next: (updated) => {
        this.obligation = updated;
        this.deferring = false;
      },
      error: () => {
        this.deferring = false;
        alert('Failed to defer obligation.');
      },
    });
  }
}
