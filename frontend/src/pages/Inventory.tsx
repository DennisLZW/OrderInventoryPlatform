import { useCallback, useEffect, useState } from 'react'
import { fetchInventory } from '../api/inventory'
import { ErrorState } from '../components/admin/ErrorState'
import { LoadingState } from '../components/admin/LoadingState'
import { InventoryAdjustmentForm } from '../components/inventory/InventoryAdjustmentForm'
import { InventoryTable } from '../components/inventory/InventoryTable'
import type { InventoryQueryDto } from '../types/queryDtos'
import { getRequestErrorMessage } from '../utils/axiosError'

type ListState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'success'; data: InventoryQueryDto[] }

export function Inventory() {
  const [state, setState] = useState<ListState>({ status: 'loading' })
  const [refreshing, setRefreshing] = useState(false)

  const loadInventory = useCallback(async () => {
    const data = await fetchInventory()
    setState({ status: 'success', data })
  }, [])

  useEffect(() => {
    let cancelled = false

    loadInventory().catch((err: unknown) => {
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
  }, [loadInventory])

  const refreshAfterAdjustment = useCallback(async () => {
    setRefreshing(true)
    try {
      await loadInventory()
    } catch (err: unknown) {
      setState({
        status: 'error',
        message: getRequestErrorMessage(err),
      })
    } finally {
      setRefreshing(false)
    }
  }, [loadInventory])

  return (
    <section className="page">
      <h1 className="page__title">Inventory</h1>
      <p className="page__lead">
        Monitor stock levels and manage inventory adjustments.
      </p>

      {state.status === 'loading' && <LoadingState />}
      {state.status === 'error' && (
        <ErrorState
          message={`${state.message} Is the API running and reachable (proxy or VITE_API_BASE_URL)?`}
        />
      )}
      {state.status === 'success' && (
        <>
          <InventoryAdjustmentForm
            rows={state.data}
            onAdjusted={refreshAfterAdjustment}
          />
          {refreshing && (
            <p className="admin-muted" role="status">
              Refreshing inventory…
            </p>
          )}
          <InventoryTable rows={state.data} />
        </>
      )}
    </section>
  )
}
