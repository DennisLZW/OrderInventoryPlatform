import { Navigate, Route, Routes } from 'react-router-dom'
import { AdminLayout } from './layouts/AdminLayout'
import { CreateOrder } from './pages/CreateOrder'
import { Dashboard } from './pages/Dashboard'
import { Inventory } from './pages/Inventory'
import { OrderDetails } from './pages/OrderDetails'
import { Orders } from './pages/Orders'
import { Products } from './pages/Products'
import './App.css'

export default function App() {
  return (
    <Routes>
      <Route element={<AdminLayout />}>
        <Route index element={<Dashboard />} />
        <Route path="products" element={<Products />} />
        <Route path="inventory" element={<Inventory />} />
        <Route path="orders/create" element={<CreateOrder />} />
        <Route path="orders" element={<Orders />} />
        <Route path="orders/:id" element={<OrderDetails />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  )
}
