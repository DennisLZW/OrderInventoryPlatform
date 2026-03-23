import axios from 'axios'
import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { fetchOrderById } from '../api/orders'
import { ErrorState } from '../components/admin/ErrorState'
import { LoadingState } from '../components/admin/LoadingState'
import { OrderLinesTable } from '../components/orders/OrderLinesTable'
import type { OrderDetailsQueryDto } from '../types/queryDtos'
import { formatMoney } from '../utils/formatMoney'
import { getRequestErrorMessage } from '../utils/axiosError'

type LoadState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'notFound' }
  | { status: 'success'; data: OrderDetailsQueryDto }

const dateTime = new Intl.DateTimeFormat(undefined, {
  dateStyle: 'medium',
  timeStyle: 'short',
})

type ContentProps = { id: string }

/** Remounted when `id` changes (`key={id}`) so load state resets without setState in an effect. */
function OrderDetailsContent({ id }: ContentProps) {
  const [state, setState] = useState<LoadState>({ status: 'loading' })

  useEffect(() => {
    let cancelled = false

    fetchOrderById(id)
      .then((data) => {
        if (!cancelled) setState({ status: 'success', data })
      })
      .catch((err: unknown) => {
        if (cancelled) return
        if (axios.isAxiosError(err) && err.response?.status === 404) {
          setState({ status: 'notFound' })
          return
        }
        setState({
          status: 'error',
          message: getRequestErrorMessage(err),
        })
      })

    return () => {
      cancelled = true
    }
  }, [id])

  return (
    <>
      {state.status === 'loading' && <LoadingState />}
      {state.status === 'error' && (
        <ErrorState
          message={`${state.message} Is the API running and reachable (proxy or VITE_API_BASE_URL)?`}
        />
      )}
      {state.status === 'notFound' && (
        <div className="admin-fetch-state admin-fetch-state--error" role="status">
          <strong>Not found</strong>
          <p>
            No order exists for this id.{' '}
            <Link to="/orders">Back to Orders</Link>
          </p>
        </div>
      )}
      {state.status === 'success' && (
        <>
          <div className="admin-order-summary">
            <div className="admin-order-summary__grid">
              <div>
                <span className="admin-order-summary__label">Order ID</span>
                <p className="admin-order-summary__value">
                  <code className="admin-code">{state.data.id}</code>
                </p>
              </div>
              <div>
                <span className="admin-order-summary__label">Created</span>
                <p className="admin-order-summary__value">
                  {dateTime.format(new Date(state.data.createdAt))}
                </p>
              </div>
              <div>
                <span className="admin-order-summary__label">Total</span>
                <p className="admin-order-summary__value">
                  {formatMoney(state.data.totalAmount)}
                </p>
              </div>
              <div>
                <span className="admin-order-summary__label">Status</span>
                <p className="admin-order-summary__value">{state.data.status}</p>
              </div>
            </div>
          </div>

          <h2 className="page__subtitle">Lines</h2>
          <OrderLinesTable lines={state.data.lines} />
        </>
      )}
    </>
  )
}

export function OrderDetails() {
  const { id } = useParams<{ id: string }>()

  return (
    <section className="page">
      <p className="page__back">
        <Link to="/orders">Back to Orders</Link>
      </p>

      <h1 className="page__title">Order Details</h1>
      <p className="page__lead">
        Review order information and line items.
      </p>

      {!id ? (
        <ErrorState message="Missing order id in the URL." />
      ) : (
        <OrderDetailsContent key={id} id={id} />
      )}
    </section>
  )
}
