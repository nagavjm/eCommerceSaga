export interface InventoryItem {
  id: string;
  productId: string;
  availableQuantity: number;
  reservedQuantity: number;
  totalQuantity: number;
}

export interface RestockRequest {
  productId: string;
  quantity: number;
}
