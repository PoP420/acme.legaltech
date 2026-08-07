import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { DashboardService, ReviewKPIs } from './dashboard.service';
import { ReviewsService, ReviewCaseDto } from '../services/reviews.service';

@Component({
  selector: 'app-reviews-pipeline',
  template: `
    <div class="container mt-3">
      <h2>Reviews Pipeline</h2>

      <div class="row">
        <div class="col-md-3" *ngFor="let column of pipelineColumns">
          <div class="card mb-3">
            <div class="card-header d-flex justify-content-between align-items-center">
              <h6 class="mb-0">{{ column.label }}</h6>
              <span class="badge" [ngClass]="column.badgeClass">{{ column.count }}</span>
            </div>
            <div class="card-body p-2">
              <div class="list-group list-group-flush">
                <a *ngFor="let review of column.reviews" [routerLink]="['/reviews', review.id]" class="list-group-item list-group-item-action py-2">
                  <div class="d-flex w-100 justify-content-between">
                    <h6 class="mb-1">{{ review.title }}</h6>
                    <small [class.text-danger]="review.priority >= 4">{{ review.priority }}</small>
                  </div>
                  <small class="text-muted">{{ review.contractTitle || review.contractId }}</small>
                  <div class="d-flex justify-content-between mt-1">
                    <small class="text-muted">{{ review.assignedUserName || '-' }}</small>
                    <small class="text-muted">{{ review.dueDate | date:'shortDate' || '-' }}</small>
                  </div>
                </a>
                <div *ngIf="column.reviews.length === 0" class="text-muted p-2 small">No reviews in this stage.</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  imports: [CommonModule, RouterLink],
})
export class ReviewsPipelineComponent implements OnInit {
  pipelineColumns = [
    { label: 'Draft', status: 'Draft', badgeClass: 'bg-secondary', reviews: [] as ReviewCaseDto[] },
    { label: 'In Progress', status: 'InProgress', badgeClass: 'bg-info', reviews: [] as ReviewCaseDto[] },
    { label: 'Completed', status: 'Completed', badgeClass: 'bg-success', reviews: [] as ReviewCaseDto[] },
    { label: 'Escalated', status: 'Escalated', badgeClass: 'bg-danger', reviews: [] as ReviewCaseDto[] },
  ];

  constructor(
    private dashboardService: DashboardService,
    private reviewsService: ReviewsService,
  ) {}

  ngOnInit(): void {
    this.reviewsService.getList({ maxResultCount: 100 }).subscribe((result) => {
      const items = result.items || [];
      for (const column of this.pipelineColumns) {
        column.reviews = items.filter(r => r.status === column.status);
      }
    });
  }
}