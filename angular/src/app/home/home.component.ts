import { Component } from '@angular/core';
import { LocalizationPipe } from '@abp/ng.core';
import { RouterLink } from '@angular/router';
import { PermissionDirective } from '@abp/ng.core';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  imports: [LocalizationPipe, RouterLink, PermissionDirective],
})
export class HomeComponent {}