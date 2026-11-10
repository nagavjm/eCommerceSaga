import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet],
  template: `
    <div class="admin-page">
      <div class="admin-header">
        <div class="header-titles">
          <span class="admin-badge">Operations Portal</span>
          <h1 class="page-title">Admin Management Console</h1>
        </div>
      </div>

      <!-- Navigation Tabs -->
      <nav class="admin-tabs">
        <a routerLink="/admin" [routerLinkActiveOptions]="{exact: true}" routerLinkActive="active" class="tab-link">
          📊 Overview
        </a>
        <a routerLink="/admin/products" routerLinkActive="active" class="tab-link">
          🏷️ Products
        </a>
        <a routerLink="/admin/inventory" routerLinkActive="active" class="tab-link">
          📦 Inventory & Stock
        </a>
        <a routerLink="/admin/saga-monitor" routerLinkActive="active" class="tab-link saga-tab">
          ⚡ Saga Monitor
        </a>
      </nav>

      <!-- Sub-view content -->
      <div class="tab-content">
        <router-outlet></router-outlet>
      </div>
    </div>
  `,
  styles: [`
    .admin-page {
      max-width: 1280px;
      margin: 0 auto;
      padding: 2rem 1.5rem;
    }
    .admin-header {
      margin-bottom: 2rem;
    }
    .admin-badge {
      background: #78350f;
      color: #fef3c7;
      font-size: 0.75rem;
      font-weight: 700;
      padding: 0.25rem 0.6rem;
      border-radius: 9999px;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
    .page-title {
      font-size: 1.8rem;
      font-weight: 800;
      color: #f8fafc;
      margin: 0.5rem 0 0;
    }
    .admin-tabs {
      display: flex;
      gap: 0.5rem;
      border-bottom: 1px solid #334155;
      padding-bottom: 0.5rem;
      margin-bottom: 2rem;
    }
    .tab-link {
      padding: 0.6rem 1.2rem;
      border-radius: 8px;
      text-decoration: none;
      color: #94a3b8;
      font-weight: 600;
      font-size: 0.95rem;
      transition: all 0.2s;
    }
    .tab-link:hover {
      color: #f8fafc;
      background: #1e293b;
    }
    .tab-link.active {
      color: #60a5fa;
      background: #1e3a8a33;
      border: 1px solid #3b82f655;
    }
    .saga-tab.active {
      color: #f59e0b;
      background: #78350f33;
      border-color: #f59e0b55;
    }
    .tab-content {
      min-height: 400px;
    }
  `]
})
export class AdminDashboardComponent {}
