import { Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ClausesComponent } from './clauses.component';

export const CLAUSES_ROUTES: Routes = [
  {
    path: '',
    component: ClausesComponent,
    canActivate: [permissionGuard],
<<<<<<< HEAD
    data: { requiredPolicy: 'LegalTech.Clauses' },
=======
    data: { requiredPolicy: 'LegalTech.Clauses.Default' },
>>>>>>> 9a366cd2c241dd347888a3c2f3176fc7686e7608
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