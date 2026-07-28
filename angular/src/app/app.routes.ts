import { authGuard, permissionGuard } from '@abp/ng.core';
import { Routes } from '@angular/router';
export const APP_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => import('./home/home.component').then(c => c.HomeComponent),
  },
  {
    path: 'account',
    loadChildren: () => import('@abp/ng.account').then(c => c.createRoutes()),
  },
  {
    path: 'identity',
    loadChildren: () => import('@abp/ng.identity').then(c => c.createRoutes()),
  },
  {
    path: 'tenant-management',
    loadChildren: () => import('@abp/ng.tenant-management').then(c => c.createRoutes()),
  },
  {
    path: 'setting-management',
    loadChildren: () => import('@abp/ng.setting-management').then(c => c.createRoutes()),
  },
  {
    path: 'contracts',
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'LegalTech.Contracts' },
    loadChildren: () => import('./contracts/contracts.routes').then(c => c.CONTRACTS_ROUTES),
  },
  {
    path: 'clauses',
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'LegalTech.Clauses' },
    loadChildren: () => import('./clauses/clauses.routes').then(c => c.CLAUSES_ROUTES),
  },
  {
    path: 'playbooks',
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'LegalTech.Playbooks' },
    loadChildren: () => import('./playbooks/playbooks.routes').then(c => c.PLAYBOOKS_ROUTES),
  },
  {
    path: 'reviews',
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'LegalTech.Reviews' },
    loadChildren: () => import('./reviews/reviews.routes').then(c => c.REVIEWS_ROUTES),
  },
  {
    path: 'obligations',
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'LegalTech.Obligations' },
    loadChildren: () => import('./obligations/obligations.routes').then(c => c.OBLIGATIONS_ROUTES),
  },
];
