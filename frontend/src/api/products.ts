import type { ProductQueryDto } from '../types/queryDtos'
import { api } from './client'

export async function fetchProducts(): Promise<ProductQueryDto[]> {
  const { data } = await api.get<ProductQueryDto[]>('/api/products')
  return data
}
