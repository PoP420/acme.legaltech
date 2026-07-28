import { Component } from '@angular/core';
import { LocalizationPipe } from '@abp/ng.core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  imports: [LocalizationPipe, RouterLink],
})
export class HomeComponent {}