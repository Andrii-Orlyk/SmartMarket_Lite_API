namespace SmartMarket.UnitTests;

public class FoundationTests
{
    [Fact]
    public void Solution_Loads_Application_And_Domain()
    {
        Assert.True(typeof(SmartMarket.Domain.Entities.User).IsClass);
        Assert.True(typeof(SmartMarket.Application.Common.Result).IsClass);
    }
}
