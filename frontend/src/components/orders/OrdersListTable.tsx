import { Link } from 'react-router-dom'
import type { OrderListItemQueryDto } from '../../types/queryDtos'
import { formatMoney } from '../../utils/formatMoney'

type Props = {
  orders: OrderListItemQueryDto[]
}

const dateTime = new Intl.DateTimeFormat(undefined, {
  dateStyle: 'medium',
  timeStyle: 'short',
})

export function OrdersListTable({ orders }: Props) {
  if (orders.length === 0) {
    return (
      <p className="admin-muted" role="status">
        No orders yet.
      </p>
    )
  }

  return (
    <div className="admin-table-wrap">
      <table className="admin-table">
        <thead>
          <tr>
            <th scope="col">Order ID</th>
            <th scope="col">Created</th>
            <th scope="col">Total</th>
            <th scope="col">Status</th>
          </tr>
        </thead>
        <tbody>
          {orders.map((o) => (
            <tr key={o.id}>
              <td>
                <Link to={`/orders/${o.id}`} className="admin-table__link">
                  <code className="admin-code">{o.id}</code>
                </Link>
              </td>
              <td>{dateTime.format(new Date(o.createdAt))}</td>
              <td>{formatMoney(o.totalAmount)}</td>
              <td>{o.status}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
