export interface OrderItem {
  id?: string;
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  totalPrice?: number;
}

export interface Address {
  street: string;
  city: string;
  state: string;
  country: string;
  zipCode: string;
}

export interface CreateOrderRequest {
  customerId: string;
  customerEmail: string;
  shippingAddress: Address;
  items: {
    productId: string;
    productName: string;
    quantity: number;
    unitPrice: number;
  }[];
}

export interface Order {
  id: string;
  customerId: string;
  customerEmail: string;
  status: string;
  totalAmount: number;
  shippingAddress: Address;
  items: OrderItem[];
  correlationId: string;
  cancellationReason?: string;
  createdAtUtc: string;
}

export interface OrderCreatedAcceptedResponse {
  orderId: string;
  status: string;
  message: string;
  correlationId: string;
}
