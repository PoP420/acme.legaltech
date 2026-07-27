import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ContractService } from '../services/contract.service';

@Component({
  selector: 'app-create-or-edit-contract',
  template: `
    <div class="container mt-3">
      <h2>Create Contract</h2>
      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <div class="mb-3">
          <label class="form-label">Title</label>
          <input class="form-control" formControlName="title" />
        </div>
        <div class="mb-3">
          <label class="form-label">Counterparty</label>
          <input class="form-control" formControlName="counterpartyName" />
        </div>
        <button type="submit" class="btn btn-primary" [disabled]="saving">Save</button>
        <button type="button" class="btn btn-secondary ms-2" (click)="onCancel()">Cancel</button>
      </form>
    </div>
  `,
  imports: [ReactiveFormsModule],
})
export class CreateOrEditContractComponent {
  form: FormGroup;
  saving = false;

  constructor(private fb: FormBuilder, private router: Router, private contractService: ContractService) {
    this.form = this.fb.group({
      title: ['', Validators.required],
      counterpartyName: ['', Validators.required],
    });
  }

  onSubmit() {
    if (this.form.invalid) return;
    this.saving = true;
    this.contractService.create(this.form.value).subscribe({
      next: () => {
        this.router.navigate(['/contracts']);
      },
      error: (err) => {
        console.error('Failed to save contract', err);
        this.saving = false;
      },
    });
  }

  onCancel() {
    this.router.navigate(['/contracts']);
  }
}
