import { Component } from '@angular/core';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { switchMap, EMPTY } from 'rxjs';
import { ReviewsService, ReviewCaseDto, ReviewCaseCreateDto, ReviewCaseUpdateDto } from '../services/reviews.service';

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
          <div class="invalid-feedback d-block" *ngIf="form.get('contractId')?.hasError('pattern')">
            Must be a valid GUID.
          </div>
        </div>
        <div class="mb-3">
          <label class="form-label">Assigned User ID</label>
          <input class="form-control" formControlName="assignedUserId" type="text" />
        </div>
        <div class="mb-3">
          <label class="form-label">Priority</label>
          <input class="form-control" formControlName="priority" type="number" min="0" max="5" />
        </div>
        <div class="mb-3">
          <label class="form-label">Due Date</label>
          <input class="form-control" formControlName="dueDate" type="date" />
          <div class="invalid-feedback d-block" *ngIf="form.get('dueDate')?.hasError('futureDate')">
            Due date must be in the future.
          </div>
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
  editingId: string | null = null;

  guidPattern = /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private reviewsService: ReviewsService,
  ) {
    this.form = this.fb.group({
      title: ['', Validators.required],
      contractId: ['', [Validators.required, Validators.pattern(this.guidPattern)]],
      assignedUserId: [''],
      priority: [0, [Validators.required, Validators.min(0), Validators.max(5)]],
      dueDate: ['', this.futureDateValidator],
      summary: [''],
    });

    this.loadReviewIfEdit();
  }

  futureDateValidator(control: any) {
    if (!control.value) return null;
    const inputDate = new Date(control.value);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return inputDate >= today ? null : { futureDate: true };
  }

  private loadReviewIfEdit() {
    this.route.paramMap.pipe(
      switchMap((params: ParamMap) => {
        const id = params.get('id');
        if (id) {
          this.isEdit = true;
          this.editingId = id;
          return this.reviewsService.get(id);
        }
        this.isEdit = false;
        this.editingId = null;
        this.loadContractFromQuery();
        return EMPTY;
      }),
    ).subscribe((review: ReviewCaseDto) => {
      if (review) {
        this.patchForm(review);
      }
    });
  }

  private loadContractFromQuery() {
    const contractId = this.route.snapshot.queryParamMap.get('contractId');
    if (contractId) {
      this.form.patchValue({ contractId });
    }
  }

  private patchForm(review: ReviewCaseDto) {
    this.form.patchValue({
      id: review.id,
      title: review.title,
      contractId: review.contractId,
      assignedUserId: review.assignedUserId || '',
      priority: review.priority,
      dueDate: review.dueDate || '',
      summary: review.summary || '',
    });
  }

  onSubmit() {
    if (this.form.invalid) return;
    this.saving = true;
    const value = this.form.value;
    if (this.isEdit && this.editingId) {
      const updateInput: ReviewCaseUpdateDto = {
        title: value.title,
        assignedUserId: value.assignedUserId || undefined,
        priority: value.priority,
        dueDate: value.dueDate || undefined,
        summary: value.summary || undefined,
      };
      this.reviewsService.update(this.editingId, updateInput).subscribe({
        next: () => { this.saving = false; this.router.navigate(['/reviews/list']); },
        error: () => { this.saving = false; },
      });
    } else {
      const createInput: ReviewCaseCreateDto = {
        title: value.title,
        contractId: value.contractId,
        assignedUserId: value.assignedUserId || undefined,
        priority: value.priority,
        dueDate: value.dueDate || undefined,
        summary: value.summary || undefined,
      };
      this.reviewsService.create(createInput).subscribe({
        next: () => { this.saving = false; this.navigateAfterCreate(createInput.contractId); },
        error: () => { this.saving = false; },
      });
    }
  }

  private navigateAfterCreate(contractId?: string) {
    if (contractId) {
      this.router.navigate(['/contracts', contractId]);
    } else {
      this.router.navigate(['/reviews/list']);
    }
  }

  onCancel() {
    this.router.navigate(['/reviews/list']);
  }
}
