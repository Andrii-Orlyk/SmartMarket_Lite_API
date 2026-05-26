using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartMarket.Domain.Entities;

namespace SmartMarket.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders", table =>
        {
            table.HasCheckConstraint("CK_Orders_TotalAmount", "\"TotalAmount\" >= 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => x.OrderNumber)
            .IsUnique();

        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => x.CreatedAt);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
