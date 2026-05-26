using SmartMarket.IntegrationTests.Infrastructure;

namespace SmartMarket.IntegrationTests;

public class ApiFoundationTests(SmartMarketWebApplicationFactory factory) : IClassFixture<SmartMarketWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
    }
}
