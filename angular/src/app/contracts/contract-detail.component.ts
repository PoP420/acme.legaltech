import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-contract-detail',
  template: `
    <div class="card">
      <div class="card-header">
        <h3>Contract Detail</h3>
      </div>
      <div class="card-body">
        <p>Contract details will be shown here.</p>
        <a class="btn btn-secondary" [routerLink]="['/contracts/list']">Back to List</a>
      </div>
    </div>
  `,
  imports: [RouterLink]
})
export class ContractDetailComponent {}
