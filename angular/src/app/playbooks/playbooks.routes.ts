import { Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { PlaybooksComponent } from './playbooks.component';

export const PLAYBOOKS_ROUTES: Routes = [
  {
    path: '',
    component: PlaybooksComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'LegalTech.Playbooks.Default' },
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