using OrderInventoryPlatform.Application.Interfaces;
using OrderInventoryPlatform.Application.Inventory;
using OrderInventoryPlatform.Domain.Common;
using OrderInventoryPlatform.Domain.Inventory;

namespace OrderInventoryPlatform.Application.Services;

public sealed class AdjustInventoryHandler(
    IProductRepository productRepository,
    IInventoryItemRepository inventoryItemRepository,
    IInventoryWriteRepository inventoryWriteRepository,
    IUnitOfWork unitOfWork) : IAdjustInventoryHandler
{
    public async Task<AdjustInventoryResult> HandleAsync(
        AdjustInventoryCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.ProductId == Guid.Empty)
        {
            return new AdjustInventoryResult.Failure(
                "INVALID_PRODUCT_ID",
                "Product id must be a non-empty GUID.");
        }

        if (command.QuantityDelta == 0)
        {
            return new AdjustInventoryResult.Failure(
                "INVALID_QUANTITY_DELTA",
                "Quantity delta must be non-zero.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            return new AdjustInventoryResult.Failure(
                "INVALID_REASON",
                "Reason is required.");
        }

        var trimmedReason = command.Reason.Trim();
        if (trimmedReason.Length > InventoryMovement.ReasonMaxLength)
        {
            return new AdjustInventoryResult.Failure(
                "REASON_TOO_LONG",
                $"Reason cannot exceed {InventoryMovement.ReasonMaxLength} characters.");
        }

        var products = await productRepository.GetByIdsAsync(
            new[] { command.ProductId },
            cancellationToken);

        if (!products.ContainsKey(command.ProductId))
        {
            return new AdjustInventoryResult.Failure(
                "PRODUCT_NOT_FOUND",
                "The product does not exist.");
        }

        var inventoryByProduct = await inventoryItemRepository.GetTrackedByProductIdsAsync(
            new[] { command.ProductId },
            cancellationToken);

        InventoryItem item;
        var isNew = !inventoryByProduct.TryGetValue(command.ProductId, out var existing);

        if (isNew)
        {
            if (command.QuantityDelta < 0)
            {
                return new AdjustInventoryResult.Failure(
                    "NEGATIVE_ADJUSTMENT_WITHOUT_INVENTORY",
                    "Cannot apply a negative adjustment when no inventory row exists for this product.");
            }

            item = InventoryItem.CreateForProduct(command.ProductId, command.QuantityDelta);
        }
        else
        {
            item = existing!;
            try
            {
                item.ApplyAdjustment(command.QuantityDelta);
            }
            catch (DomainException ex)
            {
                return new AdjustInventoryResult.Failure(
                    "ADJUSTMENT_WOULD_GO_NEGATIVE",
                    ex.Message);
            }
        }

        InventoryMovement movement;
        try
        {
            movement = InventoryMovement.CreateAdjustment(
                command.ProductId,
                command.QuantityDelta,
                trimmedReason);
        }
        catch (DomainException ex)
        {
            return new AdjustInventoryResult.Failure(
                "INVALID_MOVEMENT",
                ex.Message);
        }

        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            if (isNew)
            {
                inventoryWriteRepository.AddInventoryItem(item);
            }

            inventoryWriteRepository.AddMovement(movement);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }, cancellationToken);

        return new AdjustInventoryResult.Success(
            new AdjustInventoryResponse(
                command.ProductId,
                item.AvailableQuantity,
                movement.Id,
                movement.Reason));
    }
}
