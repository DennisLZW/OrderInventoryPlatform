import { useEffect, useState } from 'react'
import { fetchProducts } from '../api/products'
import { ErrorState } from '../components/admin/ErrorState'
import { LoadingState } from '../components/admin/LoadingState'
import { ProductsTable } from '../components/products/ProductsTable'
import type { ProductQueryDto } from '../types/queryDtos'
import { getRequestErrorMessage } from '../utils/axiosError'

type LoadState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'success'; data: ProductQueryDto[] }

export function Products() {
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
      <h1 className="page__title">Products</h1>
      <p className="page__lead">
        Browse all catalog products and pricing information.
      </p>

      {state.status === 'loading' && <LoadingState />}
      {state.status === 'error' && (
        <ErrorState
          message={`${state.message} Is the API running and reachable (proxy or VITE_API_BASE_URL)?`}
        />
      )}
      {state.status === 'success' && <ProductsTable products={state.data} />}
    </section>
  )
}
