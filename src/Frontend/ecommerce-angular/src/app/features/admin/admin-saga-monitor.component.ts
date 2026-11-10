import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SagaService } from '../../core/services/saga.service';
import { SagaStatusDto } from '../../core/models/saga.models';
import { SagaTrackerComponent } from '../../shared/components/saga-tracker.component';

@Component({
  selector: 'app-admin-saga-monitor',
  standalone: true,
  imports: [CommonModule, FormsModule, SagaTrackerComponent],
  template: `
    <div class="admin-saga-view">
      <div class="view-header">
        <div>
          <h2 class="view-title">Distributed Saga Orchestrator Monitor</h2>
          <p class="view-subtitle">Inspect active, completed, or compensated distributed transactions across the microservices ecosystem.</p>
        </div>
      </div>

      <!-- Search / Filter Bar -->
      <div class="search-panel">
        <label>Lookup Saga by Order ID:</label>
        <div class="search-input-group">
          <input
            type="text"
            [(ngModel)]="searchOrderId"
            placeholder="e.g. 99999999-9999-9999-9999-999999999991"
            class="form-control"
          />
          <button class="btn-search" (click)="onSearchSaga()">Inspect Saga</button>
        </div>
      </div>

      <!-- Quick Preset Sagas -->
      <div class="preset-sagas">
        <span class="preset-label">Sample Transactions:</span>
        <button class="btn-preset success" (click)="loadPreset('success')">
          ✓ Completed Saga (Order Confirmed + Shipped)
        </button>
        <button class="btn-preset failure" (click)="loadPreset('failure')">
          ⚠️ Compensated Saga (Payment Failed + Stock Rollback)
        </button>
      </div>

      <!-- Active Saga Inspection View -->
      <div class="saga-result" *ngIf="inspectedSaga">
        <app-saga-tracker [saga]="inspectedSaga"></app-saga-tracker>

        <div class="saga-audit-card">
          <h4 class="audit-title">State Machine Audit Log</h4>

          <div class="audit-grid">
            <div class="audit-item">
              <span class="audit-label">State Machine ID</span>
              <span class="audit-val mono">{{ inspectedSaga.id }}</span>
            </div>
            <div class="audit-item">
              <span class="audit-label">Correlation ID</span>
              <span class="audit-val mono">{{ inspectedSaga.correlationId }}</span>
            </div>
            <div class="audit-item">
              <span class="audit-label">Customer Email</span>
              <span class="audit-val">{{ inspectedSaga.customerEmail }}</span>
            </div>
            <div class="audit-item">
              <span class="audit-label">Retry Attempts</span>
              <span class="audit-val">{{ inspectedSaga.retryCount }}</span>
            </div>
            <div class="audit-item">
              <span class="audit-label">Completed Steps</span>
              <span class="audit-val mono">{{ inspectedSaga.completedSteps.join(' → ') }}</span>
            </div>
            <div class="audit-item">
              <span class="audit-label">Failure Diagnostics</span>
              <span class="audit-val error-text">{{ inspectedSaga.failureReason || 'None (Healthy)' }}</span>
            </div>
          </div>
        </div>
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
    .search-panel {
      background: #1e293b;
      border: 1px solid #334155;
      border-radius: 12px;
      padding: 1.25rem;
      margin-bottom: 1.5rem;
    }
    .search-panel label {
      display: block;
      font-size: 0.8rem;
      color: #94a3b8;
      font-weight: 600;
      margin-bottom: 0.5rem;
    }
    .search-input-group {
      display: flex;
      gap: 0.75rem;
    }
    .form-control {
      flex: 1;
      background: #0f172a;
      border: 1px solid #334155;
      color: #f8fafc;
      padding: 0.65rem 0.9rem;
      border-radius: 8px;
      font-family: monospace;
      font-size: 0.9rem;
      outline: none;
    }
    .form-control:focus {
      border-color: #3b82f6;
    }
    .btn-search {
      background: #2563eb;
      color: white;
      border: none;
      padding: 0.65rem 1.25rem;
      border-radius: 8px;
      font-weight: 600;
      cursor: pointer;
    }
    .preset-sagas {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      margin-bottom: 2rem;
      flex-wrap: wrap;
    }
    .preset-label {
      font-size: 0.8rem;
      color: #94a3b8;
      font-weight: 600;
    }
    .btn-preset {
      padding: 0.4rem 0.8rem;
      border-radius: 6px;
      font-size: 0.8rem;
      font-weight: 600;
      cursor: pointer;
      border: 1px solid transparent;
      transition: all 0.2s;
    }
    .btn-preset.success {
      background: #064e3b33;
      border-color: #059669;
      color: #6ee7b7;
    }
    .btn-preset.success:hover {
      background: #065f46;
      color: white;
    }
    .btn-preset.failure {
      background: #78350f33;
      border-color: #f59e0b;
      color: #fcd34d;
    }
    .btn-preset.failure:hover {
      background: #78350f;
      color: white;
    }
    .saga-result {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }
    .saga-audit-card {
      background: #1e293b;
      border: 1px solid #334155;
      border-radius: 12px;
      padding: 1.5rem;
    }
    .audit-title {
      color: #f1f5f9;
      font-size: 1.1rem;
      margin: 0 0 1.25rem;
    }
    .audit-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
      gap: 1.25rem;
    }
    .audit-item {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
    }
    .audit-label {
      font-size: 0.75rem;
      color: #94a3b8;
      font-weight: 600;
      text-transform: uppercase;
    }
    .audit-val {
      font-size: 0.9rem;
      color: #e2e8f0;
    }
    .audit-val.mono {
      font-family: monospace;
      color: #93c5fd;
      font-size: 0.8rem;
    }
    .error-text {
      color: #fca5a5;
    }
  `]
})
export class AdminSagaMonitorComponent implements OnInit {
  private sagaService = inject(SagaService);

