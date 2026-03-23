import type { InventoryQueryDto } from '../../types/queryDtos'

type Props = {
  rows: InventoryQueryDto[]
}

export function InventoryTable({ rows }: Props) {
  if (rows.length === 0) {
    return <p className="admin-muted">No inventory rows found.</p>
  }

  return (
    <div className="admin-table-wrap">
      <table className="admin-table">
        <thead>
          <tr>
            <th scope="col">Product</th>
            <th scope="col">SKU</th>
            <th scope="col">Available</th>
            <th scope="col">Reorder Level</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr key={row.productId}>
              <td>{row.productName}</td>
              <td>
                <code className="admin-code">{row.sku}</code>
              </td>
              <td>{row.availableQuantity}</td>
              <td>{row.reorderThreshold}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
