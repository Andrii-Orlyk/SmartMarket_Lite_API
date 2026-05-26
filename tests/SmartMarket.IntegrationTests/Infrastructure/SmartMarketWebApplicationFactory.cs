using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartMarket.Infrastructure.Persistence;

namespace SmartMarket.IntegrationTests.Infrastructure;

public sealed class SmartMarketWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"SmartMarketTests_{Guid.NewGuid()}";
    private readonly object _initLock = new();
    private bool _initialized;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<SmartMarketDbContext>>();
            services.RemoveAll<SmartMarketDbContext>();

            services.AddDbContext<SmartMarketDbContext>(options =>
                options
                    .UseInMemoryDatabase(_databaseName)
                    .ConfigureWarnings(warnings =>
                        warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
        });
    }

    public new HttpClient CreateClient()
    {
        EnsureDatabaseCreated();
        return base.CreateClient();
    }

    private void EnsureDatabaseCreated()
    {
        if (_initialized)
        {
            return;
        }

        lock (_initLock)
        {
            if (_initialized)
            {
                return;
            }

            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SmartMarketDbContext>();
            db.Database.EnsureCreated();
            _initialized = true;
        }
    }
}
