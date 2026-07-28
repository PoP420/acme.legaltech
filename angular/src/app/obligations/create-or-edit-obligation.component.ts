import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ObligationsService } from '../services/obligations.service';

@Component({
  selector: 'app-create-or-edit-obligation',
  template: `
    <div class="container mt-3">
      <h2>{{ isEdit ? 'Edit Obligation' : 'Create Obligation' }}</h2>
      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <div class="mb-3">
          <label class="form-label">Title</label>
          <input class="form-control" formControlName="title" />
        </div>
        <div class="mb-3">
          <label class="form-label">Description</label>
          <textarea class="form-control" formControlName="description" rows="3"></textarea>
        </div>
        <div class="mb-3">
          <label class="form-label">Contract ID</label>
          <input class="form-control" formControlName="contractId" type="text" />
        </div>
        <div class="mb-3">
          <label class="form-label">Due Date</label>
          <input class="form-control" formControlName="dueDate" type="date" />
        </div>
        <div class="mb-3">
          <label class="form-label">Priority</label>
          <input class="form-control" formControlName="priority" type="number" />
        </div>
        <button type="submit" class="btn btn-primary" [disabled]="saving">{{ isEdit ? 'Update' : 'Create' }}</button>
        <button type="button" class="btn btn-secondary ms-2" (click)="onCancel()">Cancel</button>
      </form>
    </div>
  `,
  imports: [ReactiveFormsModule],
})
export class CreateOrEditObligationComponent {
  form: FormGroup;
  saving = false;
  isEdit = false;

  constructor(private fb: FormBuilder, private router: Router, private obligationsService: ObligationsService) {
    this.form = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      contractId: ['', Validators.required],
      dueDate: [''],
      priority: [0],
    });
  }

  onSubmit() {
    if (this.form.invalid) return;
    this.saving = true;
    const value = this.form.value;
    if (this.isEdit) {
      this.obligationsService.update(value.id, value).subscribe({
        next: () => { this.router.navigate(['/obligations']); },
        error: () => { this.saving = false; },
      });
    } else {
      this.obligationsService.create(value).subscribe({
        next: () => { this.router.navigate(['/obligations']); },
        error: () => { this.saving = false; },
      });
    }
  }

  onCancel() {
    this.router.navigate(['/obligations']);
  }
}