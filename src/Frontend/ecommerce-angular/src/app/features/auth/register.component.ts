import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="auth-page">
      <div class="auth-card">
        <div class="auth-header">
          <div class="auth-logo">⚡</div>
          <h2 class="auth-title">Create an Account</h2>
          <p class="auth-subtitle">Join CloudSaga for high-speed cloud shopping.</p>
        </div>

        <form (ngSubmit)="onSubmit()" class="auth-form">
          <div class="form-group">
            <label>Full Name</label>
            <input
              type="text"
              [(ngModel)]="fullName"
              name="fullName"
              required
              class="form-control"
              placeholder="Jane Doe"
            />
          </div>

          <div class="form-group">
            <label>Email Address</label>
            <input
              type="email"
              [(ngModel)]="email"
              name="email"
              required
              class="form-control"
              placeholder="jane@example.com"
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

          <div class="form-group">
            <label>Account Role</label>
            <select [(ngModel)]="role" name="role" class="form-control">
              <option value="Customer">Customer</option>
              <option value="Admin">Admin</option>
            </select>
          </div>

          <div class="error-box" *ngIf="errorMessage">
            {{ errorMessage }}
          </div>

          <button type="submit" class="btn-submit" [disabled]="loading">
            <span *ngIf="!loading">Create Account</span>
            <span *ngIf="loading">Creating account...</span>
          </button>
        </form>

        <div class="auth-footer">
          <span>Already have an account?</span>
          <a routerLink="/login">Sign in here</a>
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
export class RegisterComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  fullName = '';
  email = '';
  password = '';
  role = 'Customer';
  loading = false;
  errorMessage: string | null = null;

  onSubmit(): void {
    this.loading = true;
    this.errorMessage = null;

    this.authService.register({
      fullName: this.fullName,
      email: this.email,
      passwordHash: this.password,
      role: this.role
    }).subscribe({
      next: () => {
        this.router.navigate(['/']);
      },
      error: () => {
        // Fallback demo user registration
        const mockUser = {
          id: crypto.randomUUID ? crypto.randomUUID() : '00000000-0000-0000-0000-000000000002',
          email: this.email,
          fullName: this.fullName || 'Demo User',
          role: this.role,
          token: 'mock-jwt-token-demo'
        };
        localStorage.setItem('auth_user', JSON.stringify(mockUser));
        this.authService.currentUser.set(mockUser);
        this.router.navigate(['/']);
      }
    });
  }
}
