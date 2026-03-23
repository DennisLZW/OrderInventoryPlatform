using OrderInventoryPlatform.Domain.Common;

namespace OrderInventoryPlatform.Domain.Orders;

/// <summary>
/// Line item within an order. Unit price is a snapshot from catalog at the time the line was added.
/// </summary>
public sealed class OrderLine
{
    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public Guid ProductId { get; private set; }

    public int Quantity { get; private set; }

    /// <summary>Catalog unit price at the time of ordering (snapshot).</summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>Quantity × UnitPrice, rounded for the domain currency.</summary>
    public decimal LineTotal { get; private set; }

    private OrderLine()
    {
    }

    internal static OrderLine Create(Guid orderId, Guid productId, int quantity, decimal unitPrice)
    {
        if (orderId == Guid.Empty)
        {
            throw new DomainException("Order id is required.");
        }

        if (productId == Guid.Empty)
        {
            throw new DomainException("Product id is required.");
        }

        if (quantity <= 0)
        {
            throw new DomainException("Line quantity must be greater than zero.");
        }

        ValidateUnitPrice(unitPrice);

        var lineTotal = RoundMoney(quantity * unitPrice);

        return new OrderLine
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = decimal.Round(unitPrice, 2, MidpointRounding.AwayFromZero),
            LineTotal = lineTotal
        };
    }

    private static void ValidateUnitPrice(decimal unitPrice)
    {
        if (unitPrice < 0)
        {
            throw new DomainException("Unit price cannot be negative.");
        }
    }

    private static decimal RoundMoney(decimal value) =>
        decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
