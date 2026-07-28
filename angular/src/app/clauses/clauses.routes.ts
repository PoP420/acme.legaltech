import { Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ClausesComponent } from './clauses.component';

export const CLAUSES_ROUTES: Routes = [
  {
    path: '',
    component: ClausesComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'LegalTech.Clauses.Default' },
    children: [
      {
        path: '',
        redirectTo: 'list',
        pathMatch: 'full',
      },
      {
        path: 'list',
        loadComponent: () => import('./clauses-list.component').then(c => c.ClausesListComponent),
      },
      {
        path: 'create',
        loadComponent: () => import('./create-or-edit-clause.component').then(c => c.CreateOrEditClauseComponent),
      },
      {
        path: 'edit/:id',
        loadComponent: () => import('./create-or-edit-clause.component').then(c => c.CreateOrEditClauseComponent),
      },
      {
        path: ':id',
        loadComponent: () => import('./clause-detail.component').then(c => c.ClauseDetailComponent),
      },
    ],
  },
];