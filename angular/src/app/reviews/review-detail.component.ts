import { Component } from '@angular/core';
import { RouterLink, ActivatedRoute, ParamMap } from '@angular/router';
import { switchMap, EMPTY } from 'rxjs';
import { PermissionDirective, ConfigStateService } from '@abp/ng.core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReviewsService, ReviewCaseDto } from '../services/reviews.service';

@Component({
  selector: 'app-review-detail',
  template: `
    <div class="container mt-3">
      <a class="btn btn-secondary mb-3" [routerLink]="['/reviews/list']">&larr; Back to List</a>

      <div class="card mb-4" *ngIf="review">
        <div class="card-header d-flex justify-content-between align-items-center">
          <h3>{{ review.title }}</h3>
          <span class="badge bg-secondary">{{ review.status }}</span>
        </div>
        <div class="card-body">
          <dl class="row">
            <dt class="col-sm-3">Contract</dt>
            <dd class="col-sm-9">{{ review.contractTitle || review.contractId }}</dd>
            <dt class="col-sm-3">Assigned To</dt>
            <dd class="col-sm-9">{{ review.assignedUserName || '-' }}</dd>
            <dt class="col-sm-3">Due Date</dt>
            <dd class="col-sm-9">{{ review.dueDate || '-' }}</dd>
            <dt class="col-sm-3">Priority</dt>
            <dd class="col-sm-9">{{ review.priority }}</dd>
            <dt class="col-sm-3">Summary</dt>
            <dd class="col-sm-9">{{ review.summary || '-' }}</dd>
            <dt class="col-sm-3">Created</dt>
            <dd class="col-sm-9">{{ review.creationTime || '-' }}</dd>
          </dl>

          <div class="mt-3 d-flex gap-2" *abpPermission="'LegalTech.Reviews.Assign'">
            <button class="btn btn-outline-primary" (click)="onAssign()" [disabled]="assigning">Assign to Me</button>
            <button class="btn btn-outline-warning" (click)="onEscalate()" [disabled]="escalating">Escalate</button>
            <button class="btn btn-outline-success" (click)="onComplete()" [disabled]="completing" *abpPermission="'LegalTech.Reviews.Decide'">Mark Complete</button>
          </div>
        </div>
      </div>

      <div class="row mb-4">
        <div class="col-md-4">
          <div class="card">
            <div class="card-header"><h5>Tasks</h5></div>
            <div class="card-body">
              <p class="text-muted">{{ review?.taskCount || 0 }} total, {{ review?.completedTaskCount || 0 }} completed</p>
              <p class="small text-warning">Task details require backend endpoint.</p>
            </div>
          </div>
        </div>
        <div class="col-md-4">
          <div class="card">
            <div class="card-header"><h5>Approval Steps</h5></div>
            <div class="card-body">
              <p class="small text-warning">Approval step details require backend endpoint.</p>
            </div>
          </div>
        </div>
        <div class="col-md-4">
          <div class="card">
            <div class="card-header"><h5>Escalations</h5></div>
            <div class="card-body">
              <p class="text-muted">{{ review?.escalationCount || 0 }} escalations</p>
              <p class="small text-warning">Escalation details require backend endpoint.</p>
            </div>
          </div>
        </div>
      </div>

      <div class="card mb-4">
        <div class="card-header"><h4>Comments</h4></div>
        <div class="card-body">
          <div class="alert alert-warning">Comments require a new backend endpoint.</div>
          <div class="mb-3" *abpPermission="'LegalTech.Reviews.Default'">
            <textarea class="form-control mb-2" [(ngModel)]="newComment" placeholder="Add a comment..."></textarea>
            <button class="btn btn-primary" [disabled]="!newComment || postingComment">Post Comment</button>
          </div>
        </div>
      </div>
    </div>
  `,
  imports: [CommonModule, RouterLink, PermissionDirective, FormsModule],
})
export class ReviewDetailComponent {
  review: ReviewCaseDto | null = null;
  newComment = '';
  assigning = false;
  escalating = false;
  completing = false;
  postingComment = false;

  constructor(
    private route: ActivatedRoute,
    private reviewsService: ReviewsService,
    private configState: ConfigStateService,
  ) {
    this.route.paramMap.pipe(
      switchMap((params: ParamMap) => {
        const id = params.get('id');
        if (!id) return EMPTY;
        return this.reviewsService.get(id);
      }),
    ).subscribe((review: ReviewCaseDto) => {
      this.review = review;
    });
  }

  onAssign(): void {
    if (!this.review) return;
    const currentUserId = this.configState.getOne('currentUser').id;
    if (!currentUserId) {
      alert('No current user available to assign.');
      return;
    }
    this.assigning = true;
    this.reviewsService.assign(this.review.id, currentUserId).subscribe({
      next: () => {
        this.reload();
      },
      error: () => {
        this.assigning = false;
        alert('Failed to assign review.');
      },
    });
  }

  onEscalate(): void {
    if (!this.review) return;
    const reason = prompt('Enter escalation reason:');
    if (!reason) return;
    const severity = prompt('Enter severity (e.g. High, Critical):') || 'High';
    this.escalating = true;
    this.reviewsService.escalate(this.review.id, reason, severity).subscribe({
      next: () => {
        this.reload();
      },
      error: () => {
        this.escalating = false;
        alert('Failed to escalate review.');
      },
    });
  }

  onComplete(): void {
    if (!this.review) return;
    if (!confirm('Mark this review as complete?')) return;
    this.completing = true;
    this.reviewsService.complete(this.review.id).subscribe({
      next: () => {
        this.reload();
      },
      error: () => {
        this.completing = false;
        alert('Failed to complete review.');
      },
    });
  }

  private reload(): void {
    if (!this.review) return;
    this.assigning = false;
    this.escalating = false;
    this.completing = false;
    this.reviewsService.get(this.review.id).subscribe((updated: ReviewCaseDto) => {
      this.review = updated;
    });
  }
}
