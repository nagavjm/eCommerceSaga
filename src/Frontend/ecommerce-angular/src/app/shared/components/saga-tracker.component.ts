import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SagaStatusDto, VisualSagaStep } from '../../core/models/saga.models';

@Component({
  selector: 'app-saga-tracker',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="saga-tracker-card" *ngIf="saga">
      <!-- Header with Status Badge -->
      <div class="tracker-header">
        <div class="header-left">
          <h3 class="tracker-title">Saga Orchestrator Workflow</h3>
          <span class="saga-id">Order ID: {{ saga.orderId }}</span>
        </div>
        <div class="header-right">
          <span class="state-pill" [ngClass]="getStateBadgeClass(saga.currentState)">
            {{ formatState(saga.currentState) }}
          </span>
          <span class="comp-pill" *ngIf="saga.compensationStatus !== 'None'" [ngClass]="saga.compensationStatus.toLowerCase()">
            Comp: {{ saga.compensationStatus }}
          </span>
        </div>
      </div>

      <!-- Step Timeline -->
      <div class="steps-container">
        <div class="step-item" *ngFor="let step of steps; let i = index" [ngClass]="step.state">
          <div class="step-connector" *ngIf="i > 0" [ngClass]="getConnectorClass(step.state, steps[i-1].state)"></div>
          <div class="step-icon-wrapper">
            <span class="step-icon" *ngIf="step.state === 'completed'">✓</span>
            <span class="step-icon spinner" *ngIf="step.state === 'active'">⏳</span>
            <span class="step-icon" *ngIf="step.state === 'failed'">✕</span>
            <span class="step-icon" *ngIf="step.state === 'compensated'">↩</span>
            <span class="step-number" *ngIf="step.state === 'pending'">{{ i + 1 }}</span>
          </div>
          <div class="step-content">
            <span class="step-label">{{ step.label }}</span>
            <span class="step-desc">{{ step.description }}</span>
          </div>
        </div>
      </div>

      <!-- Live Metadata / Tracking Callout -->
      <div class="tracker-callout success" *ngIf="saga.trackingNumber">
        <span class="callout-icon">📦</span>
        <div class="callout-text">
          <strong>Shipment Created:</strong> Carrier tracking number assigned:
          <span class="tracking-code">{{ saga.trackingNumber }}</span>
        </div>
      </div>

      <!-- Error / Compensation Callout -->
      <div class="tracker-callout failure" *ngIf="saga.failedStep || saga.failureReason">
        <span class="callout-icon">⚠️</span>
        <div class="callout-text">
          <strong>Workflow Halted at Step:</strong> {{ saga.failedStep || 'Unknown' }}<br/>
          <span class="failure-reason">Reason: {{ saga.failureReason }}</span>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .saga-tracker-card {
      background: #0f172a;
      border: 1px solid #1e293b;
      border-radius: 12px;
      padding: 1.5rem;
      color: #f8fafc;
      box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.3);
    }
    .tracker-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 2rem;
      border-bottom: 1px solid #1e293b;
      padding-bottom: 1rem;
    }
    .tracker-title {
      font-size: 1.15rem;
      font-weight: 700;
      color: #f1f5f9;
      margin: 0 0 0.25rem 0;
    }
    .saga-id {
      font-size: 0.75rem;
      color: #94a3b8;
      font-family: monospace;
    }
    .header-right {
      display: flex;
      gap: 0.5rem;
      align-items: center;
    }
    .state-pill {
      font-size: 0.75rem;
      font-weight: 700;
      padding: 0.3rem 0.75rem;
      border-radius: 9999px;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
    .state-pill.completed {
      background: #065f46;
      color: #6ee7b7;
      border: 1px solid #10b981;
    }
    .state-pill.progress {
      background: #1e3a8a;
      color: #93c5fd;
      border: 1px solid #3b82f6;
    }
    .state-pill.failed {
      background: #7f1d1d;
      color: #fca5a5;
      border: 1px solid #ef4444;
    }
    .state-pill.compensated {
      background: #78350f;
      color: #fcd34d;
      border: 1px solid #f59e0b;
    }
    .comp-pill {
      font-size: 0.7rem;
      font-weight: 600;
      padding: 0.25rem 0.5rem;
      border-radius: 4px;
      background: #334155;
      color: #e2e8f0;
    }
    .comp-pill.completed {
      background: #78350f;
      color: #fcd34d;
    }
    .steps-container {
      display: flex;
      justify-content: space-between;
      position: relative;
      margin-bottom: 1.5rem;
      overflow-x: auto;
      padding-bottom: 0.5rem;
    }
    .step-item {
      display: flex;
      flex-direction: column;
      align-items: center;
      text-align: center;
      position: relative;
      flex: 1;
      min-width: 110px;
    }
    .step-connector {
      position: absolute;
      top: 18px;
      right: 50%;
      width: 100%;
      height: 3px;
      background: #334155;
      z-index: 1;
    }
    .step-connector.completed {
      background: #10b981;
    }
    .step-connector.compensated {
      background: #f59e0b;
    }
    .step-icon-wrapper {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      background: #1e293b;
      border: 2px solid #475569;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 700;
      font-size: 0.85rem;
      z-index: 2;
      transition: all 0.3s ease;
    }
    .step-item.completed .step-icon-wrapper {
      background: #065f46;
      border-color: #10b981;
      color: #6ee7b7;
    }
    .step-item.active .step-icon-wrapper {
      background: #1e3a8a;
      border-color: #3b82f6;
      color: #60a5fa;
      animation: pulse 1.5s infinite;
    }
    .step-item.failed .step-icon-wrapper {
      background: #7f1d1d;
      border-color: #ef4444;
      color: #fca5a5;
    }
    .step-item.compensated .step-icon-wrapper {
      background: #78350f;
      border-color: #f59e0b;
      color: #fcd34d;
    }
    .step-content {
      margin-top: 0.5rem;
      display: flex;
      flex-direction: column;
      gap: 0.15rem;
    }
    .step-label {
      font-size: 0.8rem;
      font-weight: 600;
      color: #cbd5e1;
    }
    .step-desc {
      font-size: 0.7rem;
      color: #64748b;
    }
    .step-item.completed .step-label {
      color: #6ee7b7;
    }
    .step-item.active .step-label {
      color: #93c5fd;
    }
    .step-item.failed .step-label {
      color: #fca5a5;
    }
    .step-item.compensated .step-label {
      color: #fcd34d;
    }
    .tracker-callout {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 0.75rem 1rem;
      border-radius: 8px;
      font-size: 0.85rem;
      margin-top: 1rem;
    }
    .tracker-callout.success {
      background: #064e3b33;
      border: 1px solid #059669;
      color: #a7f3d0;
    }
    .tracker-callout.failure {
      background: #7f1d1d33;
      border: 1px solid #dc2626;
      color: #fecaca;
    }
    .tracking-code {
      background: #065f46;
      color: #a7f3d0;
      padding: 0.2rem 0.5rem;
      border-radius: 4px;
      font-family: monospace;
      font-weight: 700;
      margin-left: 0.4rem;
    }
    .failure-reason {
      font-size: 0.8rem;
      color: #f87171;
    }
    @keyframes pulse {
      0% { transform: scale(1); box-shadow: 0 0 0 0 rgba(59, 130, 246, 0.7); }
      70% { transform: scale(1.08); box-shadow: 0 0 0 10px rgba(59, 130, 246, 0); }
      100% { transform: scale(1); box-shadow: 0 0 0 0 rgba(59, 130, 246, 0); }
    }
  `]
})
export class SagaTrackerComponent {
  private _saga: SagaStatusDto | null = null;

  @Input()
  set saga(value: SagaStatusDto | null) {
    this._saga = value;
    if (value) {
      this.computeSteps(value);
    }
  }
  get saga(): SagaStatusDto | null {
    return this._saga;
  }

  steps: VisualSagaStep[] = [];

  private computeSteps(s: SagaStatusDto): void {
    const isCompleted = s.currentState === 'Completed';
    const isCompensated = s.currentState === 'Compensated';
    const isFailed = s.currentState === 'Failed';

    const stepDefs = [
      { name: 'OrderPlaced', label: '1. Order Placed', desc: 'Order received & Saga initiated' },
      { name: 'ReserveStock', label: '2. Stock Reserved', desc: 'Inventory reserved by SKU' },
      { name: 'ProcessPayment', label: '3. Payment Processed', desc: 'Payment gateway transaction' },
      { name: 'ConfirmOrder', label: '4. Order Confirmed', desc: 'Stock deducted & confirmed' },
      { name: 'CreateShipment', label: '5. Shipment Created', desc: 'Carrier tracking assigned' },
      { name: 'SendNotification', label: '6. Email Delivered', desc: 'Customer receipt sent' }
    ];

    this.steps = stepDefs.map((def, idx) => {
      let state: VisualSagaStep['state'] = 'pending';

      if (isCompleted) {
        state = 'completed';
      } else if (s.completedSteps.includes(def.name)) {
        state = 'completed';
      } else if (s.failedStep === def.name) {
        state = isCompensated ? 'compensated' : 'failed';
      } else if (s.currentState.toLowerCase().includes(def.name.toLowerCase())) {
        state = 'active';
      } else if (idx === 0) {
        state = 'completed';
      }

      return {
        name: def.name,
        label: def.label,
        description: def.desc,
        state
      };
    });
  }

  formatState(state: string): string {
    return state.replace(/([A-Z])/g, ' $1').trim();
  }

  getStateBadgeClass(state: string): string {
    switch (state) {
      case 'Completed': return 'completed';
      case 'Compensated': return 'compensated';
      case 'Failed': return 'failed';
      default: return 'progress';
    }
  }

  getConnectorClass(curr: VisualSagaStep['state'], prev: VisualSagaStep['state']): string {
    if (curr === 'completed' || (prev === 'completed' && curr === 'active')) {
      return 'completed';
    }
    if (curr === 'compensated' || prev === 'compensated') {
      return 'compensated';
    }
    return '';
  }
}
