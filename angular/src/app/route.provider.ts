import { RoutesService, eLayoutType } from '@abp/ng.core';
import { inject, provideAppInitializer } from '@angular/core';
export const APP_ROUTE_PROVIDER = [
  provideAppInitializer(() => {
    configureRoutes();
  }),
];
function configureRoutes() {
  const routes = inject(RoutesService);
  routes.add([
      {
        path: '/',
        name: '::Menu:Home',
        iconClass: 'fas fa-home',
        order: 1,
        layout: eLayoutType.application,
      },
      {
        path: 'contracts',
        name: '::Menu:Contracts',
        iconClass: 'fas fa-file-contract',
        order: 2,
        layout: eLayoutType.application,
      },
      {
        path: 'clauses',
        name: '::Menu:Clauses',
        iconClass: 'fas fa-file-lines',
        order: 3,
        layout: eLayoutType.application,
      },
      {
        path: 'playbooks',
        name: '::Menu:Playbooks',
        iconClass: 'fas fa-book',
        order: 4,
        layout: eLayoutType.application,
      },
      {
        path: 'reviews',
        name: '::Menu:Reviews',
        iconClass: 'fas fa-search-check',
        order: 5,
        layout: eLayoutType.application,
      },
      {
        path: 'obligations',
        name: '::Menu:Obligations',
        iconClass: 'fas fa-tasks',
        order: 6,
        layout: eLayoutType.application,
      },
  ]);
}
