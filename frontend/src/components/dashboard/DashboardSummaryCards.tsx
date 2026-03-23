export type DashboardSummary = {
  productCount: number
  inventoryItemCount: number
  orderCount: number
  lowStockCount: number
}

type Props = {
  summary: DashboardSummary
}

const cards: {
  key: keyof DashboardSummary
  label: string
  alert?: boolean
}[] = [
  { key: 'productCount', label: 'Products' },
  { key: 'inventoryItemCount', label: 'Inventory' },
  { key: 'orderCount', label: 'Orders' },
  { key: 'lowStockCount', label: 'Low Stock', alert: true },
]

export function DashboardSummaryCards({ summary }: Props) {
  return (
    <div className="admin-dashboard__grid">
      {cards.map(({ key, label, alert }) => {
        const isLowStock = key === 'lowStockCount'
        return (
          <div
            key={key}
            className={
              alert
                ? 'admin-stat-card admin-stat-card--alert'
                : 'admin-stat-card'
            }
          >
            <div className="admin-stat-card__label">{label}</div>
            <div className="admin-stat-card__value">{summary[key]}</div>
            {isLowStock && (
              <p className="admin-stat-card__hint">
                {summary.lowStockCount === 0
                  ? 'All stock levels are healthy'
                  : 'Items at or below reorder threshold'}
              </p>
            )}
          </div>
        )
      })}
    </div>
  )
}
