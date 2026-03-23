import { useEffect, useState } from 'react'
import { fetchInventory } from '../api/inventory'
import { fetchOrders } from '../api/orders'
import { fetchProducts } from '../api/products'
import {
  DashboardSummaryCards,
  type DashboardSummary,
} from '../components/dashboard/DashboardSummaryCards'
import { ErrorState } from '../components/admin/ErrorState'
import { LoadingState } from '../components/admin/LoadingState'
import type {
  InventoryQueryDto,
  OrderListItemQueryDto,
  ProductQueryDto,
} from '../types/queryDtos'
import { getRequestErrorMessage } from '../utils/axiosError'

type LoadState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'success'; summary: DashboardSummary }

function countLowStock(rows: InventoryQueryDto[]): number {
  return rows.filter(
    (r) => r.availableQuantity <= r.reorderThreshold,
  ).length
}

function buildSummary(
  products: ProductQueryDto[],
  inventory: InventoryQueryDto[],
  orders: OrderListItemQueryDto[],
): DashboardSummary {
  return {
    productCount: products.length,
    inventoryItemCount: inventory.length,
    orderCount: orders.length,
    lowStockCount: countLowStock(inventory),
  }
}

export function Dashboard() {
  const [state, setState] = useState<LoadState>({ status: 'loading' })

  useEffect(() => {
    let cancelled = false

    Promise.all([fetchProducts(), fetchInventory(), fetchOrders()])
      .then(([products, inventory, orders]) => {
        if (!cancelled) {
          setState({
            status: 'success',
            summary: buildSummary(products, inventory, orders),
          })
        }
      })
      .catch((err: unknown) => {
        if (!cancelled) {
          setState({
            status: 'error',
            message: getRequestErrorMessage(err),
          })
        }
      })

    return () => {
      cancelled = true
    }
  }, [])

  return (
    <section className="page">
      <h1 className="page__title">Dashboard</h1>
      <p className="page__lead">
        Operational snapshot based on products, inventory, and orders.
      </p>

      {state.status === 'loading' && <LoadingState message="Loading summary…" />}
      {state.status === 'error' && (
        <ErrorState
          message={`${state.message} Is the API running and reachable (proxy or VITE_API_BASE_URL)?`}
        />
      )}
      {state.status === 'success' && (
        <DashboardSummaryCards summary={state.summary} />
      )}
    </section>
  )
}
