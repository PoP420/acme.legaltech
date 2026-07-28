import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PermissionDirective, ListService, PagedResultDto } from '@abp/ng.core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { ReviewsService, ReviewCaseDto } from '../services/reviews.service';

@Component({
  selector: 'app-reviews-list',
  template: `
    <div class="card">
      <div class="card-header d-flex justify-content-between align-items-center">
        <h3>Review Cases</h3>
        <a class="btn btn-primary" [routerLink]="['/reviews/create']" *abpPermission="'LegalTech.Reviews.Default'">New Review</a>
      </div>
      <div class="card-body">
        <table class="table">
          <thead>
            <tr>
              <th>Title</th>
              <th>Contract</th>
              <th>Status</th>
              <th>Assigned To</th>
              <th>Priority</th>
              <th>Due Date</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let item of (reviews$ | async)?.items">
              <td><a [routerLink]="['/reviews', item.id]">{{ item.title }}</a></td>
              <td>{{ item.contractTitle }}</td>
              <td>{{ item.status }}</td>
              <td>{{ item.assignedUserName || '-' }}</td>
              <td>{{ item.priority }}</td>
              <td>{{ item.dueDate | date:'shortDate' || '-' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `,
  providers: [ListService],
  imports: [CommonModule, RouterLink, PermissionDirective]
})
export class ReviewsListComponent {
  reviews$: Observable<PagedResultDto<ReviewCaseDto>> = this.list.hookToQuery((query) =>
    this.reviewsService.getList(query),
  );

  constructor(
    public readonly list: ListService,
    private reviewsService: ReviewsService,
  ) {}
}