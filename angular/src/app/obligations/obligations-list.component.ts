import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PermissionDirective, ListService, PagedResultDto } from '@abp/ng.core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { ObligationsService, ContractObligationDto } from '../services/obligations.service';

@Component({
  selector: 'app-obligations-list',
  template: `
    <div class="card">
      <div class="card-header d-flex justify-content-between align-items-center">
        <h3>Obligations</h3>
        <a class="btn btn-primary" [routerLink]="['/obligations/create']" *abpPermission="'LegalTech.Obligations.Manage'">New Obligation</a>
      </div>
      <div class="card-body">
        <table class="table">
          <thead>
            <tr>
              <th>Title</th>
              <th>Contract</th>
              <th>Status</th>
              <th>Due Date</th>
              <th>Priority</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let item of (obligations$ | async)?.items">
              <td>{{ item.title }}</td>
              <td>{{ item.contractTitle }}</td>
              <td>{{ item.status }}</td>
              <td>{{ item.dueDate | date:'shortDate' || '-' }}</td>
              <td>{{ item.priority }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `,
  providers: [ListService],
  imports: [CommonModule, RouterLink, PermissionDirective]
})
export class ObligationsListComponent {
  obligations$: Observable<PagedResultDto<ContractObligationDto>> = this.list.hookToQuery((query) =>
    this.obligationsService.getList(query),
  );

  constructor(
    public readonly list: ListService,
    private obligationsService: ObligationsService,
  ) {}
}