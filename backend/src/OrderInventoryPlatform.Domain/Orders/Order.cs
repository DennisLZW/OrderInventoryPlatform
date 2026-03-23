using OrderInventoryPlatform.Domain.Common;

namespace OrderInventoryPlatform.Domain.Orders;

/// <summary>
/// Order aggregate root. Coordinates order lines and lifecycle before persistence / inventory commit.
/// </summary>
public sealed class Order
{
    // Backing field for EF Core (HasMany("_lines")); domain code uses AddLine.
    private List<OrderLine> _lines = new();

    public Guid Id { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public OrderStatus Status { get; private set; }

    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    /// <summary>Sum of line totals (derived).</summary>
    public decimal TotalAmount => _lines.Sum(l => l.LineTotal);

    private Order()
    {
    }

    public static Order Create()
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow,
            Status = OrderStatus.Pending
        };
    }

    /// <summary>Adds a line using catalog pricing snapshot (caller supplies unit price).</summary>
    public OrderLine AddLine(Guid productId, int quantity, decimal unitPriceSnapshot)
    {
        EnsurePending(nameof(AddLine));

        var line = OrderLine.Create(Id, productId, quantity, unitPriceSnapshot);
        _lines.Add(line);
        return line;
    }

    /// <summary>Marks the order as placed (e.g. after stock validation and persistence rules succeed).</summary>
    public void MarkAsPlaced()
    {
        EnsurePending(nameof(MarkAsPlaced));

        if (_lines.Count == 0)
        {
            throw new DomainException("Cannot place an order with no lines.");
        }

        Status = OrderStatus.Placed;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Cancelled)
        {
            return;
        }

        if (Status == OrderStatus.Placed)
        {
            throw new DomainException("Cannot cancel an order that has already been placed.");
        }

        Status = OrderStatus.Cancelled;
    }

    private void EnsurePending(string operation)
    {
        if (Status != OrderStatus.Pending)
        {
            throw new DomainException($"Cannot {operation} when order status is {Status}.");
        }
    }
}
