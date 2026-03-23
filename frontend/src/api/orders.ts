import type {
  OrderDetailsQueryDto,
  OrderListItemQueryDto,
  PlaceOrderRequest,
  PlaceOrderResponse,
} from '../types/queryDtos'
import { api } from './client'

export async function fetchOrders(): Promise<OrderListItemQueryDto[]> {
  const { data } = await api.get<OrderListItemQueryDto[]>('/api/orders')
  return data
}

export async function fetchOrderById(
  id: string,
): Promise<OrderDetailsQueryDto> {
  const { data } = await api.get<OrderDetailsQueryDto>(`/api/orders/${id}`)
  return data
}

export async function postPlaceOrder(
  body: PlaceOrderRequest,
): Promise<PlaceOrderResponse> {
  const { data } = await api.post<PlaceOrderResponse>('/api/orders', body)
  return data
}
