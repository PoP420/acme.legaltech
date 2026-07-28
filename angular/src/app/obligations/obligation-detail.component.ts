import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-obligation-detail',
  template: `
    <div class="card">
      <div class="card-header">
        <h3>Obligation Detail</h3>
      </div>
      <div class="card-body">
        <p>Obligation details will be shown here.</p>
        <a class="btn btn-secondary" [routerLink]="['/obligations']">Back to List</a>
      </div>
    </div>
  `,
  imports: [RouterLink]
})
export class ObligationDetailComponent {}