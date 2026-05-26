using SmartMarket.Domain.Enums;

namespace SmartMarket.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public OrderStatus Status { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
