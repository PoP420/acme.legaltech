import { Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ReportsComponent } from './reports.component';

export const REPORTS_ROUTES: Routes = [
  {
    path: '',
    component: ReportsComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'LegalTech.Reports' },
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./dashboard.component').then(c => c.DashboardComponent),
      },
      {
        path: 'obligations-health',
        loadComponent: () =>
          import('./obligations-health.component').then(
            c => c.ObligationsHealthComponent,
          ),
      },
      {
        path: 'reviews-pipeline',
        loadComponent: () =>
          import('./reviews-pipeline.component').then(
            c => c.ReviewsPipelineComponent,
          ),
      },
    ],
  },
];