import { Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { PlaybooksComponent } from './playbooks.component';

export const PLAYBOOKS_ROUTES: Routes = [
  {
    path: '',
    component: PlaybooksComponent,
    canActivate: [permissionGuard],
<<<<<<< HEAD
    data: { requiredPolicy: 'LegalTech.Clauses.Playbooks' },
=======
    data: { requiredPolicy: 'LegalTech.Playbooks.Default' },
>>>>>>> 9a366cd2c241dd347888a3c2f3176fc7686e7608
    children: [
      {
        path: '',
        redirectTo: 'list',
        pathMatch: 'full',
      },
      {
        path: 'list',
        loadComponent: () => import('./playbooks-list.component').then(c => c.PlaybooksListComponent),
      },
      {
        path: 'create',
        loadComponent: () => import('./create-or-edit-playbook.component').then(c => c.CreateOrEditPlaybookComponent),
      },
      {
        path: 'edit/:id',
        loadComponent: () => import('./create-or-edit-playbook.component').then(c => c.CreateOrEditPlaybookComponent),
      },
      {
        path: ':id',
        loadComponent: () => import('./playbook-detail.component').then(c => c.PlaybookDetailComponent),
      },
    ],
  },
];