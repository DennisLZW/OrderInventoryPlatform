import { useMemo, useState, type FormEvent } from 'react'
import { postInventoryAdjustment } from '../../api/inventory'
import { ErrorState } from '../admin/ErrorState'
import type { AdjustInventoryResponse, InventoryQueryDto } from '../../types/queryDtos'
import { getRequestErrorMessage } from '../../utils/axiosError'

type Props = {
  rows: InventoryQueryDto[]
  onAdjusted: () => Promise<void>
}

type SubmitState =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'success'; result: AdjustInventoryResponse }
  | { status: 'error'; message: string }

export function InventoryAdjustmentForm({ rows, onAdjusted }: Props) {
  const [productId, setProductId] = useState('')
  const [quantityDelta, setQuantityDelta] = useState('')
  const [reason, setReason] = useState('')
  const [submitState, setSubmitState] = useState<SubmitState>({ status: 'idle' })

  const effectiveProductId = useMemo(() => {
    if (rows.length === 0) return ''
    if (productId && rows.some((r) => r.productId === productId)) return productId
    return ''
  }, [rows, productId])

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    const delta = Number.parseInt(quantityDelta, 10)
    const trimmedReason = reason.trim()

    if (!effectiveProductId) {
      setSubmitState({ status: 'error', message: 'Select a product.' })
      return
    }
    if (!Number.isFinite(delta) || delta === 0) {
      setSubmitState({
        status: 'error',
        message: 'Quantity change must be a non-zero whole number.',
      })
      return
    }
    if (!trimmedReason) {
      setSubmitState({ status: 'error', message: 'Reason is required.' })
      return
    }

    setSubmitState({ status: 'loading' })
    try {
      const result = await postInventoryAdjustment({
        productId: effectiveProductId,
        quantityDelta: delta,
        reason: trimmedReason,
      })
      setSubmitState({ status: 'success', result })
      setQuantityDelta('')
      setReason('')
      await onAdjusted()
    } catch (err: unknown) {
      setSubmitState({
        status: 'error',
        message: getRequestErrorMessage(err),
      })
    }
  }

  if (rows.length === 0) {
    return (
      <div className="admin-panel admin-panel--muted">
        <h2 className="admin-panel__title">Adjust Inventory</h2>
        <p className="admin-muted">
          No inventory rows loaded. Add stock via the API or seed data, then
          refresh the page.
        </p>
      </div>
    )
  }

  return (
    <div className="admin-panel">
      <h2 className="admin-panel__title">Adjust Inventory</h2>
      <p className="admin-panel__lead">
        Update stock levels and record a reason for the change.
      </p>

      <form className="admin-form" onSubmit={handleSubmit}>
        <div className="admin-form__row">
          <label className="admin-label" htmlFor="adj-product">
            Product
          </label>
          <select
            id="adj-product"
            className="admin-input"
            value={effectiveProductId}
            onChange={(e) => setProductId(e.target.value)}
            disabled={submitState.status === 'loading'}
          >
            <option value="">Select a product</option>
            {rows.map((r) => (
              <option key={r.productId} value={r.productId}>
                {r.productName} ({r.sku}) — qty {r.availableQuantity}
              </option>
            ))}
          </select>
        </div>

        <div className="admin-form__row">
          <label className="admin-label" htmlFor="adj-delta">
            Quantity Change
          </label>
          <input
            id="adj-delta"
            className="admin-input admin-input--narrow"
            type="number"
            inputMode="numeric"
            step={1}
            placeholder="Enter quantity change (e.g. +10 or -5)"
            value={quantityDelta}
            onChange={(e) => setQuantityDelta(e.target.value)}
            disabled={submitState.status === 'loading'}
          />
        </div>

        <div className="admin-form__row">
          <label className="admin-label" htmlFor="adj-reason">
            Reason
          </label>
          <input
            id="adj-reason"
            className="admin-input"
            type="text"
            autoComplete="off"
            placeholder="Enter a reason for this adjustment"
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            disabled={submitState.status === 'loading'}
          />
        </div>

        <div className="admin-form__actions">
          <button
            type="submit"
            className="admin-button"
            disabled={submitState.status === 'loading'}
          >
            {submitState.status === 'loading' ? 'Applying…' : 'Apply adjustment'}
          </button>
        </div>
      </form>

      {submitState.status === 'success' && (
        <div className="admin-fetch-state admin-fetch-state--success" role="status">
          <strong>Inventory updated successfully.</strong>
        </div>
      )}
      {submitState.status === 'error' && (
        <ErrorState title="Adjustment failed" message={submitState.message} />
      )}
    </div>
  )
}
