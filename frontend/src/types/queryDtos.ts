/** Mirrors backend `ProductQueryDto` (Application/Queries/QueryDtos.cs). JSON is camelCase. */
export type ProductQueryDto = {
  id: string
  sku: string
  name: string
  price: number
  reorderThreshold: number
}

/** Mirrors backend `InventoryQueryDto`. */
export type InventoryQueryDto = {
  productId: string
  productName: string
  sku: string
  availableQuantity: number
  reorderThreshold: number
}

/** `AdjustInventoryRequest` in InventoryController. */
export type AdjustInventoryRequest = {
  productId: string
  quantityDelta: number
  reason: string
}

/** `AdjustInventoryResponse` from application layer. */
export type AdjustInventoryResponse = {
  productId: string
  availableQuantityAfter: number
  movementId: string
  reason: string
}

/** `PlaceOrderLineRequest` / line item for `PlaceOrderRequest`. */
export type PlaceOrderLineRequest = {
  productId: string
  quantity: number
}

/** Body for `POST /api/orders`. */
export type PlaceOrderRequest = {
  lines: PlaceOrderLineRequest[]
}

/** `PlaceOrderResponse` from OrdersController. */
export type PlaceOrderResponse = {
  orderId: string
  totalAmount: number
}

/** `OrderListItemQueryDto`. */
export type OrderListItemQueryDto = {
  id: string
  createdAt: string
  totalAmount: number
  status: string
}

/** `OrderLineQueryDto`. */
export type OrderLineQueryDto = {
  productId: string
  productName: string
  sku: string
  quantity: number
  unitPrice: number
  lineTotal: number
}

/** `OrderDetailsQueryDto`. */
export type OrderDetailsQueryDto = {
  id: string
  createdAt: string
  totalAmount: number
  status: string
  lines: OrderLineQueryDto[]
}
