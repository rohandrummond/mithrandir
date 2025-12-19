using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using mithrandir.Options;

namespace mithrandir.Tests;

public class RateLimitTestWebApplicationFactory : CustomWebApplicationFactory
{
    public FakeTimeProvider FakeTimeProvider { get; } = new FakeTimeProvider();
    public int TestRateLimit { get; set; } = 3;
    public int TestWindowMinutes { get; set; } = 10;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(services =>
        {
            // Swap in fake time provider
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TimeProvider));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddSingleton<TimeProvider>(FakeTimeProvider);

            // Update rate limits for testing
            services.Configure<RateLimitOptions>(options =>
            {
                options.FreeTierLimit = TestRateLimit;
                options.ProTierLimit = TestRateLimit;
                options.WindowMinutes = TestWindowMinutes;
            });
        });
    }
}
