import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { switchMap, EMPTY } from 'rxjs';
import { PermissionDirective } from '@abp/ng.core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { ContractService, ContractDto, ContractDocumentVersionDto, ContractStatusLabels, ContractStatus, ContractSignatoryDto, VariationOrderDto, ApprovalAuthorityResultDto, AddSignatoryDto, AddVariationOrderDto, DocumentClassification, DocumentClassificationLabels, GovernmentSignatoryRole, GovernmentSignatoryRoleLabels, DocumentPartyType, DocumentPartyTypeLabels, ContractChangeStatusDto, ContractUpdateRequest } from '../services/contract.service';

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

            <dt class="col-sm-3">Document Number</dt>
            <dd class="col-sm-9">{{ contract.documentNumber || '-' }}</dd>

            <dt class="col-sm-3">Document Series</dt>
            <dd class="col-sm-9">{{ contract.documentSeries || '-' }}</dd>

            <dt class="col-sm-3">Document Year</dt>
            <dd class="col-sm-9">{{ contract.documentYear ?? '-' }}</dd>

            <dt class="col-sm-3">Classification</dt>
            <dd class="col-sm-9">{{ classificationLabel(contract.classification) }}</dd>

            <dt class="col-sm-3">Contract Value</dt>
            <dd class="col-sm-9">{{ contract.contractValue != null ? (contract.contractValue | currency:'USD') : '-' }}</dd>

            <dt class="col-sm-3">Retention Until</dt>
            <dd class="col-sm-9">{{ contract.retentionUntil || '-' }}</dd>
          </dl>

          <div class="mt-3" *abpPermission="'LegalTech.Contracts.ChangeStatus'">
            <label class="form-label">Change Status</label>
            <div class="btn-toolbar" role="toolbar">
              <button class="btn btn-outline-primary me-2" *ngIf="canActivate" (click)="onChangeStatus(1)">Activate</button>
              <button class="btn btn-outline-warning me-2" *ngIf="canExpire" (click)="onChangeStatus(2)">Expire</button>
              <button class="btn btn-outline-danger" *ngIf="canTerminate" (click)="onChangeStatus(3)">Terminate</button>
            </div>
          </div>
        </div>
      </div>

      <div class="card mb-4" *ngIf="contract?.currentAuthority">
        <div class="card-header">
          <h4>Current Approval Authority</h4>
        </div>
        <div class="card-body">
          <dl class="row">
            <dt class="col-sm-3">Authority</dt>
            <dd class="col-sm-9">{{ contract.currentAuthority.authorityTitle }}</dd>
            <dt class="col-sm-3">NEDA Review</dt>
            <dd class="col-sm-9">{{ contract.currentAuthority.requiresNedaReview ? 'Yes' : 'No' }}</dd>
            <dt class="col-sm-3">President Approval</dt>
            <dd class="col-sm-9">{{ contract.currentAuthority.requiresPresident ? 'Yes' : 'No' }}</dd>
            <dt class="col-sm-3">Allowable Variation</dt>
            <dd class="col-sm-9">{{ contract.currentAuthority.allowableVariationPercent }}%</dd>
          </dl>
        </div>
      </div>

      <div class="card mb-4">
        <div class="card-header">
          <h4>Signatories</h4>
        </div>
        <div class="card-body">
          <div class="alert alert-danger" *ngIf="signatoryError">
            {{ signatoryErrorMessage }}
            <button class="btn btn-sm btn-outline-danger ms-2" (click)="clearSignatoryError()">Dismiss</button>
          </div>
          <div *abpPermission="'LegalTech.Contracts.ManageSignatories'">
            <div class="row g-2 mb-3">
              <div class="col-md-3">
                <select class="form-select" [(ngModel)]="newSignatory.role">
                  <option [ngValue]="undefined">Role</option>
                  <option *ngFor="let item of signatoryRoleOptions" [ngValue]="item.value">{{ item.label }}</option>
                </select>
              </div>
              <div class="col-md-3">
                <select class="form-select" [(ngModel)]="newSignatory.partyType">
                  <option [ngValue]="undefined">Party Type</option>
                  <option *ngFor="let item of partyTypeOptions" [ngValue]="item.value">{{ item.label }}</option>
                </select>
              </div>
              <div class="col-md-3">
                <input class="form-control" placeholder="Party ID" [(ngModel)]="newSignatory.partyId" />
              </div>
              <div class="col-md-3">
                <input class="form-control" placeholder="Government Agency" [(ngModel)]="newSignatory.governmentAgency" />
              </div>
            </div>
            <div class="row g-2 mb-3">
              <div class="col-md-3">
                <input class="form-control" placeholder="Capacity" [(ngModel)]="newSignatory.capacity" />
              </div>
              <div class="col-md-2">
                <input type="number" class="form-control" placeholder="Order" [(ngModel)]="newSignatory.order" />
              </div>
              <div class="col-md-3">
                <input type="date" class="form-control" [(ngModel)]="newSignatory.signedOn" />
              </div>
              <div class="col-md-2">
                <button class="btn btn-primary" (click)="onAddSignatory()" [disabled]="!contract || !newSignatory.role || !newSignatory.partyType || !newSignatory.partyId">Add Signatory</button>
              </div>
            </div>
          </div>
          <table class="table" *ngIf="signatories.length; else noSignatories">
            <thead>
              <tr>
                <th>Role</th>
                <th>Party Type</th>
                <th>Party ID</th>
                <th>Agency</th>
                <th>Capacity</th>
                <th>Order</th>
                <th>Signed On</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let signatory of signatories">
                <td>{{ signatoryRoleLabel(signatory.role) }}</td>
                <td>{{ partyTypeLabel(signatory.partyType) }}</td>
                <td>{{ signatory.partyId }}</td>
                <td>{{ signatory.governmentAgency }}</td>
                <td>{{ signatory.capacity }}</td>
                <td>{{ signatory.order }}</td>
                <td>{{ signatory.signedOn || '-' }}</td>
              </tr>
            </tbody>
          </table>
          <ng-template #noSignatories>
            <p class="text-muted">No signatories added yet.</p>
          </ng-template>
        </div>
      </div>

      <div class="card mb-4">
        <div class="card-header">
          <h4>Variation Orders</h4>
        </div>
        <div class="card-body">
          <div class="alert alert-danger" *ngIf="variationError">
            {{ variationErrorMessage }}
            <button class="btn btn-sm btn-outline-danger ms-2" (click)="clearVariationError()">Dismiss</button>
          </div>
          <div class="alert alert-warning" *ngIf="!contract?.contractValue && canAddVariation">
            Contract Value is required to add variation orders.
          </div>
          <div *abpPermission="'LegalTech.Contracts.Amend'">
            <div class="row g-2 mb-3" *ngIf="contract?.contractValue">
              <div class="col-md-6">
                <input class="form-control" placeholder="Description" [(ngModel)]="newVariation.description" />
              </div>
              <div class="col-md-3">
                <input type="number" step="0.01" class="form-control" placeholder="Amount" [(ngModel)]="newVariation.amount" />
              </div>
              <div class="col-md-2">
                <button class="btn btn-primary" (click)="onAddVariation()" [disabled]="!newVariation.description || !newVariation.amount">Add Variation</button>
              </div>
            </div>
          </div>
          <table class="table" *ngIf="variationOrders.length; else noVariations">
            <thead>
              <tr>
                <th>Description</th>
                <th>Amount</th>
                <th>Cumulative</th>
                <th>Approved On</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let variation of variationOrders">
                <td>{{ variation.description }}</td>
                <td>{{ variation.amount | currency:'USD' }}</td>
                <td>{{ variation.cumulativeAmount | currency:'USD' }}</td>
                <td>{{ variation.approvedOn || '-' }}</td>
              </tr>
            </tbody>
          </table>
          <ng-template #noVariations>
            <p class="text-muted">No variation orders yet.</p>
          </ng-template>
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
            <button class="btn btn-sm btn-outline-danger ms-2" (click)="loadVersions(contract!.id!)">Retry</button>
          </div>

          <div class="alert alert-danger" *ngIf="uploadError">
            {{ uploadErrorMessage }}
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
                          *abpPermission="'LegalTech.Contracts'">
                    Download
                  </button>
                  <button class="btn btn-sm btn-outline-info me-1"
                          (click)="viewFile(version)"
                          *abpPermission="'LegalTech.Contracts'">
                    View
                  </button>
                  <ng-container *abpPermission="'LegalTech.Contracts.Edit'">
                  <button class="btn btn-sm btn-outline-info me-1"
                          (click)="openExtractionReview(version)"
                          *ngIf="version.extractionStatus === 'Success'">
                    Review
                  </button>
                  <button class="btn btn-sm btn-outline-warning me-1"
                          (click)="retryExtraction(version)"
                          *ngIf="version.extractionStatus === 'Failed' || version.extractionStatus === 'Error'">
                    Retry
                  </button>
                </ng-container>
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

    <div class="modal-backdrop fade show" *ngIf="previewVisible" (click)="closePreview()"></div>
    <div class="modal fade show d-block" tabindex="-1" *ngIf="previewVisible" role="dialog" aria-modal="true" aria-labelledby="previewModalTitle">
      <div class="modal-dialog modal-xl modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title" id="previewModalTitle">{{ previewVersion?.fileName }}</h5>
            <button type="button" class="btn-close" (click)="closePreview()" aria-label="Close"></button>
          </div>
          <div class="modal-body p-0" style="height: 75vh;">
            <div *ngIf="previewVersion && isImage(previewVersion)" class="w-100 h-100 d-flex align-items-center justify-content-center" style="background: #f0f0f0;">
              <img *ngIf="previewBlobUrl" [src]="previewBlobUrl" class="img-fluid" (load)="previewLoaded = true" (error)="previewLoaded = false" [alt]="previewVersion.fileName" />
              <div *ngIf="!previewBlobUrl" class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
              </div>
            </div>
            <div *ngIf="previewVersion && !isImage(previewVersion) && !isPdf(previewVersion)" class="w-100 h-100 d-flex flex-column align-items-center justify-content-center">
              <p class="text-muted mb-3">Preview not available for this file type.</p>
              <button class="btn btn-primary" (click)="onDownload(previewVersion.id)">Download to View</button>
            </div>
            <iframe *ngIf="previewVersion && isPdf(previewVersion)" [src]="previewBlobUrl || ''" class="w-100 h-100 border-0" title="PDF Preview"></iframe>
          </div>
        </div>
      </div>
    </div>

    <div class="modal-backdrop fade show" *ngIf="extractionReviewVisible" (click)="closeExtractionReview()"></div>
    <div class="modal fade show d-block" tabindex="-1" *ngIf="extractionReviewVisible" role="dialog" aria-modal="true">
      <div class="modal-dialog modal-lg modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Review AI Extraction: {{ selectedVersionForReview?.fileName }}</h5>
            <button type="button" class="btn-close" (click)="closeExtractionReview()" aria-label="Close"></button>
          </div>
          <div class="modal-body">
            <div class="alert alert-info">
              AI extracted the following values. Accept to update the contract, or edit and accept.
            </div>
            <form [formGroup]="extractionForm">
              <div class="mb-3">
                <label class="form-label">Title</label>
                <input class="form-control" formControlName="title" />
              </div>
              <div class="mb-3">
                <label class="form-label">Counterparty</label>
                <input class="form-control" formControlName="counterparty" />
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
              <div class="row">
                <div class="col-md-6 mb-3">
                  <label class="form-label">Category</label>
                  <input class="form-control" formControlName="category" />
                </div>
                <div class="col-md-6 mb-3">
                  <label class="form-label">Risk Baseline</label>
                  <input class="form-control" formControlName="riskBaseline" />
                </div>
              </div>
            </form>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" (click)="closeExtractionReview()">Cancel</button>
            <button type="button" class="btn btn-primary" (click)="acceptExtraction()" [disabled]="extractionForm.invalid">Accept & Update Contract</button>
          </div>
        </div>
      </div>
    </div>
  `,
  imports: [CommonModule, RouterLink, PermissionDirective, FormsModule, ReactiveFormsModule],
})
export class ContractDetailComponent {
  contract: ContractDto | null = null;
  versions: ContractDocumentVersionDto[] = [];
  signatories: ContractSignatoryDto[] = [];
  variationOrders: VariationOrderDto[] = [];
  selectedFile: File | null = null;
  changeNote = '';
  uploading = false;
  uploadError = false;
  uploadErrorMessage = '';
  versionsError = false;
  versionsErrorMessage = '';
  previewVisible = false;
  previewVersion: ContractDocumentVersionDto | null = null;
  previewLoaded = true;
  previewBlobUrl: SafeResourceUrl | null = null;
  private currentPreviewObjectUrl: string | null = null;

  extractionReviewVisible = false;
  selectedVersionForReview: ContractDocumentVersionDto | null = null;
  extractionForm: FormGroup;

  signatoryError = false;
  signatoryErrorMessage = '';
  variationError = false;
  variationErrorMessage = '';

  newSignatory: AddSignatoryDto = {
    role: undefined as any,
    partyType: undefined as any,
    partyId: '',
    governmentAgency: '',
    capacity: '',
    order: 0,
    signedOn: '',
  };

  newVariation: AddVariationOrderDto = {
    description: '',
    amount: 0,
  };

  signatoryRoleOptions = [
    { value: 0 as GovernmentSignatoryRole, label: 'Prepared By' },
    { value: 1 as GovernmentSignatoryRole, label: 'Reviewed By' },
    { value: 2 as GovernmentSignatoryRole, label: 'Endorsed By' },
    { value: 3 as GovernmentSignatoryRole, label: 'Approved By' },
    { value: 4 as GovernmentSignatoryRole, label: 'Authorized Signatory' },
    { value: 5 as GovernmentSignatoryRole, label: 'Noted By' },
  ];

  partyTypeOptions = [
    { value: 0 as DocumentPartyType, label: 'Government Unit' },
    { value: 1 as DocumentPartyType, label: 'Individual' },
    { value: 2 as DocumentPartyType, label: 'External' },
  ];

  get statusLabel(): string {
    return this.contract ? ContractStatusLabels[this.contract.status as ContractStatus] || String(this.contract.status) : '-';
  }

  get canActivate(): boolean {
    return this.contract?.status === 0;
  }

  get canExpire(): boolean {
    return this.contract?.status === 1;
  }

  get canTerminate(): boolean {
    return this.contract?.status === 1;
  }

  get canAddVariation(): boolean {
    return this.contract?.contractValue != null && this.contract?.contractValue > 0;
  }

  constructor(
    private route: ActivatedRoute,
    private contractService: ContractService,
    private sanitizer: DomSanitizer,
    private fb: FormBuilder,
  ) {
    this.extractionForm = this.fb.group({
      title: ['', Validators.required],
      counterparty: ['', Validators.required],
      effectiveDate: [''],
      expirationDate: [''],
      category: [''],
      riskBaseline: [''],
    });
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
        this.loadSignatories(contract.id);
        this.loadVariationOrders(contract.id);
      }
    });
  }

  loadVersions(contractId: string) {
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

  loadSignatories(contractId: string) {
    this.signatoryError = false;
    this.signatoryErrorMessage = '';
    if (this.contract?.signatories?.length) {
      this.signatories = [...this.contract.signatories];
    } else {
      this.signatories = [];
    }
  }

  loadVariationOrders(contractId: string) {
    this.variationError = false;
    this.variationErrorMessage = '';
    if (this.contract?.variationOrders?.length) {
      this.variationOrders = [...this.contract.variationOrders];
    } else {
      this.variationOrders = [];
    }
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    this.selectedFile = input.files?.[0] ?? null;
  }

  onUpload() {
    if (!this.selectedFile || !this.contract) return;
    this.uploading = true;
    this.uploadError = false;
    this.uploadErrorMessage = '';
    this.contractService.upload(this.contract.id, this.selectedFile, this.changeNote || undefined).subscribe({
      next: (version) => {
        this.versions = [version, ...this.versions.filter(v => v.contractId === version.contractId)];
        this.selectedFile = null;
        this.changeNote = '';
        this.uploading = false;
      },
      error: (err) => {
        this.uploading = false;
        this.uploadError = true;
        this.uploadErrorMessage = err?.message || 'Failed to upload document.';
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

  viewFile(version: ContractDocumentVersionDto) {
    if (this.currentPreviewObjectUrl) {
      URL.revokeObjectURL(this.currentPreviewObjectUrl);
      this.currentPreviewObjectUrl = null;
    }

    this.previewVersion = version;
    this.previewVisible = true;
    this.previewLoaded = true;
    this.previewBlobUrl = null;

    if (this.isImage(version) || this.isPdf(version)) {
      this.contractService.getBlob(version.id).subscribe({
        next: blob => {
          const objectUrl = URL.createObjectURL(blob);
          this.currentPreviewObjectUrl = objectUrl;
          this.previewBlobUrl = this.sanitizer.bypassSecurityTrustResourceUrl(objectUrl);
        },
        error: () => {
          this.previewBlobUrl = null;
        },
      });
    }
  }

  closePreview() {
    this.previewVisible = false;
    this.previewVersion = null;
    this.previewBlobUrl = null;
    if (this.currentPreviewObjectUrl) {
      URL.revokeObjectURL(this.currentPreviewObjectUrl);
      this.currentPreviewObjectUrl = null;
    }
  }

  getPreviewUrl(version: ContractDocumentVersionDto): string {
    return this.contractService.getDocumentDownloadUrl(version.id);
  }

  getPreviewSafeUrl(version: ContractDocumentVersionDto): SafeResourceUrl {
    return this.sanitizer.bypassSecurityTrustResourceUrl(this.getPreviewUrl(version));
  }

  isImage(version: ContractDocumentVersionDto): boolean {
    return version.contentType?.startsWith('image/') ?? false;
  }

  isPdf(version: ContractDocumentVersionDto): boolean {
    return version.contentType === 'application/pdf';
  }

  extractionLabel(status: string | null | undefined): string {
    if (!status) return 'Processing...';
    if (status === 'Success') return 'Review';
    if (status === 'Failed') return 'Failed';
    if (status === 'Error') return 'Error';
    return status;
  }

  extractionBadgeClass(status: string | null | undefined): string {
    if (status === 'Success') return 'bg-success';
    if (status === 'Failed') return 'bg-danger';
    if (status === 'Error') return 'bg-danger';
    return 'bg-warning';
  }

  openExtractionReview(version: ContractDocumentVersionDto) {
    this.selectedVersionForReview = version;
    this.extractionForm.patchValue({
      title: version.extractedTitle || this.contract?.title || '',
      counterparty: version.extractedCounterparty || this.contract?.counterpartyName || '',
      effectiveDate: version.extractedEffectiveDate || this.contract?.effectiveDate || '',
      expirationDate: version.extractedExpirationDate || this.contract?.expirationDate || '',
      category: version.extractedCategory || this.contract?.category || '',
      riskBaseline: version.extractedRiskBaseline || this.contract?.riskBaseline || '',
    });
    this.extractionReviewVisible = true;
  }

  closeExtractionReview() {
    this.extractionReviewVisible = false;
    this.selectedVersionForReview = null;
    this.extractionForm.reset();
  }

  acceptExtraction() {
    if (!this.contract || this.extractionForm.invalid) return;
    const formValue = this.extractionForm.value;
    const updateRequest: ContractUpdateRequest = {
      title: formValue.title,
      counterpartyName: formValue.counterparty,
      effectiveDate: formValue.effectiveDate,
      expirationDate: formValue.expirationDate,
      category: formValue.category,
      riskBaseline: formValue.riskBaseline,
    };
    this.contractService.update(this.contract.id, updateRequest).subscribe({
      next: (updated) => {
        this.contract = updated;
        this.loadVersions(updated.id);
        this.closeExtractionReview();
      },
      error: (err) => {
        alert(err?.message || 'Failed to update contract with extraction values.');
      },
    });
  }

  retryExtraction(version: ContractDocumentVersionDto) {
    this.onUpload();
  }

  formatSize(bytes: number): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  onChangeStatus(targetStatus: ContractStatus) {
    if (!this.contract) return;
    if (!confirm(`Change status to ${ContractStatusLabels[targetStatus]}?`)) return;
    this.contractService.changeStatus(this.contract.id, { targetStatus }).subscribe({
      next: (updated) => {
        this.contract = updated;
        this.loadSignatories(updated.id);
        this.loadVariationOrders(updated.id);
      },
      error: (err) => {
        alert(err?.message || 'Failed to change status.');
      },
    });
  }

  onAddSignatory() {
    if (!this.contract || !this.newSignatory.role || !this.newSignatory.partyType || !this.newSignatory.partyId) return;
    this.contractService.addSignatory(this.contract.id, { ...this.newSignatory }).subscribe({
      next: (signatory) => {
        this.signatories = [...this.signatories, signatory];
        this.newSignatory = {
          role: undefined as any,
          partyType: undefined as any,
          partyId: '',
          governmentAgency: '',
          capacity: '',
          order: 0,
          signedOn: '',
        };
      },
      error: (err) => {
        this.signatoryError = true;
        this.signatoryErrorMessage = err?.message || 'Failed to add signatory.';
      },
    });
  }

  clearSignatoryError() {
    this.signatoryError = false;
    this.signatoryErrorMessage = '';
  }

  onAddVariation() {
    if (!this.contract || !this.newVariation.description || !this.newVariation.amount) return;
    if (!this.contract.contractValue) {
      this.variationError = true;
      this.variationErrorMessage = 'ContractValue is required to add a variation order.';
      return;
    }
    this.contractService.addVariationOrder(this.contract.id, { ...this.newVariation }).subscribe({
      next: (variation) => {
        this.variationOrders = [...this.variationOrders, variation];
        this.newVariation = { description: '', amount: 0 };
      },
      error: (err) => {
        this.variationError = true;
        this.variationErrorMessage = err?.message || 'Failed to add variation order.';
      },
    });
  }

  clearVariationError() {
    this.variationError = false;
    this.variationErrorMessage = '';
  }

  classificationLabel(classification: DocumentClassification | undefined): string {
    if (classification === undefined || classification === null) return '-';
    return DocumentClassificationLabels[classification] || String(classification);
  }

  signatoryRoleLabel(role: GovernmentSignatoryRole): string {
    return GovernmentSignatoryRoleLabels[role] || String(role);
  }

  partyTypeLabel(partyType: DocumentPartyType): string {
    return DocumentPartyTypeLabels[partyType] || String(partyType);
  }
}
