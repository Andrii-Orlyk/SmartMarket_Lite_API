using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SmartMarket.Domain.Entities;
using SmartMarket.Infrastructure.Persistence;

namespace SmartMarket.UnitTests.Persistence;

public sealed class PersistenceSchemaTests
{
    [Fact]
    public void Model_DefinesUniqueSkuAndOrderNumberIndexes()
    {
        using var context = CreateContext();
        var model = context.Model;

        var productSkuIndex = model.FindEntityType(typeof(Product))!
            .GetIndexes()
            .Single(index => index.IsUnique && index.Properties.Count == 1 && index.Properties[0].Name == nameof(Product.SKU));

        Assert.True(productSkuIndex.IsUnique);

        var orderNumberIndex = model.FindEntityType(typeof(Order))!
            .GetIndexes()
            .Single(index => index.IsUnique && index.Properties.Count == 1 && index.Properties[0].Name == nameof(Order.OrderNumber));

        Assert.True(orderNumberIndex.IsUnique);
    }

    [Fact]
    public void Model_DefinesDecimalPrecisionForMoneyFields()
    {
        using var context = CreateContext();
        var model = context.Model;

        AssertDecimalPrecision(model, typeof(Product), nameof(Product.Price));
        AssertDecimalPrecision(model, typeof(CartItem), nameof(CartItem.UnitPriceSnapshot));
        AssertDecimalPrecision(model, typeof(Order), nameof(Order.TotalAmount));
        AssertDecimalPrecision(model, typeof(OrderItem), nameof(OrderItem.UnitPriceSnapshot));
        AssertDecimalPrecision(model, typeof(OrderItem), nameof(OrderItem.LineTotal));
    }

    [Fact]
    public void Model_DefinesLookupIndexesForForeignKeys()
    {
        using var context = CreateContext();
        var model = context.Model;

        Assert.Contains(
            model.FindEntityType(typeof(Domain.Entities.Cart))!.GetIndexes(),
            index => index.Properties.Any(property => property.Name == nameof(Domain.Entities.Cart.UserId)));

        Assert.Contains(
            model.FindEntityType(typeof(CartItem))!.GetIndexes(),
            index => index.Properties.Any(property => property.Name == nameof(CartItem.ProductId)));

        Assert.Contains(
            model.FindEntityType(typeof(Order))!.GetIndexes(),
            index => index.Properties.Any(property => property.Name == nameof(Order.UserId)));

        Assert.Contains(
            model.FindEntityType(typeof(OrderItem))!.GetIndexes(),
            index => index.Properties.Any(property => property.Name == nameof(OrderItem.OrderId)));

        Assert.Contains(
            model.FindEntityType(typeof(OrderItem))!.GetIndexes(),
            index => index.Properties.Any(property => property.Name == nameof(OrderItem.ProductId)));
    }

    private static SmartMarketDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SmartMarketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new SmartMarketDbContext(options);
    }

    private static void AssertDecimalPrecision(IModel model, Type entityType, string propertyName)
    {
        var property = model.FindEntityType(entityType)!.FindProperty(propertyName)!;
        Assert.Equal(18, property.GetPrecision());
        Assert.Equal(2, property.GetScale());
    }
}
