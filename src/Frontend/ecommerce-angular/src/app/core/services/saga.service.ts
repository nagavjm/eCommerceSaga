import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, timer, switchMap, takeWhile, distinctUntilChanged } from 'rxjs';
import { environment } from '../../../environments/environment';
import { SagaStatusDto } from '../models/saga.models';

@Injectable({
  providedIn: 'root'
})
export class SagaService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiGatewayUrl}/api/saga`;

  getSagaStatus(orderId: string): Observable<SagaStatusDto> {
    return this.http.get<SagaStatusDto>(`${this.baseUrl}/${orderId}`);
  }

  pollSagaStatus(orderId: string, intervalMs = 1500, maxAttempts = 40): Observable<SagaStatusDto> {
    let attempts = 0;
    return timer(0, intervalMs).pipe(
      switchMap(() => this.getSagaStatus(orderId)),
      takeWhile(status => {
        attempts++;
        const isTerminal = ['Completed', 'Compensated', 'Failed'].includes(status.currentState);
        return !isTerminal && attempts < maxAttempts;
      }, true), // inclusive of final emission
      distinctUntilChanged((prev, curr) =>
        prev.currentState === curr.currentState &&
        prev.completedSteps.length === curr.completedSteps.length &&
        prev.compensationStatus === curr.compensationStatus
      )
    );
  }
}
