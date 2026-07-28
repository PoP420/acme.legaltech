import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PermissionDirective, ListService, PagedResultDto } from '@abp/ng.core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { PlaybooksService, PlaybookProfileDto } from '../services/playbooks.service';

@Component({
  selector: 'app-playbooks-list',
  template: `
    <div class="card">
      <div class="card-header d-flex justify-content-between align-items-center">
        <h3>Playbooks</h3>
        <a class="btn btn-primary" [routerLink]="['/playbooks/create']" *abpPermission="'LegalTech.Playbooks.Manage'">New Playbook</a>
      </div>
      <div class="card-body">
        <table class="table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Description</th>
              <th>Rules Count</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let item of (playbooks$ | async)?.items">
              <td><a [routerLink]="['/playbooks', item.id]">{{ item.name }}</a></td>
              <td>{{ item.description || '-' }}</td>
              <td>{{ item.rules?.length ?? 0 }}</td>
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
export class PlaybooksListComponent {
  playbooks$: Observable<PagedResultDto<PlaybookProfileDto>> = this.list.hookToQuery((query) =>
    this.playbooksService.getList(query),
  );

  constructor(
    public readonly list: ListService,
    private playbooksService: PlaybooksService,
  ) {}
}