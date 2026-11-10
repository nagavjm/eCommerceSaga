import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { InventoryItem, RestockRequest } from '../models/inventory.models';

@Injectable({
  providedIn: 'root'
})
export class InventoryService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiGatewayUrl}/api/inventory`;

  getInventoryItem(productId: string): Observable<InventoryItem> {
    return this.http.get<InventoryItem>(`${this.baseUrl}/${productId}`);
  }

  restock(request: RestockRequest): Observable<InventoryItem> {
    return this.http.post<InventoryItem>(`${this.baseUrl}/restock`, request);
  }
}
