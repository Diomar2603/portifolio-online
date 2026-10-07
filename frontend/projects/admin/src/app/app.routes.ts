import { Routes } from '@angular/router';
import { AuthCallback } from './core/auth/auth-callback';
import { authGuard } from './core/auth/auth.guard';
import { Shell } from './layout/shell';

export const routes: Routes = [
  { path: 'auth/callback', component: AuthCallback },
  {
    path: '',
    component: Shell,
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'projetos' },
      { path: 'perfil', title: 'Perfil', loadComponent: () => import('./features/profile/profile.page').then((m) => m.ProfilePage) },
      { path: 'formacao', title: 'Formação', loadComponent: () => import('./features/education/education.page').then((m) => m.EducationPage) },
      { path: 'experiencia', title: 'Experiência', loadComponent: () => import('./features/experience/experience.page').then((m) => m.ExperiencePage) },
      { path: 'certificacoes', title: 'Certificações', loadComponent: () => import('./features/certifications/certifications.page').then((m) => m.CertificationsPage) },
      { path: 'categorias', title: 'Categorias', loadComponent: () => import('./features/categories/categories.page').then((m) => m.CategoriesPage) },
      { path: 'projetos', title: 'Projetos', loadComponent: () => import('./features/projects/projects.page').then((m) => m.ProjectsPage) },
      { path: 'midia', title: 'Mídia', loadComponent: () => import('./features/media/media.page').then((m) => m.MediaPage) },
    ],
  },
  { path: '**', redirectTo: '' },
];
