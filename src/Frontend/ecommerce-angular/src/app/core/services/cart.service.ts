import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AddCartItemRequest, Cart, CartItem } from '../models/cart.models';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private baseUrl = `${environment.apiGatewayUrl}/api/cart`;

  cart = signal<Cart>(this.loadCartFromStorage());
  itemCount = computed(() =>
    this.cart().items.reduce((sum, item) => sum + item.quantity, 0)
  );
  totalAmount = computed(() => this.cart().totalAmount);

  getCustomerId(): string {
    return this.authService.currentUser()?.id || '00000000-0000-0000-0000-000000000001';
  }

  fetchCart(): Observable<Cart> {
    const customerId = this.getCustomerId();
    return this.http.get<Cart>(`${this.baseUrl}/${customerId}`).pipe(
      tap(cart => this.setCart(cart))
    );
  }

  addItem(productId: string, productName: string, unitPrice: number, quantity = 1): Observable<Cart> {
    const customerId = this.getCustomerId();
    const req: AddCartItemRequest = {
      productId,
      productName,
      unitPrice,
      quantity
    };

    return this.http.post<Cart>(`${this.baseUrl}/${customerId}/items`, req).pipe(
      tap(cart => this.setCart(cart))
    );
  }

  updateQuantity(productId: string, quantity: number): Observable<Cart> {
    const customerId = this.getCustomerId();
    return this.http.put<Cart>(`${this.baseUrl}/${customerId}/items/${productId}`, { quantity }).pipe(
      tap(cart => this.setCart(cart))
    );
  }

  removeItem(productId: string): Observable<Cart> {
    const customerId = this.getCustomerId();
    return this.http.delete<Cart>(`${this.baseUrl}/${customerId}/items/${productId}`).pipe(
      tap(cart => this.setCart(cart))
    );
  }

  clearCart(): Observable<void> {
    const customerId = this.getCustomerId();
    return this.http.delete<void>(`${this.baseUrl}/${customerId}`).pipe(
      tap(() => {
        const emptyCart: Cart = { customerId, items: [], totalAmount: 0 };
        this.setCart(emptyCart);
      })
    );
  }

  private setCart(cart: Cart): void {
    this.cart.set(cart);
    localStorage.setItem('ecommerce_cart', JSON.stringify(cart));
  }

  private loadCartFromStorage(): Cart {
    try {
      const stored = localStorage.getItem('ecommerce_cart');
      if (stored) {
        return JSON.parse(stored);
      }
    } catch {}
    return {
      customerId: this.getCustomerId(),
      items: [],
      totalAmount: 0
    };
  }
}
