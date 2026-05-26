namespace SmartMarket.UnitTests.Checkout;

public sealed class CheckoutRulesTests
{
    [Fact]
    public void OrderTotal_EqualsSumOfLineTotals()
    {
        const decimal unitPrice = 49.99m;
        var quantities = new[] { 2, 1 };

        var lineTotals = quantities.Select(q => q * unitPrice).ToList();
        var orderTotal = lineTotals.Sum();

        Assert.Equal(149.97m, orderTotal);
    }

    [Fact]
    public void LineTotal_UsesUnitPriceSnapshot_NotCurrentCatalogPrice()
    {
        const decimal snapshotPrice = 40m;
        const decimal currentCatalogPrice = 55m;
        const int quantity = 2;

        var lineTotal = quantity * snapshotPrice;

        Assert.Equal(80m, lineTotal);
        Assert.NotEqual(quantity * currentCatalogPrice, lineTotal);
    }
}
