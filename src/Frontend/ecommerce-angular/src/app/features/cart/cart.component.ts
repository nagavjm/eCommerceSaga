import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CartService } from '../../core/services/cart.service';
import { CartItem } from '../../core/models/cart.models';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="cart-page">
      <h1 class="page-title">Shopping Cart</h1>

      <!-- Empty Cart View -->
      <div class="empty-cart-card" *ngIf="cartService.cart().items.length === 0">
        <div class="empty-icon">🛒</div>
        <h2>Your cart is currently empty</h2>
        <p>Explore our high-performance tech catalog and add items to your cart.</p>
        <a routerLink="/" class="btn-shop">Explore Catalog</a>
      </div>

      <!-- Active Cart View -->
      <div class="cart-layout" *ngIf="cartService.cart().items.length > 0">
        <!-- Items Table -->
        <div class="cart-items-card">
          <div class="items-header">
            <span>Product</span>
            <span>Unit Price</span>
            <span>Quantity</span>
            <span>Total</span>
            <span></span>
          </div>

          <div class="cart-item-row" *ngFor="let item of cartService.cart().items">
            <div class="item-name-col">
              <span class="item-name">{{ item.productName }}</span>
              <span class="item-id">ID: {{ item.productId }}</span>
            </div>
            <div class="item-price">\${{ item.unitPrice | number:'1.2-2' }}</div>
            <div class="item-qty">
              <button class="btn-qty" (click)="decreaseQty(item)" [disabled]="item.quantity <= 1">-</button>
              <span class="qty-val">{{ item.quantity }}</span>
              <button class="btn-qty" (click)="increaseQty(item)">+</button>
            </div>
            <div class="item-total">\${{ item.totalPrice | number:'1.2-2' }}</div>
            <div class="item-actions">
              <button class="btn-remove" (click)="removeItem(item.productId)" title="Remove item">✕</button>
            </div>
          </div>

          <div class="cart-footer-actions">
            <button class="btn-clear" (click)="clearCart()">Clear Cart</button>
            <a routerLink="/" class="btn-continue">← Continue Shopping</a>
          </div>
        </div>

        <!-- Order Summary Card -->
        <div class="summary-card">
          <h3 class="summary-title">Order Summary</h3>
          <div class="summary-row">
            <span>Items ({{ cartService.itemCount() }}):</span>
            <span>\${{ cartService.totalAmount() | number:'1.2-2' }}</span>
          </div>
          <div class="summary-row">
            <span>Estimated Shipping:</span>
            <span class="free-text">FREE</span>
          </div>
          <div class="summary-row">
            <span>Estimated Tax:</span>
            <span>$0.00</span>
          </div>
          <div class="summary-divider"></div>
          <div class="summary-row total-row">
            <span>Total:</span>
            <span class="total-amount">\${{ cartService.totalAmount() | number:'1.2-2' }}</span>
          </div>

          <a routerLink="/checkout" class="btn-checkout">
            Proceed to Checkout →
          </a>

          <div class="saga-note">
            ⚡ <strong>Saga Architecture Notice:</strong> Checking out initiates a distributed 6-step Saga across Order, Inventory, Payment, Shipping, and Notification microservices.
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .cart-page {
      max-width: 1280px;
      margin: 0 auto;
      padding: 2rem 1.5rem;
    }
    .page-title {
      font-size: 1.8rem;
      font-weight: 800;
      color: #f8fafc;
      margin-bottom: 2rem;
    }
    .empty-cart-card {
      background: #1e293b;
      border: 1px solid #334155;
      border-radius: 12px;
      padding: 4rem 2rem;
      text-align: center;
      color: #94a3b8;
    }
    .empty-icon {
      font-size: 3.5rem;
      margin-bottom: 1rem;
    }
    .empty-cart-card h2 {
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
      transition: background 0.2s;
    }
    .btn-shop:hover {
      background: #1d4ed8;
    }
    .cart-layout {
      display: grid;
      grid-template-columns: 2fr 1fr;
      gap: 2rem;
    }
    .cart-items-card {
      background: #1e293b;
      border: 1px solid #334155;
      border-radius: 12px;
      padding: 1.5rem;
    }
    .items-header {
      display: grid;
      grid-template-columns: 3fr 1fr 1.5fr 1fr 0.5fr;
      padding-bottom: 1rem;
      border-bottom: 1px solid #334155;
      font-size: 0.8rem;
      font-weight: 700;
      color: #94a3b8;
      text-transform: uppercase;
    }
    .cart-item-row {
      display: grid;
      grid-template-columns: 3fr 1fr 1.5fr 1fr 0.5fr;
      align-items: center;
      padding: 1.25rem 0;
      border-bottom: 1px solid #334155;
    }
    .item-name-col {
      display: flex;
      flex-direction: column;
    }
    .item-name {
      font-weight: 600;
      color: #f8fafc;
      font-size: 0.95rem;
    }
    .item-id {
      font-size: 0.7rem;
      color: #64748b;
      font-family: monospace;
    }
    .item-price, .item-total {
      color: #cbd5e1;
      font-weight: 600;
      font-size: 0.95rem;
    }
    .item-total {
      color: #34d399;
    }
    .item-qty {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }
    .btn-qty {
      background: #0f172a;
      border: 1px solid #334155;
      color: #e2e8f0;
      width: 28px;
      height: 28px;
      border-radius: 4px;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 700;
    }
    .btn-qty:disabled {
      opacity: 0.4;
      cursor: not-allowed;
    }
    .qty-val {
      font-size: 0.9rem;
      font-weight: 600;
      color: #f8fafc;
      min-width: 20px;
      text-align: center;
    }
    .btn-remove {
      background: transparent;
      border: none;
      color: #ef4444;
      cursor: pointer;
      font-size: 1rem;
      padding: 0.25rem 0.5rem;
    }
    .btn-remove:hover {
      color: #dc2626;
    }
    .cart-footer-actions {
      display: flex;
      justify-content: space-between;
      margin-top: 1.5rem;
    }
    .btn-clear {
      background: transparent;
      border: 1px solid #475569;
      color: #94a3b8;
      padding: 0.5rem 1rem;
      border-radius: 6px;
      cursor: pointer;
    }
    .btn-clear:hover {
      background: #dc262622;
      border-color: #ef4444;
      color: #fca5a5;
    }
    .btn-continue {
      color: #60a5fa;
      text-decoration: none;
      font-weight: 500;
      font-size: 0.9rem;
      align-self: center;
    }
    .summary-card {
      background: #1e293b;
      border: 1px solid #334155;
      border-radius: 12px;
      padding: 1.5rem;
      height: fit-content;
    }
    .summary-title {
      font-size: 1.2rem;
      color: #f8fafc;
      margin-bottom: 1.25rem;
      font-weight: 700;
    }
    .summary-row {
      display: flex;
      justify-content: space-between;
      margin-bottom: 0.75rem;
      font-size: 0.9rem;
      color: #94a3b8;
    }
    .free-text {
      color: #34d399;
      font-weight: 700;
    }
    .summary-divider {
      height: 1px;
      background: #334155;
      margin: 1rem 0;
    }
    .total-row {
      font-size: 1.15rem;
      font-weight: 700;
      color: #f8fafc;
    }
    .total-amount {
      color: #34d399;
      font-size: 1.35rem;
    }
    .btn-checkout {
      display: block;
      width: 100%;
      text-align: center;
      background: #2563eb;
      color: white;
      padding: 0.85rem;
      border-radius: 8px;
      font-size: 1rem;
      font-weight: 700;
      text-decoration: none;
      margin-top: 1.5rem;
      transition: background 0.2s;
      box-sizing: border-box;
    }
    .btn-checkout:hover {
      background: #1d4ed8;
    }
    .saga-note {
      margin-top: 1.5rem;
      font-size: 0.75rem;
      color: #94a3b8;
      background: #0f172a;
      border: 1px solid #334155;
      padding: 0.75rem;
      border-radius: 8px;
      line-height: 1.4;
    }
    @media (max-width: 900px) {
      .cart-layout {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class CartComponent {
  cartService = inject(CartService);

  increaseQty(item: CartItem): void {
    this.cartService.updateQuantity(item.productId, item.quantity + 1).subscribe();
  }

  decreaseQty(item: CartItem): void {
    if (item.quantity > 1) {
      this.cartService.updateQuantity(item.productId, item.quantity - 1).subscribe();
    }
  }

  removeItem(productId: string): void {
    this.cartService.removeItem(productId).subscribe();
  }

  clearCart(): void {
    this.cartService.clearCart().subscribe();
  }
}
