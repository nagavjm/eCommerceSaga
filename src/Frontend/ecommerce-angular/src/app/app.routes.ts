import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/catalog/catalog.component').then(m => m.CatalogComponent)
  },
  {
    path: 'products/:id',
    loadComponent: () => import('./features/catalog/product-detail.component').then(m => m.ProductDetailComponent)
  },
  {
    path: 'cart',
    loadComponent: () => import('./features/cart/cart.component').then(m => m.CartComponent)
  },
  {
    path: 'checkout',
    canActivate: [authGuard],
    loadComponent: () => import('./features/checkout/checkout.component').then(m => m.CheckoutComponent)
  },
  {
    path: 'saga/:orderId',
    loadComponent: () => import('./features/orders/order-tracker.component').then(m => m.OrderTrackerComponent)
  },
  {
    path: 'orders',
    canActivate: [authGuard],
    loadComponent: () => import('./features/orders/order-history.component').then(m => m.OrderHistoryComponent)
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'admin',
    canActivate: [adminGuard],
    loadComponent: () => import('./features/admin/admin-dashboard.component').then(m => m.AdminDashboardComponent),
    children: [
      {
        path: '',
        loadComponent: () => import('./features/admin/admin-overview.component').then(m => m.AdminOverviewComponent)
      },
      {
        path: 'products',
        loadComponent: () => import('./features/admin/admin-products.component').then(m => m.AdminProductsComponent)
      },
      {
        path: 'inventory',
        loadComponent: () => import('./features/admin/admin-inventory.component').then(m => m.AdminInventoryComponent)
      },
      {
        path: 'saga-monitor',
        loadComponent: () => import('./features/admin/admin-saga-monitor.component').then(m => m.AdminSagaMonitorComponent)
      }
    ]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
