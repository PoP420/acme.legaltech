import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-playbook-detail',
  template: `
    <div class="card">
      <div class="card-header">
        <h3>Playbook Detail</h3>
      </div>
      <div class="card-body">
        <p>Playbook details will be shown here.</p>
        <a class="btn btn-secondary" [routerLink]="['/playbooks']">Back to List</a>
      </div>
    </div>
  `,
  imports: [RouterLink]
})
export class PlaybookDetailComponent {}