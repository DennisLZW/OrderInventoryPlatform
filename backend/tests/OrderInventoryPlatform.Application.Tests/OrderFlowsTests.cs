using OrderInventoryPlatform.Application.Interfaces;
using OrderInventoryPlatform.Application.Inventory;
using OrderInventoryPlatform.Application.Orders;
using OrderInventoryPlatform.Application.Services;
using OrderInventoryPlatform.Domain.Catalog;
using OrderInventoryPlatform.Domain.Inventory;
using OrderInventoryPlatform.Domain.Orders;

namespace OrderInventoryPlatform.Application.Tests;

public class OrderFlowsTests
{
    [Fact]
    public async Task PlaceOrder_Succeeds_WhenProductsAndStockAreValid()
    {
        // Arrange
        var keyboard = Product.Create("KB-100", "Keyboard", 99.50m, 5);
        var productRepo = new InMemoryProductRepository(keyboard);
        var inventoryRepo = new InMemoryInventoryItemRepository(
            InventoryItem.CreateForProduct(keyboard.Id, 10));
        var orderWriteRepo = new CapturingOrderWriteRepository();
        var unitOfWork = new CapturingUnitOfWork();

        var sut = new PlaceOrderHandler(productRepo, inventoryRepo, orderWriteRepo, unitOfWork);
        var command = new PlaceOrderCommand([new PlaceOrderLineItem(keyboard.Id, 3)]);

        // Act
        var result = await sut.HandleAsync(command);

        // Assert
        var success = Assert.IsType<PlaceOrderResult.Success>(result);
        Assert.Equal(298.50m, success.Value.TotalAmount);

        var item = inventoryRepo.Items[keyboard.Id];
        Assert.Equal(7, item.AvailableQuantity);

        Assert.Single(orderWriteRepo.AddedOrders);
        Assert.Single(orderWriteRepo.AddedMovements);
        Assert.Equal(InventoryMovementType.Out, orderWriteRepo.AddedMovements[0].Type);
        Assert.Equal(3, orderWriteRepo.AddedMovements[0].Quantity);
        Assert.Equal(success.Value.OrderId, orderWriteRepo.AddedMovements[0].RelatedOrderId);

        Assert.Equal(1, unitOfWork.TransactionCount);
        Assert.Equal(1, unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task PlaceOrder_ReturnsProductNotFound_WhenAnyProductIsMissing()
    {
        // Arrange
        var existing = Product.Create("KB-200", "Keyboard", 20m, 1);
        var missingProductId = Guid.NewGuid();
        var productRepo = new InMemoryProductRepository(existing);
        var inventoryRepo = new InMemoryInventoryItemRepository(
            InventoryItem.CreateForProduct(existing.Id, 5));
        var orderWriteRepo = new CapturingOrderWriteRepository();
        var unitOfWork = new CapturingUnitOfWork();

        var sut = new PlaceOrderHandler(productRepo, inventoryRepo, orderWriteRepo, unitOfWork);
        var command = new PlaceOrderCommand(
        [
            new PlaceOrderLineItem(existing.Id, 1),
            new PlaceOrderLineItem(missingProductId, 1)
        ]);

        // Act
        var result = await sut.HandleAsync(command);

        // Assert
        var failure = Assert.IsType<PlaceOrderResult.Failure>(result);
        Assert.Equal("PRODUCT_NOT_FOUND", failure.Code);
        Assert.Contains(missingProductId.ToString(), failure.Details ?? []);
        Assert.Empty(orderWriteRepo.AddedOrders);
        Assert.Empty(orderWriteRepo.AddedMovements);
        Assert.Equal(0, unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task PlaceOrder_ReturnsInventoryNotConfigured_WhenInventoryItemIsMissing()
    {
        // Arrange
        var keyboard = Product.Create("KB-300", "Keyboard", 20m, 1);
        var productRepo = new InMemoryProductRepository(keyboard);
        var inventoryRepo = new InMemoryInventoryItemRepository();
        var orderWriteRepo = new CapturingOrderWriteRepository();
        var unitOfWork = new CapturingUnitOfWork();

        var sut = new PlaceOrderHandler(productRepo, inventoryRepo, orderWriteRepo, unitOfWork);
        var command = new PlaceOrderCommand([new PlaceOrderLineItem(keyboard.Id, 1)]);

        // Act
        var result = await sut.HandleAsync(command);

        // Assert
        var failure = Assert.IsType<PlaceOrderResult.Failure>(result);
        Assert.Equal("INVENTORY_NOT_CONFIGURED", failure.Code);
        Assert.Contains(keyboard.Id.ToString(), failure.Details ?? []);
        Assert.Empty(orderWriteRepo.AddedOrders);
        Assert.Empty(orderWriteRepo.AddedMovements);
        Assert.Equal(0, unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task PlaceOrder_ReturnsInsufficientStock_WhenRequestedQuantityExceedsAvailable()
    {
        // Arrange
        var keyboard = Product.Create("KB-400", "Keyboard", 20m, 1);
        var productRepo = new InMemoryProductRepository(keyboard);
        var inventoryRepo = new InMemoryInventoryItemRepository(
            InventoryItem.CreateForProduct(keyboard.Id, 2));
        var orderWriteRepo = new CapturingOrderWriteRepository();
        var unitOfWork = new CapturingUnitOfWork();

        var sut = new PlaceOrderHandler(productRepo, inventoryRepo, orderWriteRepo, unitOfWork);
        var command = new PlaceOrderCommand([new PlaceOrderLineItem(keyboard.Id, 3)]);

        // Act
        var result = await sut.HandleAsync(command);

        // Assert
        var failure = Assert.IsType<PlaceOrderResult.Failure>(result);
        Assert.Equal("INSUFFICIENT_STOCK", failure.Code);
        Assert.Empty(orderWriteRepo.AddedOrders);
        Assert.Empty(orderWriteRepo.AddedMovements);
        Assert.Equal(2, inventoryRepo.Items[keyboard.Id].AvailableQuantity);
    }

    [Fact]
    public async Task PlaceOrder_AggregatesDuplicateProductLines_IntoSingleOrderLineAndMovement()
    {
        // Arrange
        var keyboard = Product.Create("KB-500", "Keyboard", 15m, 1);
        var mouse = Product.Create("MS-500", "Mouse", 10m, 1);
        var productRepo = new InMemoryProductRepository(keyboard, mouse);
        var inventoryRepo = new InMemoryInventoryItemRepository(
            InventoryItem.CreateForProduct(keyboard.Id, 20),
            InventoryItem.CreateForProduct(mouse.Id, 20));
        var orderWriteRepo = new CapturingOrderWriteRepository();
        var unitOfWork = new CapturingUnitOfWork();

        var sut = new PlaceOrderHandler(productRepo, inventoryRepo, orderWriteRepo, unitOfWork);
        var command = new PlaceOrderCommand(
        [
            new PlaceOrderLineItem(keyboard.Id, 2),
            new PlaceOrderLineItem(mouse.Id, 1),
            new PlaceOrderLineItem(keyboard.Id, 3)
        ]);

        // Act
        var result = await sut.HandleAsync(command);

        // Assert
        var success = Assert.IsType<PlaceOrderResult.Success>(result);
        Assert.Equal(85m, success.Value.TotalAmount); // Keyboard (5 * 15) + Mouse (1 * 10)

        var order = Assert.Single(orderWriteRepo.AddedOrders);
        Assert.Equal(2, order.Lines.Count);
        Assert.Contains(order.Lines, l => l.ProductId == keyboard.Id && l.Quantity == 5);
        Assert.Contains(order.Lines, l => l.ProductId == mouse.Id && l.Quantity == 1);

        Assert.Equal(2, orderWriteRepo.AddedMovements.Count);
        Assert.Contains(orderWriteRepo.AddedMovements, m => m.ProductId == keyboard.Id && m.Quantity == 5 && m.Type == InventoryMovementType.Out);
        Assert.Contains(orderWriteRepo.AddedMovements, m => m.ProductId == mouse.Id && m.Quantity == 1 && m.Type == InventoryMovementType.Out);

        Assert.Equal(15, inventoryRepo.Items[keyboard.Id].AvailableQuantity);
        Assert.Equal(19, inventoryRepo.Items[mouse.Id].AvailableQuantity);
    }

    [Fact]
    public async Task AdjustInventory_Succeeds_OnExistingInventory_AndCreatesAdjustmentMovement()
    {
        // Arrange
        var keyboard = Product.Create("KB-600", "Keyboard", 20m, 1);
        var productRepo = new InMemoryProductRepository(keyboard);
        var inventoryRepo = new InMemoryInventoryItemRepository(
            InventoryItem.CreateForProduct(keyboard.Id, 10));
        var inventoryWriteRepo = new CapturingInventoryWriteRepository();
        var unitOfWork = new CapturingUnitOfWork();

        var sut = new AdjustInventoryHandler(productRepo, inventoryRepo, inventoryWriteRepo, unitOfWork);
        var command = new AdjustInventoryCommand(keyboard.Id, -4, "Cycle count correction");

        // Act
        var result = await sut.HandleAsync(command);

        // Assert
        var success = Assert.IsType<AdjustInventoryResult.Success>(result);
        Assert.Equal(keyboard.Id, success.Value.ProductId);
        Assert.Equal(6, success.Value.AvailableQuantityAfter);
        Assert.Equal("Cycle count correction", success.Value.Reason);

        Assert.Empty(inventoryWriteRepo.AddedItems);
        var movement = Assert.Single(inventoryWriteRepo.AddedMovements);
        Assert.Equal(InventoryMovementType.Adjustment, movement.Type);
        Assert.Equal(-4, movement.Quantity);
        Assert.Equal("Cycle count correction", movement.Reason);
        Assert.Null(movement.RelatedOrderId);
        Assert.Equal(1, unitOfWork.TransactionCount);
        Assert.Equal(1, unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task AdjustInventory_CreatesInventory_WhenPositiveAdjustmentAndInventoryMissing()
    {
        // Arrange
        var keyboard = Product.Create("KB-700", "Keyboard", 20m, 1);
        var productRepo = new InMemoryProductRepository(keyboard);
        var inventoryRepo = new InMemoryInventoryItemRepository();
        var inventoryWriteRepo = new CapturingInventoryWriteRepository();
        var unitOfWork = new CapturingUnitOfWork();

        var sut = new AdjustInventoryHandler(productRepo, inventoryRepo, inventoryWriteRepo, unitOfWork);
        var command = new AdjustInventoryCommand(keyboard.Id, 8, "Initial stock load");

        // Act
        var result = await sut.HandleAsync(command);

        // Assert
        var success = Assert.IsType<AdjustInventoryResult.Success>(result);
        Assert.Equal(8, success.Value.AvailableQuantityAfter);

        var createdItem = Assert.Single(inventoryWriteRepo.AddedItems);
        Assert.Equal(keyboard.Id, createdItem.ProductId);
        Assert.Equal(8, createdItem.AvailableQuantity);

        var movement = Assert.Single(inventoryWriteRepo.AddedMovements);
        Assert.Equal(InventoryMovementType.Adjustment, movement.Type);
        Assert.Equal(8, movement.Quantity);
        Assert.Equal("Initial stock load", movement.Reason);
    }

    [Fact]
    public async Task AdjustInventory_RejectsNegativeDelta_WhenInventoryDoesNotExist()
    {
        // Arrange
        var keyboard = Product.Create("KB-800", "Keyboard", 20m, 1);
        var productRepo = new InMemoryProductRepository(keyboard);
        var inventoryRepo = new InMemoryInventoryItemRepository();
        var inventoryWriteRepo = new CapturingInventoryWriteRepository();
        var unitOfWork = new CapturingUnitOfWork();

        var sut = new AdjustInventoryHandler(productRepo, inventoryRepo, inventoryWriteRepo, unitOfWork);
        var command = new AdjustInventoryCommand(keyboard.Id, -1, "Shrinkage");

        // Act
        var result = await sut.HandleAsync(command);

        // Assert
        var failure = Assert.IsType<AdjustInventoryResult.Failure>(result);
        Assert.Equal("NEGATIVE_ADJUSTMENT_WITHOUT_INVENTORY", failure.Code);
        Assert.Empty(inventoryWriteRepo.AddedItems);
        Assert.Empty(inventoryWriteRepo.AddedMovements);
        Assert.Equal(0, unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task AdjustInventory_RejectsAdjustment_WhenItWouldMakeInventoryNegative()
    {
        // Arrange
        var keyboard = Product.Create("KB-900", "Keyboard", 20m, 1);
        var productRepo = new InMemoryProductRepository(keyboard);
        var inventoryRepo = new InMemoryInventoryItemRepository(
            InventoryItem.CreateForProduct(keyboard.Id, 2));
        var inventoryWriteRepo = new CapturingInventoryWriteRepository();
        var unitOfWork = new CapturingUnitOfWork();

        var sut = new AdjustInventoryHandler(productRepo, inventoryRepo, inventoryWriteRepo, unitOfWork);
        var command = new AdjustInventoryCommand(keyboard.Id, -3, "Adjustment too large");

        // Act
        var result = await sut.HandleAsync(command);

        // Assert
        var failure = Assert.IsType<AdjustInventoryResult.Failure>(result);
        Assert.Equal("ADJUSTMENT_WOULD_GO_NEGATIVE", failure.Code);
        Assert.Equal(2, inventoryRepo.Items[keyboard.Id].AvailableQuantity);
        Assert.Empty(inventoryWriteRepo.AddedItems);
        Assert.Empty(inventoryWriteRepo.AddedMovements);
        Assert.Equal(0, unitOfWork.SaveChangesCount);
    }

    private sealed class InMemoryProductRepository(params Product[] products) : IProductRepository
    {
        private readonly Dictionary<Guid, Product> _products = products.ToDictionary(x => x.Id);

        public Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Product>>(_products.Values.ToArray());

        public Task<IReadOnlyDictionary<Guid, Product>> GetByIdsAsync(
            IReadOnlyCollection<Guid> productIds,
            CancellationToken cancellationToken = default)
        {
            var map = _products
                .Where(kvp => productIds.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return Task.FromResult<IReadOnlyDictionary<Guid, Product>>(map);
        }
    }

    private sealed class InMemoryInventoryItemRepository(params InventoryItem[] items) : IInventoryItemRepository
    {
        public Dictionary<Guid, InventoryItem> Items { get; } = items.ToDictionary(x => x.ProductId);

        public Task<IReadOnlyDictionary<Guid, InventoryItem>> GetTrackedByProductIdsAsync(
            IReadOnlyCollection<Guid> productIds,
            CancellationToken cancellationToken = default)
        {
            var map = Items
                .Where(kvp => productIds.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return Task.FromResult<IReadOnlyDictionary<Guid, InventoryItem>>(map);
        }
    }

    private sealed class CapturingOrderWriteRepository : IOrderWriteRepository
    {
        public List<Order> AddedOrders { get; } = [];
        public List<InventoryMovement> AddedMovements { get; } = [];

        public void AddOrder(Order order) => AddedOrders.Add(order);

        public void AddMovements(IEnumerable<InventoryMovement> movements) =>
            AddedMovements.AddRange(movements);
    }

    private sealed class CapturingInventoryWriteRepository : IInventoryWriteRepository
    {
        public List<InventoryItem> AddedItems { get; } = [];
        public List<InventoryMovement> AddedMovements { get; } = [];

        public void AddInventoryItem(InventoryItem item) => AddedItems.Add(item);

        public void AddMovement(InventoryMovement movement) => AddedMovements.Add(movement);
    }

    private sealed class CapturingUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCount { get; private set; }
        public int TransactionCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCount++;
            return Task.FromResult(1);
        }

        public async Task ExecuteInTransactionAsync(
            Func<Task> action,
            CancellationToken cancellationToken = default)
        {
            TransactionCount++;
            await action();
        }
    }
}
