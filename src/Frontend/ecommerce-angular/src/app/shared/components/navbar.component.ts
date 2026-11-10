import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { CartService } from '../../core/services/cart.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <header class="navbar-header">
      <div class="navbar-container">
        <!-- Logo -->
        <a routerLink="/" class="navbar-brand">
          <div class="brand-logo-icon">⚡</div>
          <div class="brand-text">
            <span class="brand-title">CloudSaga</span>
            <span class="brand-sub">Microservices Store</span>
          </div>
        </a>

        <!-- Main Navigation Links -->
        <nav class="navbar-nav">
          <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{exact: true}" class="nav-link">
            Catalog
          </a>
          <a routerLink="/orders" routerLinkActive="active" *ngIf="authService.isAuthenticated()" class="nav-link">
            My Orders
          </a>
          <a routerLink="/admin" routerLinkActive="active" *ngIf="authService.isAdmin()" class="nav-link admin-nav-link">
            Admin Panel
          </a>
        </nav>

        <!-- Right Side: Cart + Auth Profile -->
        <div class="navbar-actions">
          <!-- Cart Button -->
          <a routerLink="/cart" class="cart-btn" aria-label="Shopping Cart">
            <span class="cart-icon">🛒</span>
            <span class="cart-label">Cart</span>
            <span class="cart-badge" *ngIf="cartService.itemCount() > 0">
              {{ cartService.itemCount() }}
            </span>
          </a>

          <!-- Auth Actions -->
          <ng-container *ngIf="authService.currentUser() as user; else loginButton">
            <div class="user-profile">
              <span class="role-tag" [ngClass]="user.role.toLowerCase()">
                {{ user.role }}
              </span>
              <span class="user-name">{{ user.fullName }}</span>
              <button class="btn-logout" (click)="authService.logout()">Logout</button>
            </div>
          </ng-container>

          <ng-template #loginButton>
            <div class="auth-buttons">
              <a routerLink="/login" class="btn-login">Sign In</a>
              <a routerLink="/register" class="btn-register">Register</a>
            </div>
          </ng-template>
        </div>
      </div>
    </header>
  `,
  styles: [`
    .navbar-header {
      background: #1e293b;
      color: #f8fafc;
      border-bottom: 1px solid #334155;
      position: sticky;
      top: 0;
      z-index: 1000;
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
    }
    .navbar-container {
      max-width: 1280px;
      margin: 0 auto;
      padding: 0.75rem 1.5rem;
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 1.5rem;
    }
    .navbar-brand {
      display: flex;
      align-items: center;
      gap: 0.6rem;
      text-decoration: none;
      color: inherit;
    }
    .brand-logo-icon {
      font-size: 1.6rem;
      background: linear-gradient(135deg, #3b82f6, #8b5cf6);
      border-radius: 8px;
      padding: 4px 8px;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .brand-text {
      display: flex;
      flex-direction: column;
    }
    .brand-title {
      font-weight: 700;
      font-size: 1.25rem;
      letter-spacing: -0.02em;
      background: linear-gradient(135deg, #60a5fa, #c084fc);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
    }
    .brand-sub {
      font-size: 0.7rem;
      color: #94a3b8;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
    .navbar-nav {
      display: flex;
      gap: 1.25rem;
      align-items: center;
    }
    .nav-link {
      color: #cbd5e1;
      text-decoration: none;
      font-weight: 500;
      font-size: 0.95rem;
      padding: 0.4rem 0.8rem;
      border-radius: 6px;
      transition: all 0.2s ease;
    }
    .nav-link:hover {
      color: #ffffff;
      background: #334155;
    }
    .nav-link.active {
      color: #60a5fa;
      background: #1e3a8a33;
      border: 1px solid #3b82f655;
    }
    .admin-nav-link {
      color: #f59e0b;
    }
    .admin-nav-link:hover {
      color: #fbbf24;
      background: #78350f33;
    }
    .navbar-actions {
      display: flex;
      align-items: center;
      gap: 1rem;
    }
    .cart-btn {
      display: flex;
      align-items: center;
      gap: 0.4rem;
      background: #0f172a;
      border: 1px solid #334155;
      padding: 0.45rem 0.9rem;
      border-radius: 8px;
      color: #f1f5f9;
      text-decoration: none;
      font-weight: 600;
      position: relative;
      transition: all 0.2s ease;
    }
    .cart-btn:hover {
      background: #1e293b;
      border-color: #3b82f6;
    }
    .cart-badge {
      background: #ef4444;
      color: white;
      font-size: 0.75rem;
      font-weight: 700;
      padding: 0.15rem 0.5rem;
      border-radius: 9999px;
      margin-left: 0.25rem;
    }
    .user-profile {
      display: flex;
      align-items: center;
      gap: 0.6rem;
      background: #0f172a;
      padding: 0.35rem 0.75rem;
      border-radius: 8px;
      border: 1px solid #334155;
    }
    .user-name {
      font-size: 0.85rem;
      font-weight: 500;
      color: #e2e8f0;
    }
    .role-tag {
      font-size: 0.65rem;
      font-weight: 700;
      padding: 0.15rem 0.4rem;
      border-radius: 4px;
      text-transform: uppercase;
    }
    .role-tag.admin {
      background: #b45309;
      color: #fef3c7;
    }
    .role-tag.customer {
      background: #1d4ed8;
      color: #dbeafe;
    }
    .btn-logout {
      background: transparent;
      border: 1px solid #475569;
      color: #94a3b8;
      padding: 0.25rem 0.5rem;
      font-size: 0.75rem;
      border-radius: 4px;
      cursor: pointer;
      transition: all 0.2s ease;
    }
    .btn-logout:hover {
      background: #dc2626;
      border-color: #dc2626;
      color: white;
    }
    .auth-buttons {
      display: flex;
      gap: 0.5rem;
    }
    .btn-login {
      color: #e2e8f0;
      text-decoration: none;
      padding: 0.4rem 0.8rem;
      font-size: 0.85rem;
      font-weight: 500;
      border: 1px solid #475569;
      border-radius: 6px;
      transition: all 0.2s;
    }
    .btn-login:hover {
      background: #334155;
      color: white;
    }
    .btn-register {
      background: #2563eb;
      color: white;
      text-decoration: none;
      padding: 0.4rem 0.8rem;
      font-size: 0.85rem;
      font-weight: 500;
      border-radius: 6px;
      transition: all 0.2s;
    }
    .btn-register:hover {
      background: #1d4ed8;
    }
  `]
})
export class NavbarComponent {
  authService = inject(AuthService);
  cartService = inject(CartService);
}
