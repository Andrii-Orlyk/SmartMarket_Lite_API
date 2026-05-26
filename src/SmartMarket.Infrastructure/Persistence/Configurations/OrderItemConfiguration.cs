using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartMarket.Domain.Entities;

namespace SmartMarket.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems", table =>
        {
            table.HasCheckConstraint("CK_OrderItems_Quantity", "\"Quantity\" > 0");
            table.HasCheckConstraint("CK_OrderItems_UnitPriceSnapshot", "\"UnitPriceSnapshot\" >= 0");
            table.HasCheckConstraint("CK_OrderItems_LineTotal", "\"LineTotal\" >= 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductNameSnapshot)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.UnitPriceSnapshot)
            .HasPrecision(18, 2);

        builder.Property(x => x.LineTotal)
            .HasPrecision(18, 2);

        builder.HasIndex(x => x.OrderId);

        builder.HasIndex(x => x.ProductId);
    }
}
