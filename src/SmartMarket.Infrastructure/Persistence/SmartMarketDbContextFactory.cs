using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SmartMarket.Infrastructure.Persistence;

public sealed class SmartMarketDbContextFactory : IDesignTimeDbContextFactory<SmartMarketDbContext>
{
    public SmartMarketDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SmartMarketDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=smartmarket;Username=postgres;Password=postgres");

        return new SmartMarketDbContext(optionsBuilder.Options);
    }
}
