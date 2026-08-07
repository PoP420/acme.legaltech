import { Component } from '@angular/core';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { switchMap, EMPTY } from 'rxjs';
import { ObligationsService, ContractObligationDto, ContractObligationCreateDto, ContractObligationUpdateDto, RecurrencePatternLabels } from '../services/obligations.service';

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
          <div class="invalid-feedback d-block" *ngIf="form.get('contractId')?.hasError('pattern')">
            Must be a valid GUID.
          </div>
        </div>
        <div class="mb-3">
          <label class="form-label">Due Date</label>
          <input class="form-control" formControlName="dueDate" type="date" />
        </div>
        <div class="mb-3">
          <label class="form-label">Source Clause Reference</label>
          <input class="form-control" formControlName="sourceClauseReference" type="text" />
        </div>
        <div class="mb-3">
          <div class="form-check">
            <input class="form-check-input" type="checkbox" formControlName="isRecurring" />
            <label class="form-label mb-0" for="isRecurring">Is Recurring</label>
          </div>
        </div>
        <div class="mb-3" *ngIf="form.get('isRecurring')?.value">
          <label class="form-label">Recurrence Pattern</label>
          <select class="form-select" formControlName="recurrencePattern">
            <option [ngValue]="undefined">Select pattern...</option>
            <option *ngFor="let pattern of recurrencePatterns" [ngValue]="pattern.value">{{ pattern.label }}</option>
          </select>
        </div>
        <div class="mb-3">
          <label class="form-label">Priority</label>
          <input class="form-control" formControlName="priority" type="number" min="0" max="5" />
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
  editingId: string | null = null;

  guidPattern = /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/;

  recurrencePatterns = [
    { value: 'Daily', label: 'Daily' },
    { value: 'Weekly', label: 'Weekly' },
    { value: 'Monthly', label: 'Monthly' },
    { value: 'Quarterly', label: 'Quarterly' },
    { value: 'Annually', label: 'Annually' },
  ];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private obligationsService: ObligationsService,
  ) {
    this.form = this.fb.group({
      id: [''],
      title: ['', Validators.required],
      description: [''],
      contractId: ['', [Validators.required, Validators.pattern(this.guidPattern)]],
      dueDate: [''],
      sourceClauseReference: [''],
      isRecurring: [false],
      recurrencePattern: [''],
      priority: [0, [Validators.required, Validators.min(0), Validators.max(5)]],
    });

    this.loadObligationIfEdit();
  }

  private loadObligationIfEdit() {
    this.route.paramMap.pipe(
      switchMap((params: ParamMap) => {
        const id = params.get('id');
        if (id) {
          this.isEdit = true;
          this.editingId = id;
          return this.obligationsService.get(id);
        }
        this.isEdit = false;
        this.editingId = null;
        this.loadContractFromQuery();
        return EMPTY;
      }),
    ).subscribe((obligation: ContractObligationDto) => {
      if (obligation) {
        this.patchForm(obligation);
      }
    });
  }

  private loadContractFromQuery() {
    const contractId = this.route.snapshot.queryParamMap.get('contractId');
    if (contractId) {
      this.form.patchValue({ contractId });
    }
  }

  private patchForm(obligation: ContractObligationDto) {
    this.form.patchValue({
      id: obligation.id,
      title: obligation.title,
      description: obligation.description || '',
      contractId: obligation.contractId,
      dueDate: obligation.dueDate || '',
      sourceClauseReference: obligation.sourceClauseReference || '',
      isRecurring: obligation.isRecurring || false,
      recurrencePattern: obligation.recurrencePattern || '',
      priority: obligation.priority,
    });
  }

  onSubmit() {
    if (this.form.invalid) return;
    this.saving = true;
    const value = this.form.value;
    if (this.isEdit && this.editingId) {
      const updateInput: ContractObligationUpdateDto = {
        title: value.title,
        description: value.description,
        dueDate: value.dueDate || undefined,
        priority: value.priority,
      };
      this.obligationsService.update(this.editingId, updateInput).subscribe({
        next: () => { this.saving = false; this.router.navigate(['/obligations/list']); },
        error: () => { this.saving = false; },
      });
    } else {
      const createInput: ContractObligationCreateDto = {
        contractId: value.contractId,
        title: value.title,
        description: value.description,
        dueDate: value.dueDate || undefined,
        sourceClauseReference: value.sourceClauseReference || undefined,
        isRecurring: value.isRecurring,
        recurrencePattern: value.recurrencePattern || undefined,
        priority: value.priority,
      };
      this.obligationsService.create(createInput).subscribe({
        next: () => {
          this.saving = false;
          this.navigateAfterCreate(createInput.contractId);
        },
        error: () => { this.saving = false; },
      });
    }
  }

  private navigateAfterCreate(contractId?: string) {
    if (contractId) {
      this.router.navigate(['/contracts', contractId]);
    } else {
      this.router.navigate(['/obligations/list']);
    }
  }

  onCancel() {
    this.router.navigate(['/obligations/list']);
  }
}
