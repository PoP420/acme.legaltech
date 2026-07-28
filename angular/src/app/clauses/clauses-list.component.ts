import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PermissionDirective, ListService, PagedResultDto } from '@abp/ng.core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { ClausesService, ClauseTemplateDto } from '../services/clauses.service';

@Component({
  selector: 'app-clauses-list',
  template: `
    <div class="card">
      <div class="card-header d-flex justify-content-between align-items-center">
        <h3>Clauses</h3>
        <a class="btn btn-primary" [routerLink]="['/clauses/create']" *abpPermission="'LegalTech.Clauses.Manage'">New Clause</a>
      </div>
      <div class="card-body">
        <table class="table">
          <thead>
            <tr>
              <th>Title</th>
              <th>Taxonomy</th>
              <th>Jurisdiction</th>
              <th>Category</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let item of (clauses$ | async)?.items">
              <td>{{ item.title }}</td>
              <td>{{ item.taxonomyName || '-' }}</td>
              <td>{{ item.jurisdiction || '-' }}</td>
              <td>{{ item.category || '-' }}</td>
              <td>{{ item.isActive ? 'Active' : 'Inactive' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `,
  providers: [ListService],
  imports: [CommonModule, RouterLink, PermissionDirective]
})
export class ClausesListComponent {
  clauses$: Observable<PagedResultDto<ClauseTemplateDto>> = this.list.hookToQuery((query) =>
    this.clausesService.getList(query),
  );

  constructor(
    public readonly list: ListService,
    private clausesService: ClausesService,
  ) {}
}