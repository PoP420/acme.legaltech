import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-clause-detail',
  template: `
    <div class="card">
      <div class="card-header">
        <h3>Clause Detail</h3>
      </div>
      <div class="card-body">
        <p>Clause details will be shown here.</p>
        <a class="btn btn-secondary" [routerLink]="['/clauses']">Back to List</a>
      </div>
    </div>
  `,
  imports: [RouterLink]
})
export class ClauseDetailComponent {}