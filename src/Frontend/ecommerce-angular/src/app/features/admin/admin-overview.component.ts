import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-admin-overview',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="overview-grid">
      <!-- Stat Card 1 -->
      <div class="stat-card">
        <div class="stat-header">
          <span class="stat-label">Total Catalog Products</span>
          <span class="stat-icon">🏷️</span>
        </div>
        <div class="stat-value">4 Active</div>
        <a routerLink="/admin/products" class="stat-link">Manage Products →</a>
      </div>

      <!-- Stat Card 2 -->
      <div class="stat-card">
        <div class="stat-header">
          <span class="stat-label">Inventory Health</span>
          <span class="stat-icon">📦</span>
        </div>
        <div class="stat-value">100% In Stock</div>
        <a routerLink="/admin/inventory" class="stat-link">Adjust Stock Levels →</a>
      </div>

      <!-- Stat Card 3 -->
      <div class="stat-card">
        <div class="stat-header">
          <span class="stat-label">Saga Orchestrations</span>
          <span class="stat-icon">⚡</span>
        </div>
        <div class="stat-value">Distributed Engine Active</div>
        <a routerLink="/admin/saga-monitor" class="stat-link">Inspect Sagas →</a>
      </div>

      <!-- Architecture Card -->
      <div class="arch-card">
        <h3>Distributed System Architecture</h3>
        <p>
          Each bounded context runs independently with its own database, Transactional Outbox, and Inbox deduplication tables.
        </p>
        <div class="service-pills">
          <span class="srv-pill">Identity (Port 5001)</span>
          <span class="srv-pill">Product (Port 5008)</span>
          <span class="srv-pill">Cart (Port 5009)</span>
          <span class="srv-pill">Inventory (Port 5002)</span>
          <span class="srv-pill">Order (Port 5003)</span>
          <span class="srv-pill">Payment (Port 5004)</span>
          <span class="srv-pill">Shipping (Port 5005)</span>
          <span class="srv-pill">Notification (Port 5006)</span>
          <span class="srv-pill saga">Saga Orchestrator (Port 5007)</span>
          <span class="srv-pill gw">Ocelot Gateway (Port 5000)</span>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .overview-grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 1.5rem;
    }
    .stat-card {
      background: #1e293b;
      border: 1px solid #334155;
      border-radius: 12px;
      padding: 1.5rem;
      display: flex;
      flex-direction: column;
    }
    .stat-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 0.75rem;
    }
    .stat-label {
      color: #94a3b8;
      font-size: 0.85rem;
      font-weight: 600;
    }
    .stat-icon {
      font-size: 1.3rem;
    }
    .stat-value {
      font-size: 1.5rem;
      font-weight: 800;
      color: #f8fafc;
      margin-bottom: 1rem;
    }
    .stat-link {
      margin-top: auto;
      color: #60a5fa;
      text-decoration: none;
      font-size: 0.85rem;
      font-weight: 600;
    }
    .stat-link:hover {
      text-decoration: underline;
    }
    .arch-card {
      grid-column: span 3;
      background: #0f172a;
      border: 1px solid #334155;
      border-radius: 12px;
      padding: 1.5rem;
      margin-top: 1rem;
    }
    .arch-card h3 {
      color: #f1f5f9;
      font-size: 1.15rem;
      margin: 0 0 0.5rem;
    }
    .arch-card p {
      color: #94a3b8;
      font-size: 0.85rem;
      margin: 0 0 1.25rem;
    }
    .service-pills {
      display: flex;
      flex-wrap: wrap;
      gap: 0.6rem;
    }
    .srv-pill {
      background: #1e293b;
      border: 1px solid #334155;
      color: #cbd5e1;
      padding: 0.35rem 0.75rem;
      border-radius: 6px;
      font-size: 0.8rem;
      font-weight: 500;
    }
    .srv-pill.saga {
      background: #78350f33;
      border-color: #f59e0b;
      color: #fcd34d;
      font-weight: 700;
    }
    .srv-pill.gw {
      background: #1e3a8a33;
      border-color: #3b82f6;
      color: #93c5fd;
      font-weight: 700;
    }
    @media (max-width: 900px) {
      .overview-grid {
        grid-template-columns: 1fr;
      }
      .arch-card {
        grid-column: span 1;
      }
    }
  `]
})
export class AdminOverviewComponent {}
