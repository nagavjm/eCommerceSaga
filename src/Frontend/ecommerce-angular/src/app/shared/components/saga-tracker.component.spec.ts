import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SagaTrackerComponent } from './saga-tracker.component';
import { SagaStatusDto } from '../../core/models/saga.models';

describe('SagaTrackerComponent', () => {
  let component: SagaTrackerComponent;
  let fixture: ComponentFixture<SagaTrackerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SagaTrackerComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(SagaTrackerComponent);
    component = fixture.componentInstance;
  });

  it('should create the tracker component', () => {
    expect(component).toBeTruthy();
  });

  it('should mark all 6 steps completed when currentState is Completed', () => {
    const saga: SagaStatusDto = {
      id: '1',
      orderId: 'order-123',
      customerId: 'cust-123',
      customerEmail: 'cust@example.com',
      currentState: 'Completed',
      completedSteps: ['OrderPlaced', 'ReserveStock', 'ProcessPayment', 'ConfirmOrder', 'CreateShipment', 'SendNotification'],
      retryCount: 0,
      compensationStatus: 'None',
      trackingNumber: 'TRK-12345',
      correlationId: 'corr-123',
      createdAtUtc: new Date().toISOString()
    };

    component.saga = saga;
    fixture.detectChanges();

    expect(component.steps.length).toBe(6);
    expect(component.steps.every(s => s.state === 'completed')).toBeTrue();
  });

  it('should mark failed step as compensated when saga was rolled back', () => {
    const saga: SagaStatusDto = {
      id: '2',
      orderId: 'order-456',
      customerId: 'cust-123',
      customerEmail: 'cust@example.com',
      currentState: 'Compensated',
      completedSteps: ['OrderPlaced', 'ReserveStock'],
      failedStep: 'ProcessPayment',
      failureReason: 'Card declined',
      retryCount: 1,
      compensationStatus: 'Completed',
      correlationId: 'corr-456',
      createdAtUtc: new Date().toISOString()
    };

    component.saga = saga;
    fixture.detectChanges();

    const paymentStep = component.steps.find(s => s.name === 'ProcessPayment');
    expect(paymentStep).toBeDefined();
    expect(paymentStep?.state).toBe('compensated');
  });
});
