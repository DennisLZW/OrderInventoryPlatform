using OrderInventoryPlatform.Application.Interfaces;
using OrderInventoryPlatform.Application.Orders;
using OrderInventoryPlatform.Domain.Inventory;
using OrderInventoryPlatform.Domain.Orders;

namespace OrderInventoryPlatform.Application.Services;

public sealed class PlaceOrderHandler(
    IProductRepository productRepository,
    IInventoryItemRepository inventoryItemRepository,
    IOrderWriteRepository orderWriteRepository,
    IUnitOfWork unitOfWork) : IPlaceOrderHandler
{
    public async Task<PlaceOrderResult> HandleAsync(PlaceOrderCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Lines.Count == 0)
        {
            return new PlaceOrderResult.Failure(
                "ORDER_LINES_EMPTY",
                "At least one order line is required.");
        }

        foreach (var line in command.Lines)
        {
            if (line.ProductId == Guid.Empty)
            {
                return new PlaceOrderResult.Failure(
                    "INVALID_PRODUCT_ID",
                    "Product id must be a non-empty GUID.");
            }

            if (line.Quantity <= 0)
            {
                return new PlaceOrderResult.Failure(
                    "INVALID_QUANTITY",
                    $"Quantity must be greater than zero (product {line.ProductId}).");
            }
        }

        var quantitiesByProduct = AggregateQuantities(command.Lines);
        var productIds = quantitiesByProduct.Keys.ToArray();

        var products = await productRepository.GetByIdsAsync(productIds, cancellationToken);
        var missingProducts = productIds.Where(id => !products.ContainsKey(id)).ToArray();
        if (missingProducts.Length > 0)
        {
            return new PlaceOrderResult.Failure(
                "PRODUCT_NOT_FOUND",
                "One or more products do not exist.",
                missingProducts.Select(id => id.ToString()).ToArray());
        }

        var inventoryItems = await inventoryItemRepository.GetTrackedByProductIdsAsync(productIds, cancellationToken);
        var missingInventory = productIds.Where(id => !inventoryItems.ContainsKey(id)).ToArray();
        if (missingInventory.Length > 0)
        {
            return new PlaceOrderResult.Failure(
                "INVENTORY_NOT_CONFIGURED",
                "Inventory is not configured for one or more products.",
                missingInventory.Select(id => id.ToString()).ToArray());
        }

        var insufficient = new List<string>();
        foreach (var (productId, qty) in quantitiesByProduct)
        {
            var item = inventoryItems[productId];
            if (!item.CanDecrease(qty))
            {
                insufficient.Add(
                    $"Product {productId}: requested {qty}, available {item.AvailableQuantity}.");
            }
        }

        if (insufficient.Count > 0)
        {
            return new PlaceOrderResult.Failure(
                "INSUFFICIENT_STOCK",
                "Not enough stock to fulfill this order.",
                insufficient);
        }

        var order = Order.Create();

        foreach (var (productId, qty) in quantitiesByProduct)
        {
            var product = products[productId];
            order.AddLine(productId, qty, product.Price);
        }

        var movements = new List<InventoryMovement>();

        foreach (var (productId, qty) in quantitiesByProduct)
        {
            var item = inventoryItems[productId];
            item.DecreaseAvailable(qty);
            movements.Add(InventoryMovement.CreateOutbound(productId, qty, order.Id));
        }

        order.MarkAsPlaced();

        // Single SaveChangesAsync inside one explicit DB transaction: new Order + OrderLines,
        // updated InventoryItem rows (tracked), and InventoryMovement rows — all or nothing.
        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            orderWriteRepository.AddOrder(order);
            orderWriteRepository.AddMovements(movements);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }, cancellationToken);

        return new PlaceOrderResult.Success(new PlaceOrderResponse(order.Id, order.TotalAmount));
    }

    private static Dictionary<Guid, int> AggregateQuantities(IReadOnlyList<PlaceOrderLineItem> lines)
    {
        var map = new Dictionary<Guid, int>();
        foreach (var line in lines)
        {
            if (map.TryGetValue(line.ProductId, out var current))
            {
                map[line.ProductId] = current + line.Quantity;
            }
            else
            {
                map[line.ProductId] = line.Quantity;
            }
        }

        return map;
    }
}
