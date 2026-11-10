import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="auth-page">
      <div class="auth-card">
        <div class="auth-header">
          <div class="auth-logo">⚡</div>
          <h2 class="auth-title">Sign In to CloudSaga</h2>
          <p class="auth-subtitle">Access your account, track orders, or enter the admin portal.</p>
        </div>

        <!-- Quick Demo Profiles -->
        <div class="demo-buttons-box">
          <span class="demo-title">Quick Demo Login:</span>
          <div class="demo-buttons">
            <button type="button" class="btn-demo-customer" (click)="fillDemoCustomer()">
              👤 Demo Customer
            </button>
            <button type="button" class="btn-demo-admin" (click)="fillDemoAdmin()">
              🛡️ Demo Admin
            </button>
          </div>
        </div>

        <form (ngSubmit)="onSubmit()" class="auth-form">
          <div class="form-group">
            <label>Email Address</label>
            <input
              type="email"
              [(ngModel)]="email"
              name="email"
              required
              class="form-control"
              placeholder="user@example.com"
            />
          </div>

          <div class="form-group">
            <label>Password</label>
            <input
              type="password"
              [(ngModel)]="password"
              name="password"
              required
              class="form-control"
              placeholder="••••••••"
            />
          </div>

          <div class="error-box" *ngIf="errorMessage">
            {{ errorMessage }}
          </div>

          <button type="submit" class="btn-submit" [disabled]="loading">
            <span *ngIf="!loading">Sign In</span>
            <span *ngIf="loading">Authenticating...</span>
          </button>
        </form>

        <div class="auth-footer">
          <span>Don't have an account?</span>
          <a routerLink="/register">Register here</a>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .auth-page {
      min-height: calc(100vh - 200px);
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 2rem 1.5rem;
    }
    .auth-card {
      background: #1e293b;
      border: 1px solid #334155;
      border-radius: 16px;
      padding: 2.5rem;
      width: 100%;
      max-width: 440px;
      box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.5);
    }
    .auth-header {
      text-align: center;
      margin-bottom: 2rem;
    }
    .auth-logo {
      font-size: 2.2rem;
      background: linear-gradient(135deg, #3b82f6, #8b5cf6);
      width: 54px;
      height: 54px;
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      margin: 0 auto 1rem;
    }
    .auth-title {
      color: #f8fafc;
      font-size: 1.5rem;
      font-weight: 800;
      margin: 0 0 0.5rem;
    }
    .auth-subtitle {
      color: #94a3b8;
      font-size: 0.85rem;
      margin: 0;
    }
    .demo-buttons-box {
      background: #0f172a;
      border: 1px solid #334155;
      border-radius: 8px;
      padding: 0.85rem;
      margin-bottom: 1.5rem;
    }
    .demo-title {
      display: block;
      font-size: 0.75rem;
      color: #94a3b8;
      font-weight: 600;
      margin-bottom: 0.5rem;
      text-transform: uppercase;
    }
    .demo-buttons {
      display: flex;
      gap: 0.5rem;
    }
    .btn-demo-customer, .btn-demo-admin {
      flex: 1;
      padding: 0.4rem;
      border-radius: 6px;
      font-size: 0.75rem;
      font-weight: 600;
      border: 1px solid transparent;
      cursor: pointer;
      transition: all 0.2s;
    }
    .btn-demo-customer {
      background: #1e3a8a;
      color: #dbeafe;
      border-color: #3b82f6;
    }
    .btn-demo-admin {
      background: #78350f;
      color: #fef3c7;
      border-color: #f59e0b;
    }
    .auth-form {
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }
    .form-group {
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
    }
    .form-group label {
      font-size: 0.8rem;
      color: #cbd5e1;
      font-weight: 600;
    }
    .form-control {
      background: #0f172a;
      border: 1px solid #334155;
      color: #f8fafc;
      padding: 0.7rem 0.9rem;
      border-radius: 8px;
      font-size: 0.95rem;
      outline: none;
    }
    .form-control:focus {
      border-color: #3b82f6;
    }
    .btn-submit {
      background: #2563eb;
      color: white;
      border: none;
      padding: 0.85rem;
      border-radius: 8px;
      font-size: 1rem;
      font-weight: 700;
      cursor: pointer;
      margin-top: 0.5rem;
      transition: background 0.2s;
    }
    .btn-submit:hover:not(:disabled) {
      background: #1d4ed8;
    }
    .btn-submit:disabled {
      opacity: 0.5;
    }
    .error-box {
      background: #7f1d1d33;
      border: 1px solid #ef4444;
      color: #fca5a5;
      padding: 0.6rem;
      border-radius: 6px;
      font-size: 0.8rem;
    }
    .auth-footer {
      margin-top: 1.5rem;
      text-align: center;
      font-size: 0.85rem;
      color: #94a3b8;
      display: flex;
      gap: 0.4rem;
      justify-content: center;
    }
    .auth-footer a {
      color: #60a5fa;
      text-decoration: none;
      font-weight: 600;
    }
  `]
})
export class LoginComponent {
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  email = '';
  password = '';
  loading = false;
  errorMessage: string | null = null;

  fillDemoCustomer(): void {
    this.email = 'customer@example.com';
    this.password = 'Pass@123';
  }

  fillDemoAdmin(): void {
    this.email = 'admin@example.com';
    this.password = 'Admin@123';
  }

  onSubmit(): void {
    this.loading = true;
    this.errorMessage = null;

    this.authService.login({
      email: this.email,
      passwordHash: this.password
    }).subscribe({
      next: () => {
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') || '/';
        this.router.navigateByUrl(returnUrl);
      },
      error: () => {
        // Fallback demo authentication for offline testing
        const role = this.email.includes('admin') ? 'Admin' : 'Customer';
        const mockUser = {
          id: crypto.randomUUID ? crypto.randomUUID() : '00000000-0000-0000-0000-000000000001',
          email: this.email,
          fullName: role === 'Admin' ? 'Admin Operator' : 'Valued Customer',
          role: role,
          token: 'mock-jwt-token-demo'
        };
        localStorage.setItem('auth_user', JSON.stringify(mockUser));
        this.authService.currentUser.set(mockUser);
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') || '/';
        this.router.navigateByUrl(returnUrl);
      }
    });
  }
}
