using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartMarket.Domain.Entities;

namespace SmartMarket.Infrastructure.Persistence.Configurations;

public sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems", table =>
        {
            table.HasCheckConstraint("CK_CartItems_Quantity", "\"Quantity\" > 0");
            table.HasCheckConstraint("CK_CartItems_UnitPriceSnapshot", "\"UnitPriceSnapshot\" >= 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.UnitPriceSnapshot)
            .HasPrecision(18, 2);

        builder.HasIndex(x => new { x.CartId, x.ProductId })
            .IsUnique();

        builder.HasIndex(x => x.ProductId);
    }
}
