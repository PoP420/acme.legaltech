import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { switchMap, EMPTY } from 'rxjs';
import { PermissionDirective } from '@abp/ng.core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ContractService, ContractDto, ContractDocumentVersionDto, ContractStatusLabels, ContractStatus } from '../services/contract.service';

@Component({
  selector: 'app-contract-detail',
  template: `
    <div class="container mt-3">
      <a class="btn btn-secondary mb-3" [routerLink]="['/contracts/list']">&larr; Back to List</a>

      <div class="card mb-4" *ngIf="contract">
        <div class="card-header d-flex justify-content-between align-items-center">
          <h3>{{ contract.title }}</h3>
          <span class="badge bg-secondary">{{ statusLabel }}</span>
        </div>
        <div class="card-body">
          <dl class="row">
            <dt class="col-sm-3">Counterparty</dt>
            <dd class="col-sm-9">{{ contract.counterpartyName }}</dd>

            <dt class="col-sm-3">Category</dt>
            <dd class="col-sm-9">{{ contract.category || '-' }}</dd>

            <dt class="col-sm-3">Risk Baseline</dt>
            <dd class="col-sm-9">{{ contract.riskBaseline || '-' }}</dd>

            <dt class="col-sm-3">Effective Date</dt>
            <dd class="col-sm-9">{{ contract.effectiveDate || '-' }}</dd>

            <dt class="col-sm-3">Expiration Date</dt>
            <dd class="col-sm-9">{{ contract.expirationDate || '-' }}</dd>

            <dt class="col-sm-3">Owner</dt>
            <dd class="col-sm-9">{{ contract.ownerUserId || '-' }}</dd>
          </dl>
        </div>
      </div>

      <div class="card mb-4">
        <div class="card-header">
          <h4>Document Versions</h4>
        </div>
        <div class="card-body">
          <div class="mb-3" *abpPermission="'LegalTech.Contracts.AttachDocument'">
            <label class="form-label">Upload New Version</label>
            <input type="file" class="form-control mb-2" (change)="onFileSelected($event)" #fileInput />
            <input type="text" class="form-control mb-2" placeholder="Change note (optional)" [(ngModel)]="changeNote" />
            <button class="btn btn-primary" (click)="onUpload()" [disabled]="!selectedFile || uploading">
              {{ uploading ? 'Uploading...' : 'Upload' }}
            </button>
          </div>

          <div class="alert alert-danger" *ngIf="versionsError">
            {{ versionsErrorMessage }}
          </div>

          <table class="table" *ngIf="versions.length; else noVersions">
            <thead>
              <tr>
                <th>Version</th>
                <th>File</th>
                <th>Size</th>
                <th>Uploaded</th>
                <th>Change Note</th>
                <th>Extraction</th>
                <th class="text-end">Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let version of versions">
                <td>v{{ version.versionNumber }}</td>
                <td>{{ version.fileName }}</td>
                <td>{{ formatSize(version.fileSize) }}</td>
                <td>{{ version.uploadedAt }}</td>
                <td>{{ version.changeNote || '-' }}</td>
                <td>
                  <span class="badge" [ngClass]="extractionBadgeClass(version.extractionStatus)">
                    {{ extractionLabel(version.extractionStatus) }}
                  </span>
                  <div *ngIf="version.extractedTitle" class="mt-1 small text-muted">
                    {{ version.extractedTitle }}
                  </div>
                </td>
                <td class="text-end">
                  <button class="btn btn-sm btn-outline-primary me-1"
                          (click)="onDownload(version.id)"
                          *abpPermission="'LegalTech.Contracts.Default'">
                    Download
                  </button>
                  <button class="btn btn-sm btn-outline-danger"
                          (click)="onDelete(version.id)"
                          *abpPermission="'LegalTech.Contracts.AttachDocument'">
                    Delete
                  </button>
                </td>
              </tr>
            </tbody>
          </table>

          <ng-template #noVersions>
            <p class="text-muted">No document versions uploaded yet.</p>
          </ng-template>
        </div>
      </div>
    </div>
  `,
  imports: [CommonModule, RouterLink, PermissionDirective, FormsModule],
})
export class ContractDetailComponent {
  contract: ContractDto | null = null;
  versions: ContractDocumentVersionDto[] = [];
  selectedFile: File | null = null;
  changeNote = '';
  uploading = false;
  versionsError = false;
  versionsErrorMessage = '';

  get statusLabel(): string {
    return this.contract ? ContractStatusLabels[this.contract.status as ContractStatus] || String(this.contract.status) : '-';
  }

  constructor(
    private route: ActivatedRoute,
    private contractService: ContractService,
  ) {
    this.route.paramMap.pipe(
      switchMap((params: ParamMap) => {
        const id = params.get('id');
        if (!id) return EMPTY;
        return this.contractService.get(id);
      }),
    ).subscribe((contract: ContractDto) => {
      this.contract = contract;
      if (contract?.id) {
        this.loadVersions(contract.id);
      }
    });
  }

  private loadVersions(contractId: string) {
    this.versionsError = false;
    this.versionsErrorMessage = '';
    this.contractService.getVersions(contractId).subscribe({
      next: result => {
        this.versions = result.items ?? [];
      },
      error: err => {
        this.versionsError = true;
        this.versionsErrorMessage = err?.message || 'Failed to load document versions.';
        this.versions = [];
      },
    });
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    this.selectedFile = input.files?.[0] ?? null;
  }

  onUpload() {
    if (!this.selectedFile || !this.contract) return;
    this.uploading = true;
    this.contractService.upload(this.contract.id, this.selectedFile, this.changeNote || undefined).subscribe({
      next: (version) => {
        this.versions = [version, ...this.versions.filter(v => v.contractId === version.contractId)];
        this.selectedFile = null;
        this.changeNote = '';
        this.uploading = false;
      },
      error: () => {
        this.uploading = false;
      },
    });
  }

  onDownload(versionId: string) {
    this.contractService.download(versionId);
  }

  onDelete(versionId: string) {
    if (!confirm('Are you sure you want to delete this document version?')) return;
    this.contractService.deleteVersion(versionId).subscribe({
      next: () => {
        this.versions = this.versions.filter(v => v.id !== versionId);
      },
    });
  }

  extractionLabel(status: string | null | undefined): string {
    if (!status) return 'Pending';
    return status;
  }

  extractionBadgeClass(status: string | null | undefined): string {
    if (status === 'Success') return 'bg-success';
    if (status === 'Failed') return 'bg-danger';
    if (status === 'Error') return 'bg-warning';
    return 'bg-secondary';
  }

  formatSize(bytes: number): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }
}
