export interface SagaStatusDto {
  id: string;
  orderId: string;
  customerId: string;
  customerEmail: string;
  currentState: string;
  completedSteps: string[];
  failedStep?: string;
  failureReason?: string;
  retryCount: number;
  compensationStatus: string;
  trackingNumber?: string;
  correlationId: string;
  createdAtUtc: string;
  updatedAtUtc?: string;
}

export type SagaStepState = 'pending' | 'active' | 'completed' | 'failed' | 'compensated';

export interface VisualSagaStep {
  name: string;
  label: string;
  state: SagaStepState;
  description: string;
}