  searchOrderId = '';
  inspectedSaga: SagaStatusDto | null = null;

  ngOnInit(): void {
    this.loadPreset('success');
  }

  onSearchSaga(): void {
    if (!this.searchOrderId.trim()) return;

    this.sagaService.getSagaStatus(this.searchOrderId.trim()).subscribe({
      next: s => this.inspectedSaga = s,
      error: () => this.loadPreset('success')
    });
  }

  loadPreset(type: 'success' | 'failure'): void {
    if (type === 'success') {
      this.inspectedSaga = {
        id: '11111111-2222-3333-4444-555555555555',
        orderId: '99999999-9999-9999-9999-999999999991',
        customerId: '00000000-0000-0000-0000-000000000001',
        customerEmail: 'customer@example.com',
        currentState: 'Completed',
        completedSteps: ['OrderPlaced', 'ReserveStock', 'ProcessPayment', 'ConfirmOrder', 'CreateShipment', 'SendNotification'],
        retryCount: 0,
        compensationStatus: 'None',
        trackingNumber: 'TRK-984210B',
        correlationId: '88888888-8888-8888-8888-888888888881',
        createdAtUtc: new Date(Date.now() - 3600000).toISOString()
      };
      this.searchOrderId = this.inspectedSaga.orderId;
    } else {
      this.inspectedSaga = {
        id: '22222222-3333-4444-5555-666666666666',
        orderId: '77777777-7777-7777-7777-777777777772',
        customerId: '00000000-0000-0000-0000-000000000001',
        customerEmail: 'customer@example.com',
        currentState: 'Compensated',
        completedSteps: ['OrderPlaced', 'ReserveStock'],
        failedStep: 'ProcessPayment',
        failureReason: 'Payment gateway rejected: insufficient funds on card 4242.',
        retryCount: 1,
        compensationStatus: 'Completed',
        correlationId: '66666666-6666-6666-6666-666666666662',
        createdAtUtc: new Date(Date.now() - 7200000).toISOString()
      };
      this.searchOrderId = this.inspectedSaga.orderId;
    }
  }
}
