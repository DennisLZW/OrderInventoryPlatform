import type { ProductQueryDto } from '../../types/queryDtos'

type Props = {
  products: ProductQueryDto[]
}

const money = new Intl.NumberFormat(undefined, {
  style: 'currency',
  currency: 'USD',
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

export function ProductsTable({ products }: Props) {
  if (products.length === 0) {
    return <p className="admin-muted">No products found.</p>
  }

  return (
    <div className="admin-table-wrap">
      <table className="admin-table">
        <thead>
          <tr>
            <th scope="col">Name</th>
            <th scope="col">SKU</th>
            <th scope="col">Price</th>
            <th scope="col">Reorder threshold</th>
          </tr>
        </thead>
        <tbody>
          {products.map((p) => (
            <tr key={p.id}>
              <td>{p.name}</td>
              <td>
                <code className="admin-code">{p.sku}</code>
              </td>
              <td>{money.format(p.price)}</td>
              <td>{p.reorderThreshold}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
