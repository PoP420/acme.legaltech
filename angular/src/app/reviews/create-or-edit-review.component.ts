import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ReviewsService } from '../services/reviews.service';

@Component({
  selector: 'app-create-or-edit-review',
  template: `
    <div class="container mt-3">
      <h2>{{ isEdit ? 'Edit Review Case' : 'Create Review Case' }}</h2>
      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <div class="mb-3">
          <label class="form-label">Title</label>
          <input class="form-control" formControlName="title" />
        </div>
        <div class="mb-3">
          <label class="form-label">Contract ID</label>
          <input class="form-control" formControlName="contractId" type="text" />
        </div>
        <div class="mb-3">
          <label class="form-label">Assigned User ID</label>
          <input class="form-control" formControlName="assignedUserId" type="text" />
        </div>
        <div class="mb-3">
          <label class="form-label">Priority</label>
          <input class="form-control" formControlName="priority" type="number" />
        </div>
        <div class="mb-3">
          <label class="form-label">Due Date</label>
          <input class="form-control" formControlName="dueDate" type="date" />
        </div>
        <div class="mb-3">
          <label class="form-label">Summary</label>
          <textarea class="form-control" formControlName="summary" rows="3"></textarea>
        </div>
        <button type="submit" class="btn btn-primary" [disabled]="saving">{{ isEdit ? 'Update' : 'Create' }}</button>
        <button type="button" class="btn btn-secondary ms-2" (click)="onCancel()">Cancel</button>
      </form>
    </div>
  `,
  imports: [ReactiveFormsModule],
})
export class CreateOrEditReviewComponent {
  form: FormGroup;
  saving = false;
  isEdit = false;

  constructor(private fb: FormBuilder, private router: Router, private reviewsService: ReviewsService) {
    this.form = this.fb.group({
      title: ['', Validators.required],
      contractId: ['', Validators.required],
      assignedUserId: [''],
      priority: [0],
      dueDate: [''],
      summary: [''],
    });
  }

  onSubmit() {
    if (this.form.invalid) return;
    this.saving = true;
    const value = this.form.value;
    if (this.isEdit) {
      this.reviewsService.update(value.id, value).subscribe({
        next: () => { this.router.navigate(['/reviews']); },
        error: () => { this.saving = false; },
      });
    } else {
      this.reviewsService.create(value).subscribe({
        next: () => { this.router.navigate(['/reviews']); },
        error: () => { this.saving = false; },
      });
    }
  }

  onCancel() {
    this.router.navigate(['/reviews']);
  }
}