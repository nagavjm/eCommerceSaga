import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { OrderService } from '../../core/services/order.service';
import { AuthService } from '../../core/services/auth.service';
import { Order } from '../../core/models/order.models';

@Component({
  selector: 'app-order-history',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="orders-page">
      <div class="page-header">
        <h1 class="page-title">My Orders</h1>
        <p class="page-subtitle">Track the lifecycle and real-time saga state of all your placed orders.</p>
      </div>

      <div class="orders-list" *ngIf="orders.length > 0">
        <div class="order-card" *ngFor="let order of orders">
          <div class="order-header">
            <div class="order-meta">
              <span class="order-id">Order #{{ order.id }}</span>
              <span class="order-date">{{ order.createdAtUtc | date:'medium' }}</span>
            </div>
            <div class="order-badges">
              <span class="status-badge" [ngClass]="order.status.toLowerCase()">
                {{ order.status }}
              </span>
            </div>
          </div>

          <div class="order-body">
            <div class="items-summary">
              <div class="item-row" *ngFor="let item of order.items">
                <span class="item-qty">{{ item.quantity }}x</span>
                <span class="item-name">{{ item.productName }}</span>
                <span class="item-price">\${{ (item.unitPrice * item.quantity) | number:'1.2-2' }}</span>
              </div>
            </div>

            <div class="order-footer">
              <div class="total-box">
                <span class="total-label">Total Amount:</span>
                <span class="total-amount">\${{ order.totalAmount | number:'1.2-2' }}</span>
              </div>
              <a [routerLink]="['/saga', order.id]" class="btn-track-saga">
                ⚡ Track Saga Workflow
              </a>
            </div>
          </div>
        </div>
      </div>

      <div class="empty-orders" *ngIf="orders.length === 0">
        <div class="empty-icon">📦</div>
        <h2>No orders found</h2>
        <p>You haven't placed any orders yet. Visit our catalog to make your first purchase!</p>
        <a routerLink="/" class="btn-shop">Browse Products</a>
      </div>
    </div>
  `,
  styles: [`
    .orders-page {
      max-width: 1000px;
      margin: 0 auto;
      padding: 2rem 1.5rem;
    }
    .page-title {
      font-size: 1.8rem;
      font-weight: 800;
      color: #f8fafc;
      margin-bottom: 0.25rem;
    }
    .page-subtitle {
      color: #94a3b8;
      font-size: 0.95rem;
      margin-bottom: 2rem;
    }
    .orders-list {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }
    .order-card {
      background: #1e293b;
      border: 1px solid #334155;
      border-radius: 12px;
      overflow: hidden;
    }
    .order-header {
      background: #0f172a;
      padding: 1rem 1.5rem;
      border-bottom: 1px solid #334155;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .order-meta {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
    }
    .order-id {
      font-weight: 700;
      color: #f1f5f9;
      font-family: monospace;
      font-size: 0.95rem;
    }
    .order-date {
      color: #64748b;
      font-size: 0.8rem;
    }
    .status-badge {
      font-size: 0.75rem;
      font-weight: 700;
      padding: 0.3rem 0.75rem;
      border-radius: 9999px;
      text-transform: uppercase;
    }
    .status-badge.confirmed, .status-badge.completed {
      background: #065f46;
      color: #6ee7b7;
    }
    .status-badge.pending, .status-badge.stockreserved, .status-badge.paid {
      background: #1e3a8a;
      color: #93c5fd;
    }
    .status-badge.cancelled {
      background: #7f1d1d;
      color: #fca5a5;
    }
    .order-body {
      padding: 1.5rem;
    }
    .items-summary {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      margin-bottom: 1.5rem;
    }
    .item-row {
      display: flex;
      gap: 0.75rem;
      align-items: center;
      font-size: 0.9rem;
    }
    .item-qty {
      font-weight: 700;
      color: #60a5fa;
      min-width: 25px;
    }
    .item-name {
      color: #e2e8f0;
      flex: 1;
    }
    .item-price {
      color: #cbd5e1;
      font-weight: 600;
    }
    .order-footer {
      border-top: 1px solid #334155;
      padding-top: 1.25rem;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .total-box {
      display: flex;
      align-items: baseline;
      gap: 0.5rem;
    }
    .total-label {
      color: #94a3b8;
      font-size: 0.9rem;
    }
    .total-amount {
      color: #34d399;
      font-size: 1.25rem;
      font-weight: 800;
    }
    .btn-track-saga {
      background: #2563eb;
      color: white;
      text-decoration: none;
      font-weight: 600;
      font-size: 0.85rem;
      padding: 0.5rem 1rem;
      border-radius: 6px;
      transition: background 0.2s;
    }
    .btn-track-saga:hover {
      background: #1d4ed8;
    }
    .empty-orders {
      background: #1e293b;
      border: 1px solid #334155;
      border-radius: 12px;
      padding: 4rem 2rem;
      text-align: center;
      color: #94a3b8;
    }
    .empty-icon {
      font-size: 3rem;
      margin-bottom: 1rem;
    }
    .empty-orders h2 {
      color: #f8fafc;
      margin-bottom: 0.5rem;
    }
    .btn-shop {
      display: inline-block;
      margin-top: 1.5rem;
      background: #2563eb;
      color: white;
      padding: 0.75rem 1.5rem;
      border-radius: 8px;
      text-decoration: none;
      font-weight: 600;
    }
  `]
})
export class OrderHistoryComponent implements OnInit {
  private orderService = inject(OrderService);
  private authService = inject(AuthService);

  orders: Order[] = [];

  ngOnInit(): void {
    const customerId = this.authService.currentUser()?.id || '00000000-0000-0000-0000-000000000001';
    this.orderService.getCustomerOrders(customerId).subscribe({
      next: orders => {
        this.orders = orders;
      },
      error: () => {
        this.orders = this.getMockOrders();
      }
    });
  }

  private getMockOrders(): Order[] {
    return [
      {
        id: '99999999-9999-9999-9999-999999999991',
        customerId: '00000000-0000-0000-0000-000000000001',
        customerEmail: 'customer@example.com',
        status: 'Confirmed',
        totalAmount: 2499.00,
        shippingAddress: {
          street: '123 Main St',
          city: 'Metropolis',
          state: 'NY',
          country: 'USA',
          zipCode: '10001'
        },
        items: [
          {
            productId: '11111111-1111-1111-1111-111111111111',
            productName: 'MacBook Pro 16" M3 Max',
            quantity: 1,
            unitPrice: 2499.00,
            totalPrice: 2499.00
          }
        ],
        correlationId: '88888888-8888-8888-8888-888888888881',
        createdAtUtc: new Date(Date.now() - 3600000 * 2).toISOString()
      }
    ];
  }
}
