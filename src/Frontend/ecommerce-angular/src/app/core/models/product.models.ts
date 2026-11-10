export interface Product {
  id: string;
  name: string;
  description: string;
  sku: string;
  price: number;
  currency: string;
  category: string;
  imageUrl?: string;
  isActive: boolean;
  stockQuantity?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

export interface CreateProductRequest {
  name: string;
  description: string;
  sku: string;
  price: number;
  currency: string;
  category: string;
  imageUrl?: string;
}

export interface UpdateProductRequest {
  name: string;
  description: string;
  price: number;
  category: string;
  imageUrl?: string;
}
