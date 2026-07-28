import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ClausesService } from '../services/clauses.service';

@Component({
  selector: 'app-create-or-edit-clause',
  template: `
    <div class="container mt-3">
      <h2>{{ isEdit ? 'Edit Clause' : 'Create Clause' }}</h2>
      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <div class="mb-3">
          <label class="form-label">Title</label>
          <input class="form-control" formControlName="title" />
        </div>
        <div class="mb-3">
          <label class="form-label">Content</label>
          <textarea class="form-control" formControlName="content" rows="6"></textarea>
        </div>
        <div class="mb-3">
          <label class="form-label">Jurisdiction</label>
          <input class="form-control" formControlName="jurisdiction" />
        </div>
        <div class="mb-3">
          <label class="form-label">Category</label>
          <input class="form-control" formControlName="category" />
        </div>
        <button type="submit" class="btn btn-primary" [disabled]="saving">{{ isEdit ? 'Update' : 'Create' }}</button>
        <button type="button" class="btn btn-secondary ms-2" (click)="onCancel()">Cancel</button>
      </form>
    </div>
  `,
  imports: [ReactiveFormsModule],
})
export class CreateOrEditClauseComponent {
  form: FormGroup;
  saving = false;
  isEdit = false;

  constructor(private fb: FormBuilder, private router: Router, private clausesService: ClausesService) {
    this.form = this.fb.group({
      title: ['', Validators.required],
      content: ['', Validators.required],
      taxonomyId: [null],
      jurisdiction: [''],
      category: [''],
      tags: [''],
      riskLevel: [''],
    });
  }

  onSubmit() {
    if (this.form.invalid) return;
    this.saving = true;
    const value = this.form.value;
    if (this.isEdit) {
      this.clausesService.update(value.id, value).subscribe({
        next: () => { this.router.navigate(['/clauses']); },
        error: () => { this.saving = false; },
      });
    } else {
      this.clausesService.create(value).subscribe({
        next: () => { this.router.navigate(['/clauses']); },
        error: () => { this.saving = false; },
      });
    }
  }

  onCancel() {
    this.router.navigate(['/clauses']);
  }
}