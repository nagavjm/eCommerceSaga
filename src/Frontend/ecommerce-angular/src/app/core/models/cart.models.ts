export interface CartItem {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  totalPrice: number;
}

export interface Cart {
  customerId: string;
  items: CartItem[];
  totalAmount: number;
}

export interface AddCartItemRequest {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
}
