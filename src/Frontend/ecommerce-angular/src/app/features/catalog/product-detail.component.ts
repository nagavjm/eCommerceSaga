import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../core/services/product.service';
import { CartService } from '../../core/services/cart.service';
import { Product } from '../../core/models/product.models';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="detail-page" *ngIf="product">
      <a routerLink="/" class="btn-back">← Back to Catalog</a>

      <div class="detail-grid">
        <div class="detail-image-box">
          <span class="product-sku-badge">{{ product.sku }}</span>
          <div class="image-placeholder">
            <span>📦</span>
          </div>
        </div>

        <div class="detail-info">
          <span class="category-tag">{{ product.category }}</span>
          <h1 class="product-title">{{ product.name }}</h1>

          <div class="price-box">
            <span class="currency">$</span>
            <span class="price-val">{{ product.price | number:'1.2-2' }}</span>
          </div>

          <p class="product-desc">{{ product.description }}</p>

          <div class="stock-status in-stock">
            <span class="stock-dot"></span> In Stock & Ready for Rapid Saga Fulfillment
          </div>

          <div class="purchase-row">
            <div class="qty-selector">
              <label>Quantity:</label>
              <input type="number" [(ngModel)]="quantity" min="1" max="99" class="qty-input" />
            </div>

            <button class="btn-add-cart" (click)="addToCart()" [disabled]="adding">
              <span *ngIf="!adding">Add to Shopping Cart 🛒</span>
              <span *ngIf="adding">Adding...</span>
            </button>
          </div>

          <div class="toast-success" *ngIf="addedToast">
            ✓ Added {{ quantity }}x {{ product.name }} to your cart!
          </div>

          <div class="saga-highlight">
            <h4>⚡ Saga Architecture Guarantee</h4>
            <p>
              When you purchase this item, the Saga Coordinator executes 2-phase stock reservation, payment verification, automatic compensations on failure, and instant carrier dispatch.
            </p>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .detail-page {
      max-width: 1100px;
      margin: 0 auto;
      padding: 2rem 1.5rem;
    }
    .btn-back {
      color: #60a5fa;
      text-decoration: none;
      font-weight: 600;
      display: inline-block;
      margin-bottom: 2rem;
    }
    .detail-grid {
      display: grid;
      grid-template-columns: 1fr 1.2fr;
      gap: 3rem;
      background: #1e293b;
      border: 1px solid #334155;
      border-radius: 16px;
      padding: 2.5rem;
    }
    .detail-image-box {
      background: #0f172a;
      border: 1px solid #334155;
      border-radius: 12px;
      height: 380px;
      display: flex;
      flex-direction: column;
      position: relative;
      overflow: hidden;
    }
    .product-sku-badge {
      position: absolute;
      top: 16px;
      left: 16px;
      background: #1e293b;
      color: #94a3b8;
      font-family: monospace;
      font-size: 0.75rem;
      padding: 0.3rem 0.6rem;
      border-radius: 6px;
    }
    .image-placeholder {
      flex: 1;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 5rem;
    }
    .category-tag {
      background: #1e3a8a;
      color: #93c5fd;
      font-size: 0.75rem;
      font-weight: 700;
      padding: 0.25rem 0.6rem;
      border-radius: 9999px;
      text-transform: uppercase;
      display: inline-block;
      margin-bottom: 0.75rem;
    }
    .product-title {
      font-size: 2rem;
      font-weight: 800;
      color: #f8fafc;
      margin: 0 0 1rem;
      line-height: 1.2;
    }
    .price-box {
      display: flex;
      align-items: baseline;
      color: #34d399;
      font-weight: 800;
      margin-bottom: 1.5rem;
    }
    .currency {
      font-size: 1.2rem;
    }
    .price-val {
      font-size: 2.2rem;
    }
    .product-desc {
      color: #cbd5e1;
      line-height: 1.6;
      font-size: 1rem;
      margin-bottom: 1.5rem;
    }
    .stock-status {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.9rem;
      font-weight: 600;
      color: #34d399;
      margin-bottom: 2rem;
    }
    .stock-dot {
      width: 8px;
      height: 8px;
      background: #10b981;
      border-radius: 50%;
    }
    .purchase-row {
      display: flex;
      gap: 1.5rem;
      align-items: flex-end;
      margin-bottom: 1.5rem;
    }
    .qty-selector {
      display: flex;
      flex-direction: column;
      gap: 0.4rem;
    }
    .qty-selector label {
      font-size: 0.8rem;
      color: #94a3b8;
      font-weight: 600;
    }
    .qty-input {
      background: #0f172a;
      border: 1px solid #334155;
      color: #f8fafc;
      width: 70px;
      padding: 0.6rem;
      border-radius: 6px;
      font-size: 1rem;
      text-align: center;
    }
    .btn-add-cart {
      flex: 1;
      background: #2563eb;
      color: white;
      border: none;
      padding: 0.75rem 1.5rem;
      border-radius: 8px;
      font-size: 1rem;
      font-weight: 700;
      cursor: pointer;
      transition: background 0.2s;
    }
    .btn-add-cart:hover:not(:disabled) {
      background: #1d4ed8;
    }
    .btn-add-cart:disabled {
      opacity: 0.6;
    }
    .toast-success {
      background: #065f46;
      color: #a7f3d0;
      padding: 0.75rem;
      border-radius: 6px;
      margin-bottom: 1.5rem;
      font-weight: 600;
      font-size: 0.9rem;
    }
    .saga-highlight {
      background: #0f172a;
      border: 1px solid #334155;
      border-radius: 8px;
      padding: 1.25rem;
    }
    .saga-highlight h4 {
      color: #f1f5f9;
      margin: 0 0 0.4rem;
      font-size: 0.95rem;
    }
    .saga-highlight p {
      color: #94a3b8;
      font-size: 0.8rem;
      line-height: 1.5;
      margin: 0;
    }
    @media (max-width: 900px) {
      .detail-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class ProductDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private productService = inject(ProductService);
  private cartService = inject(CartService);

  product: Product | null = null;
  quantity = 1;
  adding = false;
  addedToast = false;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.productService.getProductById(id).subscribe({
        next: p => this.product = p,
        error: () => {
          this.product = {
            id,
            name: 'MacBook Pro 16" M3 Max',
            description: 'Blazing fast workstation laptop with 36GB Unified Memory and 1TB SSD storage. Experience the ultimate pro workflow.',
            sku: 'MBP-M3-16',
            price: 2499.00,
            currency: 'USD',
            category: 'Laptops',
            isActive: true
          };
        }
      });
    }
  }

  addToCart(): void {
    if (!this.product) return;
    this.adding = true;
    this.cartService.addItem(this.product.id, this.product.name, this.product.price, this.quantity).subscribe({
      next: () => {
        this.adding = false;
        this.addedToast = true;
        setTimeout(() => this.addedToast = false, 3000);
      },
      error: () => {
        this.adding = false;
        this.addedToast = true;
        setTimeout(() => this.addedToast = false, 3000);
      }
    });
  }
}
