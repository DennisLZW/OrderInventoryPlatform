import type {
  AdjustInventoryRequest,
  AdjustInventoryResponse,
  InventoryQueryDto,
} from '../types/queryDtos'
import { api } from './client'

export async function fetchInventory(): Promise<InventoryQueryDto[]> {
  const { data } = await api.get<InventoryQueryDto[]>('/api/inventory')
  return data
}

export async function postInventoryAdjustment(
  body: AdjustInventoryRequest,
): Promise<AdjustInventoryResponse> {
  const { data } = await api.post<AdjustInventoryResponse>(
    '/api/inventory/adjustments',
    body,
  )
  return data
}
