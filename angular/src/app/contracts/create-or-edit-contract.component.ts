import { Component } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormArray } from '@angular/forms';
import { ContractService, ContractDto } from '../services/contract.service';
import { switchMap } from 'rxjs';

@Component({
  selector: 'app-create-or-edit-contract',
  template: `
    <div class="container mt-3">
      <h2>{{ isEdit ? 'Edit Contract' : 'Create Contract' }}</h2>
      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <div class="mb-3">
          <label class="form-label">Title</label>
          <input class="form-control" formControlName="title" />
        </div>
        <div class="mb-3">
          <label class="form-label">Counterparty Name</label>
          <input class="form-control" formControlName="counterpartyName" />
        </div>
        <div class="mb-3">
          <label class="form-label">Category</label>
          <input class="form-control" formControlName="category" />
        </div>
        <div class="mb-3">
          <label class="form-label">Risk Baseline</label>
          <input class="form-control" formControlName="riskBaseline" />
        </div>
        <div class="row">
          <div class="col-md-6 mb-3">
            <label class="form-label">Effective Date</label>
            <input type="date" class="form-control" formControlName="effectiveDate" />
          </div>
          <div class="col-md-6 mb-3">
            <label class="form-label">Expiration Date</label>
            <input type="date" class="form-control" formControlName="expirationDate" />
          </div>
        </div>
        <div class="mb-3">
          <label class="form-label">Owner User ID</label>
          <input class="form-control" formControlName="ownerUserId" />
        </div>
        <div class="mb-3">
          <label class="form-label">Tags <small class="text-muted">(comma-separated)</small></label>
          <input class="form-control" [value]="tagsInput" (input)="onTagsInput($event)" />
        </div>
        <div class="mb-3">
          <label class="form-label">Counterparties</label>
          <div formArrayName="counterparties">
            <div *ngFor="let ctrl of counterparties.controls; let i = index" [formGroupName]="i" class="row g-2 mb-2">
              <div class="col-md-5">
                <input class="form-control" formControlName="name" placeholder="Name" />
              </div>
              <div class="col-md-5">
                <input class="form-control" formControlName="externalReference" placeholder="External Reference (optional)" />
              </div>
              <div class="col-md-2">
                <button type="button" class="btn btn-outline-danger" (click)="removeCounterparty(i)">Remove</button>
              </div>
            </div>
          </div>
          <button type="button" class="btn btn-outline-secondary btn-sm" (click)="addCounterparty()">Add Counterparty</button>
        </div>
        <button type="submit" class="btn btn-primary" [disabled]="saving">{{ isEdit ? 'Update' : 'Create' }}</button>
        <button type="button" class="btn btn-secondary ms-2" (click)="onCancel()">Cancel</button>
      </form>
    </div>
  `,
  imports: [ReactiveFormsModule],
})
export class CreateOrEditContractComponent {
  form: FormGroup;
  saving = false;
  isEdit = false;
  editingId: string | null = null;
  tagsInput = '';

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private contractService: ContractService,
  ) {
    this.form = this.fb.group({
      id: [''],
      title: ['', Validators.required],
      counterpartyName: ['', Validators.required],
      category: [''],
      riskBaseline: [''],
      effectiveDate: [''],
      expirationDate: [''],
      ownerUserId: [''],
      tags: [[]],
      counterparties: this.fb.array([]),
    });

    this.loadContractIfEdit();
  }

  get counterparties(): FormArray {
    return this.form.get('counterparties') as FormArray;
  }

  addCounterparty() {
    this.counterparties.push(this.fb.group({
      name: [''],
      externalReference: [''],
    }));
  }

  removeCounterparty(index: number) {
    this.counterparties.removeAt(index);
  }

  onTagsInput(event: Event) {
    const input = event.target as HTMLInputElement;
    this.tagsInput = input.value;
    const value = input.value;
    this.form.patchValue({
      tags: value ? value.split(',').map(t => t.trim()).filter(t => t.length > 0).map(name => ({ name })) : [],
    });
  }

  private loadContractIfEdit() {
    this.route.paramMap.pipe(
      switchMap(params => {
        const id = params.get('id');
        if (id) {
          this.isEdit = true;
          this.editingId = id;
          return this.contractService.get(id);
        }
        this.isEdit = false;
        this.editingId = null;
        return null;
      }),
    ).subscribe(contract => {
      if (contract) {
        this.patchForm(contract);
      }
    });
  }

  private patchForm(contract: ContractDto) {
    this.form.patchValue({
      id: contract.id,
      title: contract.title,
      counterpartyName: contract.counterpartyName,
      category: contract.category || '',
      riskBaseline: contract.riskBaseline || '',
      effectiveDate: contract.effectiveDate || '',
      expirationDate: contract.expirationDate || '',
      ownerUserId: contract.ownerUserId || '',
    });
    this.tagsInput = contract.tags?.map(t => t.name).join(', ') || '';
    if (contract.counterparties?.length) {
      this.counterparties.clear();
      contract.counterparties.forEach(c => {
        this.counterparties.push(this.fb.group({
          name: [c.name],
          externalReference: [c.externalReference || ''],
        }));
      });
    }
  }

  onSubmit() {
    if (this.form.invalid) return;
    this.saving = true;
    const value = this.form.value;
    if (this.isEdit && this.editingId) {
      this.contractService.update(this.editingId, value).subscribe({
        next: () => { this.router.navigate(['/contracts']); },
        error: () => { this.saving = false; },
      });
    } else {
      this.contractService.create(value).subscribe({
        next: () => { this.router.navigate(['/contracts']); },
        error: () => { this.saving = false; },
      });
    }
  }

  onCancel() {
    this.router.navigate(['/contracts']);
  }
}
