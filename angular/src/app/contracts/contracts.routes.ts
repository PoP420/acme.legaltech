import { Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ContractsComponent } from './contracts.component';

export const CONTRACTS_ROUTES: Routes = [
  {
    path: '',
    component: ContractsComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'LegalTech.Contracts' },
    children: [
      {
        path: '',
        redirectTo: 'list',
        pathMatch: 'full',
      },
      {
        path: 'list',
        loadComponent: () => import('./contracts-list.component').then(c => c.ContractsListComponent),
      },
      {
        path: 'create',
        loadComponent: () => import('./create-or-edit-contract.component').then(c => c.CreateOrEditContractComponent),
      },
      {
        path: 'edit/:id',
        loadComponent: () => import('./create-or-edit-contract.component').then(c => c.CreateOrEditContractComponent),
      },
      {
        path: ':id',
        canActivate: [permissionGuard],
        data: { requiredPolicy: 'LegalTech.Contracts.Default' },
        loadComponent: () => import('./contract-detail.component').then(c => c.ContractDetailComponent),
      },
    ],
  },
];
