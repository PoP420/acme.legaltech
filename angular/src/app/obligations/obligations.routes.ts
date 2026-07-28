import { Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ObligationsComponent } from './obligations.component';

export const OBLIGATIONS_ROUTES: Routes = [
  {
    path: '',
    component: ObligationsComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'LegalTech.Obligations' },
    children: [
      {
        path: '',
        redirectTo: 'list',
        pathMatch: 'full',
      },
      {
        path: 'list',
        loadComponent: () => import('./obligations-list.component').then(c => c.ObligationsListComponent),
      },
      {
        path: 'create',
        loadComponent: () => import('./create-or-edit-obligation.component').then(c => c.CreateOrEditObligationComponent),
      },
      {
        path: 'edit/:id',
        loadComponent: () => import('./create-or-edit-obligation.component').then(c => c.CreateOrEditObligationComponent),
      },
      {
        path: ':id',
        loadComponent: () => import('./obligation-detail.component').then(c => c.ObligationDetailComponent),
      },
    ],
  },
];