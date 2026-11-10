import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../core/services/product.service';
import { CreateProductRequest, Product } from '../../core/models/product.models';

@Component({
  selector: 'app-admin-products',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="admin-products-view">
      <div class="view-header">
        <div>
          <h2 class="view-title">Product Catalog Management</h2>
          <p class="view-subtitle">Add, edit, or remove catalog items across product categories.</p>
        </div>
        <button class="btn-primary" (click)="showCreateModal = true">
          + Add New Product
        </button>
      </div>

      <!-- Products Table -->
      <div class="table-card">
        <table class="data-table">
          <thead>
            <tr>
              <th>SKU</th>
              <th>Product Name</th>
              <th>Category</th>
              <th>Price</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let product of products">
              <td class="mono">{{ product.sku }}</td>
              <td class="bold">{{ product.name }}</td>
              <td><span class="category-badge">{{ product.category }}</span></td>
              <td class="price">\${{ product.price | number:'1.2-2' }}</td>
              <td>
                <span class="status-pill active" *ngIf="product.isActive">Active</span>
                <span class="status-pill inactive" *ngIf="!product.isActive">Inactive</span>
              </td>
              <td>
                <button class="btn-delete" (click)="deleteProduct(product.id)">Delete</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Add Product Modal -->
      <div class="modal-overlay" *ngIf="showCreateModal">
        <div class="modal-card">
          <div class="modal-header">
            <h3>Add New Product</h3>
            <button class="btn-close" (click)="showCreateModal = false">✕</button>
          </div>

          <form (ngSubmit)="onCreateProduct()" class="modal-form">
            <div class="form-group">
              <label>Product Name</label>
              <input type="text" [(ngModel)]="newProduct.name" name="name" required class="form-control" />
            </div>

            <div class="form-row">
              <div class="form-group">
                <label>SKU</label>
                <input type="text" [(ngModel)]="newProduct.sku" name="sku" required class="form-control" />
              </div>
              <div class="form-group">
                <label>Category</label>
                <input type="text" [(ngModel)]="newProduct.category" name="category" required class="form-control" />
              </div>
            </div>

            <div class="form-row">
              <div class="form-group">
                <label>Price (USD)</label>
                <input type="number" [(ngModel)]="newProduct.price" name="price" step="0.01" required class="form-control" />
              </div>
            </div>

            <div class="form-group">
              <label>Description</label>
              <textarea [(ngModel)]="newProduct.description" name="description" rows="3" class="form-control"></textarea>
            </div>

            <div class="modal-actions">
              <button type="button" class="btn-cancel" (click)="showCreateModal = false">Cancel</button>
              <button type="submit" class="btn-save" [disabled]="saving">
                <span *ngIf="!saving">Save Product</span>
                <span *ngIf="saving">Saving...</span>
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .view-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;
    }
    .view-title {
      font-size: 1.4rem;
      font-weight: 800;
      color: #f8fafc;
      margin: 0 0 0.25rem;
    }
    .view-subtitle {
      color: #94a3b8;
      font-size: 0.85rem;
      margin: 0;
    }
    .btn-primary {
      background: #2563eb;
      color: white;
      border: none;
      padding: 0.6rem 1.2rem;
      border-radius: 8px;
      font-weight: 600;
      cursor: pointer;
      font-size: 0.9rem;
    }
    .btn-primary:hover {
      background: #1d4ed8;
    }
    .table-card {
      background: #1e293b;
      border: 1px solid #334155;
      border-radius: 12px;
      overflow-x: auto;
    }
    .data-table {
      width: 100%;
      border-collapse: collapse;
      text-align: left;
      font-size: 0.9rem;
    }
    .data-table th {
      background: #0f172a;
      color: #94a3b8;
      font-weight: 700;
      padding: 0.85rem 1.25rem;
      border-bottom: 1px solid #334155;
      font-size: 0.75rem;
      text-transform: uppercase;
    }
    .data-table td {
      padding: 1rem 1.25rem;
      border-bottom: 1px solid #334155;
      color: #cbd5e1;
    }
    .mono {
      font-family: monospace;
      color: #93c5fd;
    }
    .bold {
      font-weight: 600;
      color: #f8fafc;
    }
    .category-badge {
      background: #0f172a;
      border: 1px solid #334155;
      padding: 0.25rem 0.5rem;
      border-radius: 4px;
      font-size: 0.75rem;
    }
    .price {
      color: #34d399;
      font-weight: 700;
    }
    .status-pill {
      font-size: 0.7rem;
      font-weight: 700;
      padding: 0.2rem 0.5rem;
      border-radius: 9999px;
      text-transform: uppercase;
    }
    .status-pill.active {
      background: #065f46;
      color: #6ee7b7;
    }
    .status-pill.inactive {
      background: #7f1d1d;
      color: #fca5a5;
    }
    .btn-delete {
      background: transparent;
      border: 1px solid #ef444455;
      color: #fca5a5;
      padding: 0.3rem 0.6rem;
      border-radius: 6px;
      cursor: pointer;
      font-size: 0.75rem;
    }
    .btn-delete:hover {
      background: #dc2626;
      color: white;
    }
    .modal-overlay {
      position: fixed;
      inset: 0;
      background: rgba(0, 0, 0, 0.75);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 9999;
      padding: 1.5rem;
    }
    .modal-card {
      background: #1e293b;
      border: 1px solid #334155;
      border-radius: 16px;
      width: 100%;
      max-width: 520px;
      padding: 1.75rem;
      box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.6);
    }
    .modal-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;
      border-bottom: 1px solid #334155;
      padding-bottom: 0.75rem;
    }
    .modal-header h3 {
      color: #f8fafc;
      margin: 0;
      font-size: 1.25rem;
    }
    .btn-close {
      background: transparent;
      border: none;
      color: #94a3b8;
      font-size: 1.2rem;
      cursor: pointer;
    }
    .modal-form {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }
    .form-group {
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
      flex: 1;
    }
    .form-group label {
      font-size: 0.8rem;
      color: #94a3b8;
      font-weight: 600;
    }
    .form-control {
      background: #0f172a;
      border: 1px solid #334155;
      color: #f8fafc;
      padding: 0.6rem 0.8rem;
      border-radius: 6px;
      font-size: 0.9rem;
      outline: none;
    }
    .form-control:focus {
      border-color: #3b82f6;
    }
    .form-row {
      display: flex;
      gap: 1rem;
    }
    .modal-actions {
      display: flex;
      justify-content: flex-end;
      gap: 0.75rem;
      margin-top: 1rem;
      border-top: 1px solid #334155;
      padding-top: 1rem;
    }
    .btn-cancel {
      background: transparent;
      border: 1px solid #475569;
      color: #cbd5e1;
      padding: 0.5rem 1rem;
      border-radius: 6px;
      cursor: pointer;
    }
    .btn-save {
      background: #2563eb;
      color: white;
      border: none;
      padding: 0.5rem 1.25rem;
      border-radius: 6px;
      font-weight: 600;
      cursor: pointer;
    }
    .btn-save:disabled {
      opacity: 0.5;
    }
  `]
})
export class AdminProductsComponent implements OnInit {
  private productService = inject(ProductService);

  products: Product[] = [];
  showCreateModal = false;
  saving = false;

  newProduct: CreateProductRequest = {
    name: '',
    description: '',
    sku: '',
    price: 0,
    currency: 'USD',
    category: 'Electronics'
  };

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.productService.getProducts(1, 50).subscribe({
      next: res => this.products = res.items,
      error: () => this.products = this.getFallbackProducts()
    });
  }

  onCreateProduct(): void {
    this.saving = true;
    this.productService.createProduct(this.newProduct).subscribe({
      next: p => {
        this.products.unshift(p);
        this.showCreateModal = false;
        this.saving = false;
      },
      error: () => {
        // Fallback for offline demo
        const created: Product = {
          id: crypto.randomUUID ? crypto.randomUUID() : '00000000-0000-0000-0000-000000000099',
          ...this.newProduct,
          isActive: true
        };
        this.products.unshift(created);
        this.showCreateModal = false;
        this.saving = false;
      }
    });
  }

  deleteProduct(id: string): void {
    this.productService.deleteProduct(id).subscribe({
      next: () => this.products = this.products.filter(p => p.id !== id),
      error: () => this.products = this.products.filter(p => p.id !== id)
    });
  }

  private getFallbackProducts(): Product[] {
    return [
      {
        id: '11111111-1111-1111-1111-111111111111',
        name: 'MacBook Pro 16" M3 Max',
        description: 'Workstation laptop',
        sku: 'MBP-M3-16',
        price: 2499.00,
        currency: 'USD',
        category: 'Laptops',
        isActive: true
      },
      {
        id: '22222222-2222-2222-2222-222222222222',
        name: 'Sony WH-1000XM5 Headphones',
        description: 'Noise cancelling',
        sku: 'SNY-WH1000-XM5',
        price: 398.00,
        currency: 'USD',
        category: 'Audio',
        isActive: true
      }
    ];
  }
}
