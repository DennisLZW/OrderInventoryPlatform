import { NavLink, Outlet } from 'react-router-dom'

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  `admin-sidebar__link${isActive ? ' admin-sidebar__link--active' : ''}`

const navItems = [
  { to: '/', label: 'Dashboard', end: true },
  { to: '/products', label: 'Products', end: false },
  { to: '/inventory', label: 'Inventory', end: false },
  { to: '/orders/create', label: 'Create Order', end: false },
  { to: '/orders', label: 'Orders', end: true },
] as const

export function AdminLayout() {
  return (
    <div className="admin-app">
      <aside className="admin-sidebar" aria-label="Main navigation">
        <div className="admin-sidebar__brand">Order Admin</div>
        <nav className="admin-sidebar__nav">
          {navItems.map(({ to, label, end }) => (
            <NavLink key={to} to={to} end={end} className={navLinkClass}>
              {label}
            </NavLink>
          ))}
        </nav>
      </aside>
      <div className="admin-main">
        <Outlet />
      </div>
    </div>
  )
}
