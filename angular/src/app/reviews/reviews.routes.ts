import { Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ReviewsComponent } from './reviews.component';

export const REVIEWS_ROUTES: Routes = [
  {
    path: '',
    component: ReviewsComponent,
    canActivate: [permissionGuard],
    data: { requiredPolicy: 'LegalTech.Reviews' },
    children: [
      {
        path: '',
        redirectTo: 'list',
        pathMatch: 'full',
      },
      {
        path: 'list',
        loadComponent: () => import('./reviews-list.component').then(c => c.ReviewsListComponent),
      },
      {
        path: 'create',
        loadComponent: () => import('./create-or-edit-review.component').then(c => c.CreateOrEditReviewComponent),
      },
      {
        path: 'edit/:id',
        loadComponent: () => import('./create-or-edit-review.component').then(c => c.CreateOrEditReviewComponent),
      },
      {
        path: ':id',
        loadComponent: () => import('./review-detail.component').then(c => c.ReviewDetailComponent),
      },
    ],
  },
];