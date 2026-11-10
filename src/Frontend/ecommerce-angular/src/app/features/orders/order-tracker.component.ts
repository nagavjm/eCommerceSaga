import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { SagaService } from '../../core/services/saga.service';
import { SagaStatusDto } from '../../core/models/saga.models';
import { SagaTrackerComponent } from '../../shared/components/saga-tracker.component';

@Component({
  selector: 'app-order-tracker',
  standalone: true,
  imports: [CommonModule, RouterLink, SagaTrackerComponent],
  template: `
    <div class="tracker-page">
      <div class="tracker-banner">
        <div class="banner-content">
          <span class="banner-pill">Live Execution</span>
          <h1 class="banner-title">Real-Time Saga Progression</h1>
          <p class="banner-desc">
            Observing the distributed transaction across autonomous microservices using Azure Service Bus Topics, Transactional Outbox, and Inbox deduplication.
          </p>
        </div>
      </div>

      <!-- Live Saga Tracker Visualizer -->
      <app-saga-tracker [saga]="sagaStatus"></app-saga-tracker>

      <!-- Status Summary Card -->
      <div class="details-card" *ngIf="sagaStatus">
        <h3 class="details-title">Saga Transaction Details</h3>

        <div class="details-grid">
          <div class="detail-item">
            <span class="detail-label">Order Reference ID</span>
            <span class="detail-value mono">{{ sagaStatus.orderId }}</span>
          </div>
          <div class="detail-item">
            <span class="detail-label">Correlation ID</span>
            <span class="detail-value mono">{{ sagaStatus.correlationId }}</span>
          </div>
          <div class="detail-item">
            <span class="detail-label">Current State</span>
            <span class="detail-value highlight">{{ sagaStatus.currentState }}</span>
          </div>
          <div class="detail-item">
            <span class="detail-label">Compensation Status</span>
            <span class="detail-value">{{ sagaStatus.compensationStatus }}</span>
          </div>
          <div class="detail-item">
            <span class="detail-label">Carrier Tracking Number</span>
            <span class="detail-value tracking" *ngIf="sagaStatus.trackingNumber; else noTracking">
              {{ sagaStatus.trackingNumber }}
            </span>
            <ng-template #noTracking>
              <span class="detail-value text-muted">Awaiting Step 5 completion...</span>
            </ng-template>
          </div>
          <div class="detail-item">
            <span class="detail-label">Transaction Initiated</span>
            <span class="detail-value">{{ sagaStatus.createdAtUtc | date:'medium' }}</span>
          </div>
        </div>

        <div class="action-buttons">
          <a routerLink="/" class="btn-home">← Back to Catalog</a>
          <a routerLink="/orders" class="btn-orders">View Order History</a>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .tracker-page {
      max-width: 1100px;
      margin: 0 auto;
      padding: 2rem 1.5rem;
      display: flex;
      flex-direction: column;
      gap: 2rem;
    }
    .tracker-banner {
      background: linear-gradient(135deg, #0f172a, #1e293b);
      border: 1px solid #334155;
      border-radius: 12px;
      padding: 2rem;
    }
    .banner-pill {
      background: #1e3a8a;
      color: #93c5fd;
      border: 1px solid #3b82f6;
      font-size: 0.75rem;
      font-weight: 700;
      padding: 0.25rem 0.6rem;
      border-radius: 9999px;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
    .banner-title {
      font-size: 1.8rem;
      font-weight: 800;
      color: #f8fafc;
      margin: 0.75rem 0 0.5rem;
    }
    .banner-desc {
      color: #94a3b8;
      font-size: 0.95rem;
      line-height: 1.5;
      margin: 0;
      max-width: 800px;
    }
    .details-card {
      background: #1e293b;
      border: 1px solid #334155;
      border-radius: 12px;
      padding: 1.5rem;
    }
    .details-title {
      color: #f1f5f9;
      font-size: 1.15rem;
      font-weight: 700;
      margin-bottom: 1.25rem;
    }
    .details-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
      gap: 1.25rem;
      margin-bottom: 2rem;
    }
    .detail-item {
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
    }
    .detail-label {
      font-size: 0.75rem;
      color: #94a3b8;
      font-weight: 600;
      text-transform: uppercase;
    }
    .detail-value {
      font-size: 0.95rem;
      color: #f8fafc;
      font-weight: 500;
    }
    .detail-value.mono {
      font-family: monospace;
      font-size: 0.85rem;
      color: #93c5fd;
    }
    .detail-value.highlight {
      color: #34d399;
      font-weight: 700;
    }
    .detail-value.tracking {
      color: #60a5fa;
      font-family: monospace;
      font-weight: 700;
    }
    .text-muted {
      color: #64748b;
      font-style: italic;
    }
    .action-buttons {
      display: flex;
      gap: 1rem;
      border-top: 1px solid #334155;
      padding-top: 1.25rem;
    }
    .btn-home, .btn-orders {
      padding: 0.6rem 1.2rem;
      border-radius: 8px;
      font-size: 0.9rem;
      font-weight: 600;
      text-decoration: none;
      transition: all 0.2s;
    }
    .btn-home {
      background: #0f172a;
      border: 1px solid #334155;
      color: #cbd5e1;
    }
    .btn-home:hover {
      background: #334155;
      color: white;
    }
    .btn-orders {
      background: #2563eb;
      color: white;
    }
    .btn-orders:hover {
      background: #1d4ed8;
    }
  `]
})
export class OrderTrackerComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private sagaService = inject(SagaService);
  private sub: Subscription | null = null;

  orderId = '';
  sagaStatus: SagaStatusDto | null = null;

  ngOnInit(): void {
    this.orderId = this.route.snapshot.paramMap.get('orderId') || '';
    const simulateFailure = this.route.snapshot.queryParamMap.get('simulateFailure') === 'true';

    if (this.orderId) {
      this.sub = this.sagaService.pollSagaStatus(this.orderId).subscribe({
        next: status => {
          this.sagaStatus = status;
        },
        error: () => {
          // If Gateway is not connected, provide a dynamic animated mock progression
          this.runMockSagaSimulation(simulateFailure);
        }
      });
    }
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  private runMockSagaSimulation(simulateFailure: boolean): void {
    const base: SagaStatusDto = {
      id: crypto.randomUUID ? crypto.randomUUID() : '00000000-0000-0000-0000-000000000001',
      orderId: this.orderId,
      customerId: '00000000-0000-0000-0000-000000000001',
      customerEmail: 'customer@example.com',
      currentState: 'OrderPlaced',
      completedSteps: ['OrderPlaced'],
      retryCount: 0,
      compensationStatus: 'None',
      correlationId: crypto.randomUUID ? crypto.randomUUID() : '00000000-0000-0000-0000-000000000002',
      createdAtUtc: new Date().toISOString()
    };

    this.sagaStatus = { ...base };

    setTimeout(() => {
      if (this.sagaStatus) {
        this.sagaStatus = {
          ...this.sagaStatus,
          currentState: 'ReservingStock',
          completedSteps: ['OrderPlaced', 'ReserveStock']
        };
      }
    }, 1500);

    setTimeout(() => {
      if (!this.sagaStatus) return;

      if (simulateFailure) {
        // Demonstrate Saga Compensation Rollback
        this.sagaStatus = {
          ...this.sagaStatus,
          currentState: 'Compensated',
          failedStep: 'ProcessPayment',
          failureReason: 'Payment simulated failure: insufficient funds on card 4242.',
          compensationStatus: 'Completed',
          completedSteps: ['OrderPlaced', 'ReserveStock']
        };
      } else {
        this.sagaStatus = {
          ...this.sagaStatus,
          currentState: 'ProcessingPayment',
          completedSteps: ['OrderPlaced', 'ReserveStock', 'ProcessPayment']
        };
      }
    }, 3000);

    if (!simulateFailure) {
      setTimeout(() => {
        if (!this.sagaStatus) return;
        this.sagaStatus = {
          ...this.sagaStatus,
          currentState: 'ConfirmingOrder',
          completedSteps: ['OrderPlaced', 'ReserveStock', 'ProcessPayment', 'ConfirmOrder']
        };
      }, 4500);

      setTimeout(() => {
        if (!this.sagaStatus) return;
        this.sagaStatus = {
          ...this.sagaStatus,
          currentState: 'Completed',
          trackingNumber: 'TRK-' + Math.random().toString(36).substring(2, 10).toUpperCase(),
          completedSteps: ['OrderPlaced', 'ReserveStock', 'ProcessPayment', 'ConfirmOrder', 'CreateShipment', 'SendNotification']
        };
      }, 6000);
    }
  }
}
