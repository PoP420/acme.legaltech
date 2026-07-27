import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PermissionDirective, ListService, PagedResultDto } from '@abp/ng.core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { ContractService, ContractDto } from '../services/contract.service';

@Component({
  selector: 'app-contracts-list',
  template: `
    <div class="card">
      <div class="card-header d-flex justify-content-between align-items-center">
        <h3>Contracts</h3>
        <a class="btn btn-primary" [routerLink]="['/contracts/create']" *abpPermission="'LegalTech.Contracts.Create'">New Contract</a>
      </div>
      <div class="card-body">
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
              <td>{{ item.status }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `,
  providers: [ListService],
  imports: [CommonModule, RouterLink, PermissionDirective]
})
export class ContractsListComponent {
  contracts$: Observable<PagedResultDto<ContractDto>> = this.list.hookToQuery((query) =>
    this.contractService.getList(query),
  );

  constructor(
    public readonly list: ListService,
    private contractService: ContractService,
  ) {}
}
