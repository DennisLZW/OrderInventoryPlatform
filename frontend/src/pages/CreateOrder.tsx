import { useEffect, useState } from 'react'
import { fetchProducts } from '../api/products'
import { ErrorState } from '../components/admin/ErrorState'
import { LoadingState } from '../components/admin/LoadingState'
import { CreateOrderForm } from '../components/orders/CreateOrderForm'
import type { ProductQueryDto } from '../types/queryDtos'
import { getRequestErrorMessage } from '../utils/axiosError'

type LoadState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'success'; data: ProductQueryDto[] }

export function CreateOrder() {
  const [state, setState] = useState<LoadState>({ status: 'loading' })

  useEffect(() => {
    let cancelled = false

    fetchProducts()
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
      <h1 className="page__title">Create Order</h1>
      <p className="page__lead">
        Select products and quantities to place a new order.
      </p>

      {state.status === 'loading' && <LoadingState message="Loading products…" />}
      {state.status === 'error' && (
        <ErrorState
          message={`${state.message} Is the API running and reachable (proxy or VITE_API_BASE_URL)?`}
        />
      )}
      {state.status === 'success' && <CreateOrderForm products={state.data} />}
    </section>
  )
}
