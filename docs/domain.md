# Domain Design - Order & Inventory Platform

## 1. Inventory Source of Truth

Inventory is managed exclusively in the Inventory module.

- InventoryItem stores current available quantity per product
- InventoryMovement stores all stock changes (IN, OUT, ADJUSTMENT)

Catalog module does NOT store stock as source of truth.

Reason:
To avoid double-write issues and ensure a single source of truth.

---

## 2. Pricing Strategy

OrderLine stores unit price snapshot at the time of order creation.

- UnitPrice is copied from Product at order time
- Future product price changes do NOT affect existing orders

Reason:
To preserve historical accuracy of orders.

---

## 3. Domain Modules

### Catalog

- Product
  - Id
  - SKU (unique)
  - Name
  - Price
  - ReorderThreshold

---

### Orders

- Order
- OrderLine
  - ProductId
  - Quantity
  - UnitPrice (snapshot)
  - LineTotal

---

### Inventory

- InventoryItem
  - ProductId
  - AvailableQuantity

- InventoryMovement
  - Id
  - ProductId
  - Quantity
  - Type (IN / OUT / ADJUSTMENT)
  - RelatedOrderId (optional)
  - Reason (required for ADJUSTMENT; empty for IN/OUT)
  - CreatedAt

---

## 4. Domain Events (conceptual for now)

- OrderCreated
- StockDeducted
- LowStockDetected

Events are defined conceptually first and implemented later.

---

## 5. Core Use Case: Place Order

Flow:

1. Validate products exist
2. Validate quantities > 0
3. Check inventory availability
4. If insufficient → reject order
5. If sufficient:
   - Create Order + OrderLines
   - Deduct inventory
   - Create InventoryMovement records
6. Save all changes in a single transaction

---

## 6. Core Use Case: Adjust Inventory

API: `POST /api/inventory/adjustments` with `productId`, `quantityDelta` (signed), `reason`.

Flow:

1. Validate product exists
2. Validate non-zero delta and non-empty reason
3. If no `InventoryItem` exists: create one with `AvailableQuantity = quantityDelta` (rejects negative delta)
4. If it exists: apply signed delta via domain rules (cannot go below zero)
5. Create `InventoryMovement` of type ADJUSTMENT with the same signed quantity and persisted reason
6. Save inventory + movement in one transaction
