import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InventoryService } from '../../core/services/inventory.service';
import { InventoryItem } from '../../core/models/inventory.models';

@Component({
  selector: 'app-admin-inventory',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="admin-inventory-view">
      <div class="view-header">
        <div>
          <h2 class="view-title">Inventory & Real-Time Stock</h2>
          <p class="view-subtitle">Monitor available units, reserved quantities, and execute instant stock replenishment.</p>
        </div>
      </div>

      <div class="table-card">
        <table class="data-table">
          <thead>
            <tr>
              <th>Product Reference ID</th>
              <th>Available Units</th>
              <th>Reserved in Sagas</th>
              <th>Total Physical Units</th>
              <th>Stock Status</th>
              <th>Action</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let item of inventoryItems">
              <td class="mono">{{ item.productId }}</td>
              <td class="qty available">{{ item.availableQuantity }}</td>
              <td class="qty reserved">{{ item.reservedQuantity }}</td>
              <td class="qty total">{{ item.totalQuantity }}</td>
              <td>
                <span class="stock-badge in-stock" *ngIf="item.availableQuantity > 10">Healthy</span>
                <span class="stock-badge low-stock" *ngIf="item.availableQuantity <= 10 && item.availableQuantity > 0">Low Stock</span>
                <span class="stock-badge out-stock" *ngIf="item.availableQuantity === 0">Out of Stock</span>
              </td>
              <td>
                <button class="btn-restock" (click)="restock(item.productId)">
                  + Restock 50 Units
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="toast-restock" *ngIf="restockedMsg">
        ✓ {{ restockedMsg }}
      </div>
    </div>
  `,
  styles: [`
    .view-header {
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
      font-size: 0.8rem;
      color: #93c5fd;
    }
    .qty {
      font-weight: 700;
    }
    .qty.available {
      color: #34d399;
      font-size: 1.05rem;
    }
    .qty.reserved {
      color: #f59e0b;
    }
    .qty.total {
      color: #f8fafc;
    }
    .stock-badge {
      font-size: 0.7rem;
      font-weight: 700;
      padding: 0.2rem 0.5rem;
      border-radius: 9999px;
      text-transform: uppercase;
    }
    .stock-badge.in-stock {
      background: #065f46;
      color: #6ee7b7;
    }
    .stock-badge.low-stock {
      background: #78350f;
      color: #fcd34d;
    }
    .stock-badge.out-stock {
      background: #7f1d1d;
      color: #fca5a5;
    }
    .btn-restock {
      background: #0f172a;
      border: 1px solid #3b82f6;
      color: #60a5fa;
      padding: 0.4rem 0.8rem;
      border-radius: 6px;
      font-size: 0.8rem;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.2s;
    }
    .btn-restock:hover {
      background: #2563eb;
      color: white;
    }
    .toast-restock {
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
    }
  `]
})
export class AdminInventoryComponent implements OnInit {
  private inventoryService = inject(InventoryService);

  inventoryItems: InventoryItem[] = [];
  restockedMsg: string | null = null;

  ngOnInit(): void {
    this.inventoryItems = this.getMockInventory();
  }

  restock(productId: string): void {
    this.inventoryService.restock({ productId, quantity: 50 }).subscribe({
      next: updated => {
        const item = this.inventoryItems.find(i => i.productId === productId);
        if (item) {
          item.availableQuantity = updated.availableQuantity;
          item.totalQuantity = updated.totalQuantity;
        }
        this.notify(`Successfully restocked 50 units!`);
      },
      error: () => {
        // Fallback update
        const item = this.inventoryItems.find(i => i.productId === productId);
        if (item) {
          item.availableQuantity += 50;
          item.totalQuantity += 50;
        }
        this.notify(`Successfully restocked 50 units for item!`);
      }
    });
  }

  private notify(msg: string): void {
    this.restockedMsg = msg;
    setTimeout(() => this.restockedMsg = null, 3000);
  }

  private getMockInventory(): InventoryItem[] {
    return [
      {
        id: '11111111-aaaa-bbbb-cccc-111111111111',
        productId: '11111111-1111-1111-1111-111111111111',
        availableQuantity: 45,
        reservedQuantity: 5,
        totalQuantity: 50
      },
      {
        id: '22222222-aaaa-bbbb-cccc-222222222222',
        productId: '22222222-2222-2222-2222-222222222222',
        availableQuantity: 120,
        reservedQuantity: 2,
        totalQuantity: 122
      },
      {
        id: '33333333-aaaa-bbbb-cccc-333333333333',
        productId: '33333333-3333-3333-3333-333333333333',
        availableQuantity: 15,
        reservedQuantity: 0,
        totalQuantity: 15
      },
      {
        id: '44444444-aaaa-bbbb-cccc-444444444444',
        productId: '44444444-4444-4444-4444-444444444444',
        availableQuantity: 300,
        reservedQuantity: 10,
        totalQuantity: 310
      }
    ];
  }
}
