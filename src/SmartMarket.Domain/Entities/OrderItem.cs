namespace SmartMarket.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public Guid ProductId { get; set; }

    public string ProductNameSnapshot { get; set; } = string.Empty;

    public decimal UnitPriceSnapshot { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal { get; set; }

    public Order Order { get; set; } = null!;

    public Product Product { get; set; } = null!;
}
