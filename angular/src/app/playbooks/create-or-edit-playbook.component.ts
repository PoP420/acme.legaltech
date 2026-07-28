import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { PlaybooksService } from '../services/playbooks.service';

@Component({
  selector: 'app-create-or-edit-playbook',
  template: `
    <div class="container mt-3">
      <h2>{{ isEdit ? 'Edit Playbook' : 'Create Playbook' }}</h2>
      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <div class="mb-3">
          <label class="form-label">Name</label>
          <input class="form-control" formControlName="name" />
        </div>
        <div class="mb-3">
          <label class="form-label">Description</label>
          <textarea class="form-control" formControlName="description" rows="3"></textarea>
        </div>
        <button type="submit" class="btn btn-primary" [disabled]="saving">{{ isEdit ? 'Update' : 'Create' }}</button>
        <button type="button" class="btn btn-secondary ms-2" (click)="onCancel()">Cancel</button>
      </form>
    </div>
  `,
  imports: [ReactiveFormsModule],
})
export class CreateOrEditPlaybookComponent {
  form: FormGroup;
  saving = false;
  isEdit = false;

  constructor(private fb: FormBuilder, private router: Router, private playbooksService: PlaybooksService) {
    this.form = this.fb.group({
      name: ['', Validators.required],
      description: [''],
    });
  }

  onSubmit() {
    if (this.form.invalid) return;
    this.saving = true;
    const value = this.form.value;
    if (this.isEdit) {
      this.playbooksService.update(value.id, value).subscribe({
        next: () => { this.router.navigate(['/playbooks']); },
        error: () => { this.saving = false; },
      });
    } else {
      this.playbooksService.create(value).subscribe({
        next: () => { this.router.navigate(['/playbooks']); },
        error: () => { this.saving = false; },
      });
    }
  }

  onCancel() {
    this.router.navigate(['/playbooks']);
  }
}