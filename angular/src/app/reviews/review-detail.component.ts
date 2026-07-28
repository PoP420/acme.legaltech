import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-review-detail',
  template: `
    <div class="card">
      <div class="card-header">
        <h3>Review Case Detail</h3>
      </div>
      <div class="card-body">
        <p>Review case details will be shown here.</p>
        <a class="btn btn-secondary" [routerLink]="['/reviews']">Back to List</a>
      </div>
    </div>
  `,
  imports: [RouterLink]
})
export class ReviewDetailComponent {}