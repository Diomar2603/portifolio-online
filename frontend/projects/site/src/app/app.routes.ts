import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', title: 'Início', loadComponent: () => import('./features/home/home').then((m) => m.Home) },
  { path: 'projetos', title: 'Projetos', loadComponent: () => import('./features/projects/projects').then((m) => m.Projects) },
  {
    path: 'projetos/:slug',
    loadComponent: () => import('./features/project-detail/project-detail').then((m) => m.ProjectDetail),
  },
  { path: '**', redirectTo: '' },
];
