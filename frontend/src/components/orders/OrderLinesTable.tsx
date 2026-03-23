import type { OrderLineQueryDto } from '../../types/queryDtos'
import { formatMoney } from '../../utils/formatMoney'

type Props = {
  lines: OrderLineQueryDto[]
}

export function OrderLinesTable({ lines }: Props) {
  if (lines.length === 0) {
    return <p className="admin-muted">No lines on this order.</p>
  }

  return (
    <div className="admin-table-wrap">
      <table className="admin-table">
        <thead>
          <tr>
            <th scope="col">Product</th>
            <th scope="col">SKU</th>
            <th scope="col">Quantity</th>
            <th scope="col">Unit Price</th>
            <th scope="col">Line Total</th>
          </tr>
        </thead>
        <tbody>
          {lines.map((line, index) => (
            <tr key={`${line.productId}-${index}`}>
              <td>{line.productName}</td>
              <td>
                <code className="admin-code">{line.sku}</code>
              </td>
              <td>{line.quantity}</td>
              <td>{formatMoney(line.unitPrice)}</td>
              <td>{formatMoney(line.lineTotal)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
