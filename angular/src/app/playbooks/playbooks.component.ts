import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-playbooks',
  template: '<router-outlet></router-outlet>',
  imports: [RouterOutlet]
})
export class PlaybooksComponent {}