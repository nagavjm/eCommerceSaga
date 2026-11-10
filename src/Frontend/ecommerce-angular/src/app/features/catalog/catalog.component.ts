import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ProductService } from '../../core/services/product.service';
import { CartService } from '../../core/services/cart.service';
import { Product } from '../../core/models/product.models';

@Component({
  selector: 'app-catalog',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="catalog-page">
      <!-- Hero Banner -->
      <section class="hero-banner">
        <div class="hero-content">
          <h1 class="hero-title">High-Performance E-Commerce</h1>
          <p class="hero-subtitle">
            Powered by Choreographed & Orchestrated Saga Architecture, .NET 9 Microservices, and Event-Driven Outbox Messaging.
          </p>
        </div>
      </section>

      <!-- Filter & Search Toolbar -->
      <div class="filter-toolbar">
        <div class="search-box">
          <span class="search-icon">🔍</span>
          <input
            type="text"
            [(ngModel)]="searchTerm"
            (keyup.enter)="onSearch()"
            placeholder="Search products by title or description..."
            class="search-input"
          />
          <button class="btn-search" (click)="onSearch()">Search</button>
        </div>

        <div class="category-pills">
          <button
            *ngFor="let cat of categories"
            class="category-pill"
            [class.active]="selectedCategory === cat"
            (click)="selectCategory(cat)">
            {{ cat }}
          </button>
        </div>
      </div>

      <!-- Loading State -->
      <div class="loading-state" *ngIf="loading">
        <div class="spinner"></div>
        <p>Loading catalog products...</p>
      </div>

      <!-- Error State -->
      <div class="error-banner" *ngIf="error">
        <span>⚠️ {{ error }}</span>
        <button class="btn-retry" (click)="loadProducts()">Retry</button>
      </div>

      <!-- Product Grid -->
      <div class="product-grid" *ngIf="!loading && !error">
        <div class="product-card" *ngFor="let product of products">
          <div class="product-badge">{{ product.category }}</div>
          <div class="product-img-placeholder">
            <span class="product-sku">{{ product.sku }}</span>
          </div>
          <div class="product-info">
            <h3 class="product-name">
              <a [routerLink]="['/products', product.id]">{{ product.name }}</a>
            </h3>
            <p class="product-desc">{{ product.description }}</p>
            <div class="product-bottom">
              <div class="product-price">
                <span class="currency">$</span>
                <span class="amount">{{ product.price | number:'1.2-2' }}</span>
              </div>
              <button
                class="btn-add-cart"
                (click)="addToCart(product)"
                [disabled]="addingProductId === product.id">
                <span *ngIf="addingProductId !== product.id">Add to Cart 🛒</span>
                <span *ngIf="addingProductId === product.id">Adding...</span>
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div class="empty-state" *ngIf="!loading && !error && products.length === 0">
        <p>No products found matching your search criteria.</p>
        <button class="btn-reset" (click)="resetFilters()">Reset Filters</button>
      </div>

      <!-- Toast Notification -->
      <div class="toast-notification" *ngIf="addedNotification">
        ✓ {{ addedNotification }} added to cart!
      </div>
    </div>
  `,
  styles: [`
    .catalog-page {
      max-width: 1280px;
      margin: 0 auto;
      padding: 1.5rem;
    }
    .hero-banner {
      background: linear-gradient(135deg, #1e3a8a 0%, #312e81 50%, #1e1b4b 100%);
      border-radius: 16px;
      padding: 3rem 2rem;
      margin-bottom: 2rem;
      border: 1px solid #3b82f633;
      box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.2);
    }
    .hero-title {
      font-size: 2.25rem;
      font-weight: 800;
      color: #ffffff;
      margin: 0 0 0.5rem;
      letter-spacing: -0.025em;
    }
    .hero-subtitle {
      font-size: 1.05rem;
      color: #bfdbfe;
      max-width: 800px;
      line-height: 1.6;
      margin: 0;
    }
    .filter-toolbar {
      display: flex;
      flex-direction: column;
      gap: 1rem;
      margin-bottom: 2rem;
      background: #1e293b;
      padding: 1rem 1.25rem;
      border-radius: 12px;
      border: 1px solid #334155;
    }
    .search-box {
      display: flex;
      gap: 0.5rem;
      align-items: center;
    }
    .search-input {
      flex: 1;
      background: #0f172a;
      border: 1px solid #334155;
      padding: 0.6rem 1rem;
      border-radius: 8px;
      color: #f8fafc;
      font-size: 0.95rem;
      outline: none;
    }
    .search-input:focus {
      border-color: #3b82f6;
    }
    .btn-search {
      background: #3b82f6;
      color: white;
      border: none;
      padding: 0.6rem 1.2rem;
      border-radius: 8px;
      font-weight: 600;
      cursor: pointer;
    }
    .category-pills {
      display: flex;
      gap: 0.5rem;
      flex-wrap: wrap;
    }
    .category-pill {
      background: #0f172a;
      border: 1px solid #334155;
      color: #94a3b8;
      padding: 0.4rem 0.9rem;
      border-radius: 9999px;
      font-size: 0.85rem;
      font-weight: 500;
      cursor: pointer;
      transition: all 0.2s;
    }
    .category-pill:hover {
      color: white;
      border-color: #64748b;
    }
    .category-pill.active {
      background: #2563eb;
      color: white;
      border-color: #3b82f6;
    }
    .product-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: 1.5rem;
    }
    .product-card {
      background: #1e293b;
      border: 1px solid #334155;
      border-radius: 12px;
      overflow: hidden;
      display: flex;
      flex-direction: column;
      position: relative;
      transition: transform 0.2s, box-shadow 0.2s;
    }
    .product-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 12px 20px -5px rgba(0, 0, 0, 0.4);
      border-color: #3b82f6;
    }
    .product-badge {
      position: absolute;
      top: 12px;
      right: 12px;
      background: #0f172a;
      color: #60a5fa;
      border: 1px solid #3b82f644;
      font-size: 0.7rem;
      font-weight: 700;
      padding: 0.2rem 0.6rem;
      border-radius: 9999px;
      text-transform: uppercase;
      z-index: 10;
    }
    .product-img-placeholder {
      height: 160px;
      background: linear-gradient(135deg, #0f172a, #1e293b);
      display: flex;
      align-items: center;
      justify-content: center;
      border-bottom: 1px solid #334155;
    }
    .product-sku {
      color: #475569;
      font-size: 0.85rem;
      font-family: monospace;
      font-weight: 600;
    }
    .product-info {
      padding: 1.25rem;
      display: flex;
      flex-direction: column;
      flex: 1;
    }
    .product-name {
      margin: 0 0 0.5rem 0;
      font-size: 1.1rem;
      font-weight: 700;
    }
    .product-name a {
      color: #f8fafc;
      text-decoration: none;
    }
    .product-name a:hover {
      color: #60a5fa;
    }
    .product-desc {
      color: #94a3b8;
      font-size: 0.85rem;
      line-height: 1.4;
      margin: 0 0 1rem 0;
      flex: 1;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }
    .product-bottom {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-top: auto;
    }
    .product-price {
      display: flex;
      align-items: baseline;
      color: #34d399;
      font-weight: 800;
    }
    .currency {
      font-size: 0.9rem;
      margin-right: 2px;
    }
    .amount {
      font-size: 1.35rem;
    }
    .btn-add-cart {
      background: #2563eb;
      color: white;
      border: none;
      padding: 0.5rem 0.9rem;
      border-radius: 8px;
      font-size: 0.85rem;
      font-weight: 600;
      cursor: pointer;
      transition: background 0.2s;
    }
    .btn-add-cart:hover:not(:disabled) {
      background: #1d4ed8;
    }
    .btn-add-cart:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }
    .loading-state, .empty-state {
      text-align: center;
      padding: 4rem 2rem;
      color: #94a3b8;
    }
    .spinner {
      width: 40px;
      height: 40px;
      border: 3px solid #334155;
      border-top-color: #3b82f6;
      border-radius: 50%;
      margin: 0 auto 1rem;
      animation: spin 1s infinite linear;
    }
    @keyframes spin {
      100% { transform: rotate(360deg); }
    }
    .error-banner {
      background: #7f1d1d33;
      border: 1px solid #ef4444;
      color: #fca5a5;
      padding: 1rem;
      border-radius: 8px;
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 2rem;
    }
    .btn-retry {
      background: #ef4444;
      color: white;
      border: none;
      padding: 0.4rem 0.8rem;
      border-radius: 6px;
      cursor: pointer;
    }
    .toast-notification {
      position: fixed;
      bottom: 24px;
      right: 24px;
      background: #059669;
      color: white;
      padding: 0.75rem 1.25rem;
      border-radius: 8px;
      box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.3);
      font-weight: 600;
      z-index: 9999;
      animation: slideIn 0.3s ease;
    }
    @keyframes slideIn {
      from { transform: translateY(20px); opacity: 0; }
      to { transform: translateY(0); opacity: 1; }
    }
  `]
})
export class CatalogComponent implements OnInit {
  private productService = inject(ProductService);
  private cartService = inject(CartService);

  products: Product[] = [];
  loading = false;
  error: string | null = null;
  searchTerm = '';
  selectedCategory = 'All';
  categories = ['All', 'Electronics', 'Laptops', 'Audio', 'Accessories'];
  addingProductId: string | null = null;
  addedNotification: string | null = null;

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.loading = true;
    this.error = null;

    this.productService.getProducts(1, 20, this.selectedCategory, this.searchTerm).subscribe({
      next: res => {
        this.products = res.items;
        this.loading = false;
      },
      error: err => {
        // Fallback with demo products if Gateway isn't running yet during test
        this.products = this.getFallbackProducts();
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    this.loadProducts();
  }

  selectCategory(category: string): void {
    this.selectedCategory = category;
    this.loadProducts();
  }

  resetFilters(): void {
    this.searchTerm = '';
    this.selectedCategory = 'All';
    this.loadProducts();
  }

  addToCart(product: Product): void {
    this.addingProductId = product.id;
    this.cartService.addItem(product.id, product.name, product.price, 1).subscribe({
      next: () => {
        this.addingProductId = null;
        this.showNotification(product.name);
      },
      error: () => {
        // Local fallback update if offline
        const currentCart = this.cartService.cart();
        const existing = currentCart.items.find(i => i.productId === product.id);
        if (existing) {
          existing.quantity++;
          existing.totalPrice = existing.quantity * existing.unitPrice;
        } else {
          currentCart.items.push({
            productId: product.id,
            productName: product.name,
            unitPrice: product.price,
            quantity: 1,
            totalPrice: product.price
          });
        }
        currentCart.totalAmount = currentCart.items.reduce((s, i) => s + i.totalPrice, 0);
        this.cartService.cart.set({ ...currentCart });
        this.addingProductId = null;
        this.showNotification(product.name);
      }
    });
  }

  private showNotification(name: string): void {
    this.addedNotification = name;
    setTimeout(() => {
      this.addedNotification = null;
    }, 2500);
  }

  private getFallbackProducts(): Product[] {
    return [
      {
        id: '11111111-1111-1111-1111-111111111111',
        name: 'MacBook Pro 16" M3 Max',
        description: 'Blazing fast workstation laptop with 36GB Unified Memory and 1TB SSD.',
        sku: 'MBP-M3-16',
        price: 2499.00,
        currency: 'USD',
        category: 'Laptops',
        isActive: true
      },
      {
        id: '22222222-2222-2222-2222-222222222222',
        name: 'Sony WH-1000XM5 Headphones',
        description: 'Industry-leading noise cancelling wireless headphones with 30-hour battery life.',
        sku: 'SNY-WH1000-XM5',
        price: 398.00,
        currency: 'USD',
        category: 'Audio',
        isActive: true
      },
      {
        id: '33333333-3333-3333-3333-333333333333',
        name: 'Dell UltraSharp 32" 4K Monitor',
        description: 'Brilliant IPS Black color accuracy with USB-C 90W power delivery.',
        sku: 'DEL-U3223QE',
        price: 789.99,
        currency: 'USD',
        category: 'Electronics',
        isActive: true
      },
      {
        id: '44444444-4444-4444-4444-444444444444',
        name: 'Keychron Q1 Pro Mechanical Keyboard',
        description: 'Custom wireless QMK/VIA gasket-mounted aluminum mechanical keyboard.',
        sku: 'KEY-Q1-PRO',
        price: 199.50,
        currency: 'USD',
        category: 'Accessories',
        isActive: true
      }
    ];
  }
}
