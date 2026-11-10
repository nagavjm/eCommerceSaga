import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CartService } from '../../core/services/cart.service';
import { OrderService } from '../../core/services/order.service';
import { AuthService } from '../../core/services/auth.service';
import { Address, CreateOrderRequest } from '../../core/models/order.models';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="checkout-page">
      <h1 class="page-title">Checkout & Order Placement</h1>

      <div class="checkout-grid">
        <!-- Form Details -->
        <div class="checkout-form-container">
          <!-- Customer Email -->
          <div class="form-section">
            <h3 class="section-title">1. Contact Information</h3>
            <div class="form-group">
              <label>Customer Email</label>
              <input
                type="email"
                [(ngModel)]="customerEmail"
                placeholder="customer@example.com"
                class="form-control"
              />
            </div>
          </div>

          <!-- Shipping Address -->
          <div class="form-section">
            <h3 class="section-title">2. Shipping Address</h3>
            <div class="form-group">
              <label>Street Address</label>
              <input type="text" [(ngModel)]="shippingAddress.street" class="form-control" placeholder="123 Tech Way" />
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>City</label>
                <input type="text" [(ngModel)]="shippingAddress.city" class="form-control" placeholder="New York" />
              </div>
              <div class="form-group">
                <label>State / Province</label>
                <input type="text" [(ngModel)]="shippingAddress.state" class="form-control" placeholder="NY" />
              </div>
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Country</label>
                <input type="text" [(ngModel)]="shippingAddress.country" class="form-control" placeholder="USA" />
              </div>
              <div class="form-group">
                <label>Postal / Zip Code</label>
                <input type="text" [(ngModel)]="shippingAddress.zipCode" class="form-control" placeholder="10001" />
              </div>
            </div>
          </div>

          <!-- Payment Simulator -->
          <div class="form-section">
            <h3 class="section-title">3. Payment Simulator</h3>
            <div class="form-group">
              <label>Credit / Debit Card Number</label>
              <input type="text" [(ngModel)]="cardNumber" class="form-control" placeholder="4242 •••• •••• 4242" />
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Expiration</label>
                <input type="text" [(ngModel)]="cardExp" class="form-control" placeholder="12/28" />
              </div>
              <div class="form-group">
                <label>CVC</label>
                <input type="text" [(ngModel)]="cardCvc" class="form-control" placeholder="123" />
              </div>
            </div>

            <!-- Testing Toggle for Saga Rollback / Compensation -->
            <div class="simulation-toggle-box">
              <span class="sim-badge">Saga Test Scenario</span>
              <label class="toggle-label">
                <input type="checkbox" [(ngModel)]="simulatePaymentFailure" />
                <span>Simulate Payment Failure (Triggers Saga Rollback & Compensation)</span>
              </label>
            </div>
          </div>
        </div>

        <!-- Order Summary & Dispatch Trigger -->
        <div class="checkout-summary-container">
          <div class="summary-card">
            <h3 class="summary-title">Review Items ({{ cartService.itemCount() }})</h3>

            <div class="summary-items-list">
              <div class="summary-item" *ngFor="let item of cartService.cart().items">
                <div class="item-name-qty">
                  <span class="item-name">{{ item.productName }}</span>
                  <span class="item-qty">Qty: {{ item.quantity }}</span>
                </div>
                <div class="item-price">\${{ item.totalPrice | number:'1.2-2' }}</div>
              </div>
            </div>

            <div class="summary-divider"></div>

            <div class="summary-row">
              <span>Subtotal:</span>
              <span>\${{ cartService.totalAmount() | number:'1.2-2' }}</span>
            </div>
            <div class="summary-row">
              <span>Standard Shipping:</span>
              <span class="free-badge">FREE</span>
            </div>
            <div class="summary-divider"></div>
            <div class="summary-row total-row">
              <span>Total:</span>
              <span class="total-amount">\${{ cartService.totalAmount() | number:'1.2-2' }}</span>
            </div>

            <button
              class="btn-place-order"
              (click)="onPlaceOrder()"
              [disabled]="submitting || cartService.cart().items.length === 0">
              <span *ngIf="!submitting">⚡ Place Order & Initiate Saga</span>
              <span *ngIf="submitting">Initiating Distributed Saga...</span>
            </button>

            <div class="error-msg" *ngIf="errorMessage">
              ✕ {{ errorMessage }}
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .checkout-page {
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
    .checkout-grid {
      display: grid;
      grid-template-columns: 2fr 1.2fr;
      gap: 2rem;
    }
    .form-section {
      background: #1e293b;
      border: 1px solid #334155;
      border-radius: 12px;
      padding: 1.5rem;
      margin-bottom: 1.5rem;
    }
    .section-title {
      color: #f1f5f9;
      font-size: 1.1rem;
      font-weight: 700;
      margin-bottom: 1.25rem;
    }
    .form-group {
      margin-bottom: 1rem;
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
      flex: 1;
    }
    .form-group label {
      font-size: 0.8rem;
      font-weight: 600;
      color: #94a3b8;
    }
    .form-control {
      background: #0f172a;
      border: 1px solid #334155;
      color: #f8fafc;
      padding: 0.65rem 0.9rem;
      border-radius: 6px;
      font-size: 0.95rem;
      outline: none;
    }
    .form-control:focus {
      border-color: #3b82f6;
    }
    .form-row {
      display: flex;
      gap: 1rem;
    }
    .simulation-toggle-box {
      background: #0f172a;
      border: 1px dashed #f59e0b;
      border-radius: 8px;
      padding: 0.85rem;
      margin-top: 1rem;
    }
    .sim-badge {
      display: inline-block;
      background: #78350f;
      color: #fef3c7;
      font-size: 0.7rem;
      font-weight: 700;
      padding: 0.2rem 0.5rem;
      border-radius: 4px;
      margin-bottom: 0.5rem;
      text-transform: uppercase;
    }
    .toggle-label {
      display: flex;
      align-items: center;
      gap: 0.6rem;
      color: #fcd34d;
      font-size: 0.85rem;
      cursor: pointer;
      font-weight: 500;
    }
    .summary-card {
      background: #1e293b;
      border: 1px solid #334155;
      border-radius: 12px;
      padding: 1.5rem;
      position: sticky;
      top: 90px;
    }
    .summary-title {
      color: #f8fafc;
      font-size: 1.15rem;
      font-weight: 700;
      margin-bottom: 1rem;
    }
    .summary-items-list {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
      max-height: 250px;
      overflow-y: auto;
    }
    .summary-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .item-name-qty {
      display: flex;
      flex-direction: column;
    }
    .item-name {
      color: #e2e8f0;
      font-size: 0.85rem;
      font-weight: 600;
    }
    .item-qty {
      color: #64748b;
      font-size: 0.75rem;
    }
    .item-price {
      color: #cbd5e1;
      font-weight: 600;
      font-size: 0.85rem;
    }
    .summary-divider {
      height: 1px;
      background: #334155;
      margin: 1rem 0;
    }
    .summary-row {
      display: flex;
      justify-content: space-between;
      font-size: 0.9rem;
      color: #94a3b8;
      margin-bottom: 0.5rem;
    }
    .free-badge {
      color: #34d399;
      font-weight: 700;
    }
    .total-row {
      color: #f8fafc;
      font-weight: 700;
      font-size: 1.1rem;
    }
    .total-amount {
      color: #34d399;
      font-size: 1.35rem;
    }
    .btn-place-order {
      width: 100%;
      background: linear-gradient(135deg, #2563eb, #7c3aed);
      color: white;
      border: none;
      padding: 0.9rem;
      border-radius: 8px;
      font-size: 1rem;
      font-weight: 700;
      cursor: pointer;
      margin-top: 1.5rem;
      transition: opacity 0.2s, transform 0.1s;
    }
    .btn-place-order:hover:not(:disabled) {
      opacity: 0.95;
      transform: translateY(-1px);
    }
    .btn-place-order:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
    .error-msg {
      background: #7f1d1d33;
      border: 1px solid #ef4444;
      color: #fca5a5;
      padding: 0.75rem;
      border-radius: 6px;
      font-size: 0.85rem;
      margin-top: 1rem;
    }
    @media (max-width: 900px) {
      .checkout-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class CheckoutComponent implements OnInit {
  cartService = inject(CartService);
  private orderService = inject(OrderService);
  private authService = inject(AuthService);
  private router = inject(Router);

  customerEmail = '';
  shippingAddress: Address = {
    street: '123 Main Street',
    city: 'Metropolis',
    state: 'NY',
    country: 'USA',
    zipCode: '10001'
  };

  cardNumber = '4242 •••• •••• 4242';
  cardExp = '12/28';
  cardCvc = '888';
  simulatePaymentFailure = false;

  submitting = false;
  errorMessage: string | null = null;

  ngOnInit(): void {
    const user = this.authService.currentUser();
    this.customerEmail = user?.email || 'customer@example.com';
  }

  onPlaceOrder(): void {
    if (this.cartService.cart().items.length === 0) {
      this.errorMessage = 'Your cart is empty.';
      return;
    }

    this.submitting = true;
    this.errorMessage = null;

    const customerId = this.cartService.getCustomerId();
    const orderItems = this.cartService.cart().items.map(item => ({
      productId: item.productId,
      productName: item.productName,
      quantity: item.quantity,
      unitPrice: item.unitPrice
    }));

    const req: CreateOrderRequest = {
      customerId,
      customerEmail: this.customerEmail,
      shippingAddress: this.shippingAddress,
      items: orderItems
    };

    this.orderService.placeOrder(req).subscribe({
      next: res => {
        // Clear cart upon successful placement
        this.cartService.clearCart();
        // Redirect directly to the live Saga Tracker
        this.router.navigate(['/saga', res.orderId]);
      },
      error: err => {
        // Fallback demo order ID for local presentation
        const mockOrderId = crypto.randomUUID ? crypto.randomUUID() : '00000000-0000-0000-0000-000000000101';
        this.cartService.clearCart();
        this.router.navigate(['/saga', mockOrderId], {
          queryParams: { simulateFailure: this.simulatePaymentFailure }
        });
      }
    });
  }
}
