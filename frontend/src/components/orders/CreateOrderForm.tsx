import { useCallback, useState, type FormEvent } from 'react'
import { postPlaceOrder } from '../../api/orders'
import { ErrorState } from '../admin/ErrorState'
import type { PlaceOrderResponse, ProductQueryDto } from '../../types/queryDtos'
import { getRequestErrorMessage } from '../../utils/axiosError'

type LineRow = {
  key: string
  productId: string
  quantity: string
}

function newLine(): LineRow {
  return {
    key: crypto.randomUUID(),
    productId: '',
    quantity: '',
  }
}

type Props = {
  products: ProductQueryDto[]
}

const money = new Intl.NumberFormat(undefined, {
  style: 'currency',
  currency: 'USD',
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

type SubmitState =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'success'; result: PlaceOrderResponse }
  | { status: 'error'; message: string }

const VALIDATION_MESSAGE =
  'Please select a product and enter a valid quantity for each item.'

function getPlaceOrderErrorMessage(err: unknown): string {
  const msg = getRequestErrorMessage(err)
  if (
    !msg ||
    msg === 'An unexpected error occurred.' ||
    msg === 'Network Error'
  ) {
    return 'Unable to place order. Please check your inputs.'
  }
  return msg
}

export function CreateOrderForm({ products }: Props) {
  const [lines, setLines] = useState<LineRow[]>(() => [newLine()])
  const [submitState, setSubmitState] = useState<SubmitState>({ status: 'idle' })
  const [validationError, setValidationError] = useState<string | null>(null)

  const updateLine = useCallback(
    (key: string, patch: Partial<Pick<LineRow, 'productId' | 'quantity'>>) => {
      setLines((prev) =>
        prev.map((row) => (row.key === key ? { ...row, ...patch } : row)),
      )
    },
    [],
  )

  const addLine = useCallback(() => {
    setLines((prev) => [...prev, newLine()])
  }, [])

  const removeLine = useCallback((key: string) => {
    setLines((prev) => (prev.length <= 1 ? prev : prev.filter((r) => r.key !== key)))
  }, [])

  function validate(): boolean {
    if (lines.length === 0) return false
    for (const row of lines) {
      if (!row.productId) return false
      const q = Number.parseInt(row.quantity, 10)
      if (!Number.isFinite(q) || q <= 0) return false
    }
    return true
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setValidationError(null)

    if (!validate()) {
      setValidationError(VALIDATION_MESSAGE)
      setSubmitState({ status: 'idle' })
      return
    }

    const payload = {
      lines: lines.map((row) => ({
        productId: row.productId,
        quantity: Number.parseInt(row.quantity, 10),
      })),
    }

    setSubmitState({ status: 'loading' })
    try {
      const result = await postPlaceOrder(payload)
      setSubmitState({ status: 'success', result })
      setLines([newLine()])
    } catch (unknownErr: unknown) {
      setSubmitState({
        status: 'error',
        message: getPlaceOrderErrorMessage(unknownErr),
      })
    }
  }

  if (products.length === 0) {
    return (
      <div className="admin-panel admin-panel--muted">
        <p className="admin-muted">
          No products available. Please add products before creating an order.
        </p>
      </div>
    )
  }

  return (
    <div className="admin-panel">
      <h2 className="admin-panel__title">Order lines</h2>
      <p className="admin-panel__lead">
        Add one or more items. Duplicate products on separate lines are combined
        when the order is placed.
      </p>

      <form className="admin-form" onSubmit={handleSubmit}>
        <div className="admin-order-lines">
          {lines.map((row, index) => (
            <div key={row.key} className="admin-order-line">
              <div className="admin-form__row admin-order-line__field">
                <label className="admin-label" htmlFor={`line-${row.key}-product`}>
                  Product
                </label>
                <select
                  id={`line-${row.key}-product`}
                  className="admin-input"
                  value={row.productId}
                  onChange={(e) =>
                    updateLine(row.key, { productId: e.target.value })
                  }
                  disabled={submitState.status === 'loading'}
                >
                  <option value="">Select a product</option>
                  {products.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.name} ({p.sku}) — {money.format(p.price)}
                    </option>
                  ))}
                </select>
              </div>
              <div className="admin-form__row admin-order-line__field admin-order-line__field--qty">
                <label className="admin-label" htmlFor={`line-${row.key}-qty`}>
                  Quantity
                </label>
                <input
                  id={`line-${row.key}-qty`}
                  className="admin-input admin-input--narrow"
                  type="number"
                  inputMode="numeric"
                  min={1}
                  step={1}
                  placeholder="Enter quantity"
                  value={row.quantity}
                  onChange={(e) =>
                    updateLine(row.key, { quantity: e.target.value })
                  }
                  disabled={submitState.status === 'loading'}
                />
              </div>
              <div className="admin-order-line__actions">
                <button
                  type="button"
                  className="admin-button admin-button--secondary"
                  onClick={() => removeLine(row.key)}
                  disabled={submitState.status === 'loading' || lines.length <= 1}
                  aria-label={`Remove item ${index + 1}`}
                >
                  Remove
                </button>
              </div>
            </div>
          ))}
        </div>

        <div className="admin-form__row admin-form__row--inline">
          <button
            type="button"
            className="admin-button admin-button--secondary"
            onClick={addLine}
            disabled={submitState.status === 'loading'}
          >
            Add Item
          </button>
        </div>

        {validationError && (
          <div className="admin-fetch-state admin-fetch-state--error" role="alert">
            <p>{validationError}</p>
          </div>
        )}

        <div className="admin-form__actions">
          <button
            type="submit"
            className="admin-button"
            disabled={submitState.status === 'loading'}
          >
            {submitState.status === 'loading' ? 'Placing order…' : 'Place Order'}
          </button>
        </div>
      </form>

      {submitState.status === 'success' && (
        <div className="admin-fetch-state admin-fetch-state--success" role="status">
          <p className="admin-order-success__lead">
            <strong>Order placed successfully.</strong>
          </p>
          <p className="admin-order-success__detail">
            Order ID{' '}
            <code className="admin-code">{submitState.result.orderId}</code>
          </p>
          <p className="admin-order-success__detail">
            Total amount {money.format(submitState.result.totalAmount)}
          </p>
        </div>
      )}
      {submitState.status === 'error' && (
        <ErrorState title="Order failed" message={submitState.message} />
      )}
    </div>
  )
}
