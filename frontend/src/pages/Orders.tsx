import { useEffect, useState } from 'react'
import { fetchOrders } from '../api/orders'
import { ErrorState } from '../components/admin/ErrorState'
import { LoadingState } from '../components/admin/LoadingState'
import { OrdersListTable } from '../components/orders/OrdersListTable'
import type { OrderListItemQueryDto } from '../types/queryDtos'
import { getRequestErrorMessage } from '../utils/axiosError'

type LoadState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'success'; data: OrderListItemQueryDto[] }

export function Orders() {
  const [state, setState] = useState<LoadState>({ status: 'loading' })

  useEffect(() => {
    let cancelled = false

    fetchOrders()
      .then((data) => {
        if (!cancelled) setState({ status: 'success', data })
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
      <h1 className="page__title">Orders</h1>
      <p className="page__lead">
        View all placed orders and their current status.
      </p>

      {state.status === 'loading' && <LoadingState />}
      {state.status === 'error' && (
        <ErrorState
          message={`${state.message} Is the API running and reachable (proxy or VITE_API_BASE_URL)?`}
        />
      )}
      {state.status === 'success' && <OrdersListTable orders={state.data} />}
    </section>
  )
}
