import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PermissionDirective, ListService, PagedResultDto } from '@abp/ng.core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { ContractService, ContractDto, ContractStatus, ContractStatusLabels, GetContractsInput } from '../services/contract.service';

@Component({
  selector: 'app-contracts-list',
  template: `
    <div class="card">
      <div class="card-header d-flex justify-content-between align-items-center">
        <h3>Contracts</h3>
        <a class="btn btn-primary" [routerLink]="['/contracts/create']" *abpPermission="'LegalTech.Contracts.Create'">New Contract</a>
      </div>
      <div class="card-body">
        <div class="row g-2 mb-3">
          <div class="col-md-4">
            <input class="form-control" placeholder="Search..." [(ngModel)]="filterText" (ngModelChange)="onFilterTextChange()" />
          </div>
          <div class="col-md-3">
            <select class="form-select" [(ngModel)]="statusFilter" (ngModelChange)="onStatusChange()">
              <option [ngValue]="undefined">All Statuses</option>
              <option [ngValue]="0">Draft</option>
              <option [ngValue]="1">Active</option>
              <option [ngValue]="2">Expired</option>
              <option [ngValue]="3">Terminated</option>
            </select>
          </div>
          <div class="col-md-3">
            <input class="form-control" placeholder="Category..." [(ngModel)]="categoryFilter" (ngModelChange)="onCategoryChange()" />
          </div>
        </div>
        <table class="table">
          <thead>
            <tr>
              <th>Title</th>
              <th>Counterparty</th>
              <th>Category</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let item of (contracts$ | async)?.items">
              <td><a [routerLink]="['/contracts', item.id]">{{ item.title }}</a></td>
              <td>{{ item.counterpartyName }}</td>
              <td>{{ item.category || '-' }}</td>
              <td>{{ statusLabel(item.status) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `,
  providers: [ListService],
  imports: [CommonModule, RouterLink, PermissionDirective, FormsModule]
})
export class ContractsListComponent {
  filterText = '';
  statusFilter: ContractStatus | undefined;
  categoryFilter = '';

  contracts$: Observable<PagedResultDto<ContractDto>> = this.list.hookToQuery((query) =>
    this.contractService.getList({
      ...query,
      filter: this.filterText || undefined,
      status: this.statusFilter,
      category: this.categoryFilter || undefined,
    } as GetContractsInput),
  );

  constructor(
    public readonly list: ListService,
    private contractService: ContractService,
  ) {}

  onFilterTextChange(): void {
    this.list.filter = this.filterText;
  }

  onStatusChange(): void {
    this.list.get();
  }

  onCategoryChange(): void {
    this.list.get();
  }

  statusLabel(status: number | undefined): string {
    if (status === undefined || status === null) return '-';
    return ContractStatusLabels[status as ContractStatus] || String(status);
  }
}
